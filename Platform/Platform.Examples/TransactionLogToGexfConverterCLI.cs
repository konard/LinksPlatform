using System;
using System.IO;
using Platform.IO;

namespace Platform.Examples
{
    public class TransactionLogToGexfConverterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var transactionLogFile = ConsoleHelpers.GetOrReadArgument(0, "Transaction log file", args);
            var exportTo = ConsoleHelpers.GetOrReadArgument(1, "Export to (GEXF file)", args);

            if (!File.Exists(transactionLogFile))
            {
                Console.WriteLine($"Transaction log file does not exist: {transactionLogFile}");
                return;
            }

            try
            {
                File.Create(exportTo).Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot create export file: {ex.Message}");
                return;
            }

            if (!File.Exists(exportTo))
            {
                Console.WriteLine("Export file cannot be created.");
                return;
            }

            try
            {
                var converter = new TransactionLogToGexfConverter(transactionLogFile);
                converter.Export(exportTo);
                Console.WriteLine($"Successfully exported transaction log to: {exportTo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during export: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
