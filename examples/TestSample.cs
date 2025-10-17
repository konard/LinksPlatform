using System;
using System.Collections.Generic;

namespace Platform.Examples.TestSample
{
    /// <summary>
    /// Sample class for testing MethodImplInlining transformer
    /// </summary>
    public class TestSample
    {
        private int _value;

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Name { get; set; }

        public void SimpleMethod()
        {
            Console.WriteLine("Simple method");
        }

        public static void StaticMethod()
        {
            Console.WriteLine("Static method");
        }

        public virtual void VirtualMethod()
        {
            Console.WriteLine("Virtual method");
        }

        protected internal void ComplexAccessMethod()
        {
            Console.WriteLine("Complex access method");
        }

        public T GenericMethod<T>(T value)
        {
            return value;
        }

        public T GenericMethodWithConstraint<T>(T value) where T : class
        {
            return value;
        }

        public async System.Threading.Tasks.Task AsyncMethod()
        {
            await System.Threading.Tasks.Task.Delay(100);
        }

        public int ExpressionBodiedMethod() => 42;

        public int ExpressionBodiedProperty => _value * 2;
    }
}
