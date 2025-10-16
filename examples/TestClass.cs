using System;

namespace ExampleNamespace
{
    public class TestClass
    {
        private string _name;
        private int _count;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public void ProcessData(string input, bool validate)
        {
            if (validate && string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input cannot be null or empty");
            }
            _name = input;
        }

        public int Calculate(int a, int b)
        {
            return a + b;
        }

        public void Reset()
        {
            _name = string.Empty;
            _count = 0;
        }
    }
}
