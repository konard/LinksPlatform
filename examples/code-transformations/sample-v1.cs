using System;
using Platform.Data.Core;

namespace MyApplication
{
    public class MyClass
    {
        private LinkStorage _storage;

        public MyClass()
        {
            _storage = new LinkStorage();
        }

        public void ProcessLinks()
        {
            var links = _storage.GetLinks();
            foreach (var link in links)
            {
                Console.WriteLine(link);
            }
        }

        public void AddNewLink()
        {
            _storage.CreateLink("Source", "Target");
        }
    }
}
