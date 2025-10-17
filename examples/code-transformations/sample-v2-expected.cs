using System;
using Platform.Data;

namespace MyApplication
{
    public class MyClass
    {
        private Links _storage;

        public MyClass()
        {
            _storage = new Links();
        }

        public void ProcessLinks()
        {
            var links = _storage.GetAll();
            foreach (var link in links)
            {
                Console.WriteLine(link);
            }
        }

        public void AddNewLink()
        {
            _storage.Create("Source", "Target");
        }
    }
}
