using System.Collections.Generic;

namespace Platform.Data.TDDStackOverflow.Models
{
    /// <summary>
    /// Represents a language-agnostic test suite using CLI input/output
    /// </summary>
    public class TestSuite
    {
        public List<TestCase> TestCases { get; set; }

        public int TimeoutMilliseconds { get; set; }

        public TestSuite()
        {
            TestCases = new List<TestCase>();
            TimeoutMilliseconds = 5000; // Default 5 seconds
        }
    }

    /// <summary>
    /// Represents a single test case with input and expected output
    /// </summary>
    public class TestCase
    {
        public string Input { get; set; }

        public string ExpectedOutput { get; set; }

        public string Description { get; set; }
    }
}
