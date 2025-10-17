using System;

namespace TestNamespace
{
    public class TestClass
    {
        private int _field;

        public int Property { get; set; }

        public void SimpleMethod()
        {
            Console.WriteLine("Test");
        }

        public static void StaticMethod()
        {
            Console.WriteLine("Static");
        }

        public T GenericMethod<T>(T value) where T : class
        {
            return value;
        }

        public int ExpressionMethod() => 42;
    }
}
