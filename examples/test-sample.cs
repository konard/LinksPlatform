// Sample code snippet for testing the Bug Fixing by Analogy tool
using System;
using System.IO;

public class FileProcessor
{
    public void SaveData(string path, string data)
    {
        File.WriteAllText(path, data);
    }

    public string LoadData(string path)
    {
        return File.ReadAllText(path);
    }
}
