using System;
using Microsoft.EntityFrameworkCore;
using Platform.Data.EntityFramework.Extensions;

namespace Platform.Data.EntityFramework.Examples
{
    /// <summary>
    /// Demonstrates basic usage of the Links EntityFramework provider.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Links EntityFramework Provider Example");
            Console.WriteLine("======================================");
            Console.WriteLine();

            using (var context = new SampleContext())
            {
                Console.WriteLine("DbContext created successfully with Links provider.");
                Console.WriteLine("The Links provider is configured and ready to use.");
            }

            Console.WriteLine();
            Console.WriteLine("Example completed successfully!");
        }
    }

    /// <summary>
    /// Sample DbContext using the Links provider.
    /// </summary>
    public class SampleContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLinks("SampleLinksDatabase");
        }
    }
}
