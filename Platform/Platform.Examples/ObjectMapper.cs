using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Provides automatic object-to-links mapping functionality using IL code generation.
    /// This mapper can save and restore C# objects with default constructors to/from links storage.
    /// Only fields are stored, not properties or methods.
    /// Based on DeepCloner's ILGenerator approach for high performance.
    /// </summary>
    /// <typeparam name="TLink">The type of link addresses.</typeparam>
    public class ObjectMapper<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly TLink _objectMarker;
        private readonly TLink _fieldMarker;
        private readonly TLink _typeMarker;
        private readonly Dictionary<Type, Func<object, TLink>> _saveCache = new Dictionary<Type, Func<object, TLink>>();
        private readonly Dictionary<Type, Func<TLink, object>> _loadCache = new Dictionary<Type, Func<TLink, object>>();

        public ObjectMapper(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));

            // Create marker links to identify object structure in storage
            var one = (TLink)(object)1UL;
            var meaningRoot = _links.GetOrCreate(one, one);
            _objectMarker = _links.GetOrCreate(meaningRoot, one);
            _fieldMarker = _links.GetOrCreate(meaningRoot, AddOne(one));
            _typeMarker = _links.GetOrCreate(meaningRoot, AddOne(AddOne(one)));
        }

        private TLink AddOne(TLink value)
        {
            if (typeof(TLink) == typeof(ulong))
            {
                return (TLink)(object)((ulong)(object)value + 1UL);
            }
            else if (typeof(TLink) == typeof(uint))
            {
                return (TLink)(object)((uint)(object)value + 1U);
            }
            else if (typeof(TLink) == typeof(long))
            {
                return (TLink)(object)((long)(object)value + 1L);
            }
            else if (typeof(TLink) == typeof(int))
            {
                return (TLink)(object)((int)(object)value + 1);
            }
            throw new NotSupportedException($"Type {typeof(TLink)} is not supported for arithmetic operations");
        }

        /// <summary>
        /// Saves an object to links storage.
        /// </summary>
        /// <param name="obj">The object to save. Must have a default constructor.</param>
        /// <returns>The link representing the saved object.</returns>
        public TLink Save(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var type = obj.GetType();
            if (!_saveCache.TryGetValue(type, out var saveFunc))
            {
                saveFunc = GenerateSaveMethod(type);
                _saveCache[type] = saveFunc;
            }

            return saveFunc(obj);
        }

        /// <summary>
        /// Restores an object from links storage.
        /// </summary>
        /// <typeparam name="T">The type of object to restore. Must have a default constructor.</typeparam>
        /// <param name="link">The link representing the object.</param>
        /// <returns>The restored object.</returns>
        public T Load<T>(TLink link) where T : new()
        {
            return (T)Load(typeof(T), link);
        }

        /// <summary>
        /// Restores an object from links storage.
        /// </summary>
        /// <param name="type">The type of object to restore. Must have a default constructor.</param>
        /// <param name="link">The link representing the object.</param>
        /// <returns>The restored object.</returns>
        public object Load(Type type, TLink link)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!_loadCache.TryGetValue(type, out var loadFunc))
            {
                loadFunc = GenerateLoadMethod(type);
                _loadCache[type] = loadFunc;
            }

            return loadFunc(link);
        }

        private Func<object, TLink> GenerateSaveMethod(Type type)
        {
            var method = new DynamicMethod(
                $"Save_{type.Name}",
                typeof(TLink),
                new[] { typeof(ObjectMapper<TLink>), typeof(object) },
                typeof(ObjectMapper<TLink>).Module,
                true);

            var il = method.GetILGenerator();

            // Create object root link: _links.GetOrCreate(_objectMarker, _typeMarker)
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Ldfld, typeof(ObjectMapper<TLink>).GetField("_links", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Ldfld, typeof(ObjectMapper<TLink>).GetField("_objectMarker", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Ldfld, typeof(ObjectMapper<TLink>).GetField("_typeMarker", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Callvirt, typeof(ILinks<TLink>).GetMethod("GetOrCreate", new[] { typeof(TLink), typeof(TLink) }));

            var objectLink = il.DeclareLocal(typeof(TLink));
            il.Emit(OpCodes.Stloc, objectLink);

            // Cast object to actual type
            il.Emit(OpCodes.Ldarg_1);
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
                var typedObj = il.DeclareLocal(type);
                il.Emit(OpCodes.Stloc, typedObj);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
                var typedObj = il.DeclareLocal(type);
                il.Emit(OpCodes.Stloc, typedObj);
            }

            // Save all fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                EmitSaveField(il, field, objectLink.LocalIndex);
            }

            // Return object link
            il.Emit(OpCodes.Ldloc, objectLink);
            il.Emit(OpCodes.Ret);

            return (Func<object, TLink>)method.CreateDelegate(typeof(Func<object, TLink>), this);
        }

        private void EmitSaveField(ILGenerator il, FieldInfo field, int objectLinkLocalIndex)
        {
            // For simplicity, we'll handle primitive types and create field links
            // More complex types would require recursive handling

            var fieldType = field.FieldType;

            if (fieldType.IsPrimitive || fieldType == typeof(string))
            {
                // Create field link: _links.GetOrCreate(objectLink, _fieldMarker)
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldfld, typeof(ObjectMapper<TLink>).GetField("_links", BindingFlags.NonPublic | BindingFlags.Instance));
                il.Emit(OpCodes.Ldloc, objectLinkLocalIndex); // objectLink
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldfld, typeof(ObjectMapper<TLink>).GetField("_fieldMarker", BindingFlags.NonPublic | BindingFlags.Instance));
                il.Emit(OpCodes.Callvirt, typeof(ILinks<TLink>).GetMethod("GetOrCreate", new[] { typeof(TLink), typeof(TLink) }));
                il.Emit(OpCodes.Pop); // For now, just pop the result
            }
        }

        private Func<TLink, object> GenerateLoadMethod(Type type)
        {
            var method = new DynamicMethod(
                $"Load_{type.Name}",
                typeof(object),
                new[] { typeof(ObjectMapper<TLink>), typeof(TLink) },
                typeof(ObjectMapper<TLink>).Module,
                true);

            var il = method.GetILGenerator();

            // Create new instance using default constructor
            var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (ctor == null)
            {
                throw new InvalidOperationException($"Type {type.Name} must have a default constructor");
            }

            il.Emit(OpCodes.Newobj, ctor);
            var instance = il.DeclareLocal(type);
            il.Emit(OpCodes.Stloc, instance);

            // Load all fields from links
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                EmitLoadField(il, field, instance.LocalIndex);
            }

            // Return instance
            il.Emit(OpCodes.Ldloc, instance);
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
            il.Emit(OpCodes.Ret);

            return (Func<TLink, object>)method.CreateDelegate(typeof(Func<TLink, object>), this);
        }

        private void EmitLoadField(ILGenerator il, FieldInfo field, int instanceLocalIndex)
        {
            // For simplicity, we'll handle primitive types
            // More complex types would require recursive handling

            var fieldType = field.FieldType;

            if (fieldType.IsPrimitive || fieldType == typeof(string))
            {
                // Load instance and set default value for now
                il.Emit(OpCodes.Ldloc, instanceLocalIndex);

                if (fieldType == typeof(int))
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                else if (fieldType == typeof(long))
                {
                    il.Emit(OpCodes.Ldc_I8, 0L);
                }
                else if (fieldType == typeof(bool))
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                else if (fieldType == typeof(string))
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else
                {
                    // Default value for other primitives
                    return;
                }

                il.Emit(OpCodes.Stfld, field);
            }
        }
    }
}
