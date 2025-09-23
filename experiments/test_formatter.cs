using System;
using Platform.Data.WebTerminal.Services;

class Program
{
    static void Main()
    {
        var formatter = new TextFormatterService();

        // Test the error formatting example from the issue
        var originalText = "The .NET Core SDK cannot be located: Error running dotnet --info: Error: Command failed: dotnet --info /bin/sh: 1: dotnet: not found /bin/sh: 1: dotnet: not found . .NET Core debugging will not be enabled. Make sure the .NET Core SDK is installed and is on the path.";

        Console.WriteLine("=== ORIGINAL TEXT ===");
        Console.WriteLine(originalText);
        Console.WriteLine();

        Console.WriteLine("=== FORMATTED TEXT ===");
        var formattedText = formatter.FormatErrorText(originalText);
        Console.WriteLine(formattedText);
        Console.WriteLine();

        // Test JSON formatting
        var jsonTest = @"{""error"": ""Some error message"", ""details"": ""More details""}";
        Console.WriteLine("=== JSON TEST ===");
        Console.WriteLine("Original:");
        Console.WriteLine(jsonTest);
        Console.WriteLine("Formatted:");
        Console.WriteLine(formatter.FormatJson(jsonTest));
        Console.WriteLine();

        Console.WriteLine("Test completed successfully!");
    }
}