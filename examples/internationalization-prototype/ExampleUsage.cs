using System;
using System.Globalization;
using System.Threading;
using Platform.Data.Exceptions;

namespace Platform.Data.Examples
{
    /// <summary>
    /// Demonstrates how to use internationalized exception messages.
    /// </summary>
    public class InternationalizationExample
    {
        public static void Main()
        {
            Console.WriteLine("=== Platform.Data Internationalization Example ===\n");

            // Example 1: Default culture (English)
            Console.WriteLine("--- Example 1: Default Culture (English) ---");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            DemonstrateExceptions();

            Console.WriteLine();

            // Example 2: Russian culture
            Console.WriteLine("--- Example 2: Russian Culture (ru) ---");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru");
            DemonstrateExceptions();

            Console.WriteLine();

            // Example 3: Fallback to default culture (English) for unsupported languages
            Console.WriteLine("--- Example 3: Unsupported Culture (Fallback to English) ---");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr");
            DemonstrateExceptions();

            Console.WriteLine("\n=== End of Examples ===");
        }

        private static void DemonstrateExceptions()
        {
            // ArgumentLinkDoesNotExistsException without parameter name
            try
            {
                throw new ArgumentLinkDoesNotExistsException<int>(42);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"1. {ex.Message}");
            }

            // ArgumentLinkDoesNotExistsException with parameter name
            try
            {
                throw new ArgumentLinkDoesNotExistsException<int>(42, "linkId");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"2. {ex.Message}");
            }

            // ArgumentLinkHasDependenciesException without parameter name
            try
            {
                throw new ArgumentLinkHasDependenciesException<int>(123);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"3. {ex.Message}");
            }

            // ArgumentLinkHasDependenciesException with parameter name
            try
            {
                throw new ArgumentLinkHasDependenciesException<int>(123, "linkId");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"4. {ex.Message}");
            }

            // LinkWithSameValueAlreadyExistsException
            try
            {
                throw new LinkWithSameValueAlreadyExistsException();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"5. {ex.Message}");
            }

            // LinksLimitReachedException without limit value
            try
            {
                throw new LinksLimitReachedException<long>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"6. {ex.Message}");
            }

            // LinksLimitReachedException with limit value
            try
            {
                throw new LinksLimitReachedException<long>(1000000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"7. {ex.Message}");
            }
        }
    }
}
