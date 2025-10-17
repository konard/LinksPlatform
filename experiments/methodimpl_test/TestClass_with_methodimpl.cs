using System;
using System.Runtime.CompilerServices;

namespace TestNamespace
{
    public class TestClass
    {
        private int _field;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int Property { get; set; }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SimpleMethod()
        {
            Console.WriteLine("Test");
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void StaticMethod()
        {
            Console.WriteLine("Static");
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public T GenericMethod<T>(T value) where T : class
        {
            return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int ExpressionMethod() => 42;
    }
}
