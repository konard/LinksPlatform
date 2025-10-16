using System;
using Platform.Examples;

namespace Platform.Sandbox
{
    public class ProjectExecutorApp
    {
        public static void Main(string[] args)
        {
            new ProjectExecutorCLI().Run(args);
        }
    }
}
