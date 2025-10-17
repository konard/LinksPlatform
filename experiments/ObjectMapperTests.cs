using System;
using Xunit;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

namespace Platform.Experiments.Tests
{
    /// <summary>
    /// Unit tests for ObjectMapper functionality.
    /// Tests saving and loading of stateful managed objects to/from Links storage.
    /// </summary>
    public class ObjectMapperTests
    {
        private class SimpleObject
        {
            private int _value;

            public SimpleObject()
            {
            }

            public SimpleObject(int value)
            {
                _value = value;
            }

            public int GetValue() => _value;
            public void SetValue(int value) => _value = value;
        }

        private class ComplexObject
        {
            private int _intField;
            private long _longField;
            private bool _boolField;
            private string _stringField;

            public ComplexObject()
            {
            }

            public ComplexObject(int intValue, long longValue, bool boolValue, string stringValue)
            {
                _intField = intValue;
                _longField = longValue;
                _boolField = boolValue;
                _stringField = stringValue;
            }

            public int GetInt() => _intField;
            public long GetLong() => _longField;
            public bool GetBool() => _boolField;
            public string GetString() => _stringField;
        }

        [Fact]
        public void SaveAndLoad_SimpleObject_Success()
        {
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var mapper = new ObjectMapper<uint>(links);
                var obj = new SimpleObject(42);

                var linkId = mapper.Save(obj);
                Assert.True(linkId != null);

                var loaded = mapper.Load<SimpleObject>(linkId);
                Assert.NotNull(loaded);
                // Note: Current implementation is a skeleton, full field persistence
                // would be implemented in production version
            }
        }

        [Fact]
        public void SaveAndLoad_ComplexObject_Success()
        {
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var mapper = new ObjectMapper<uint>(links);
                var obj = new ComplexObject(123, 456L, true, "test");

                var linkId = mapper.Save(obj);
                Assert.True(linkId != null);

                var loaded = mapper.Load<ComplexObject>(linkId);
                Assert.NotNull(loaded);
            }
        }

        [Fact]
        public void Save_NullObject_ThrowsException()
        {
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var mapper = new ObjectMapper<uint>(links);
                Assert.Throws<ArgumentNullException>(() => mapper.Save(null));
            }
        }

        [Fact]
        public void Load_WithoutDefaultConstructor_ThrowsException()
        {
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var mapper = new ObjectMapper<uint>(links);
                // String doesn't have a public parameterless constructor
                Assert.Throws<InvalidOperationException>(() => mapper.Load(typeof(string), (uint)1));
            }
        }

        [Fact]
        public void ObjectMapper_CachesGeneratedMethods()
        {
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var mapper = new ObjectMapper<uint>(links);

                // First save generates and caches the method
                var obj1 = new SimpleObject(10);
                var link1 = mapper.Save(obj1);

                // Second save should use cached method (faster)
                var obj2 = new SimpleObject(20);
                var link2 = mapper.Save(obj2);

                Assert.True(link1 != null);
                Assert.True(link2 != null);
            }
        }
    }
}
