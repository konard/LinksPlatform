using System;
using System.Net.Sockets;
using Platform.Exceptions;
using Platform.Threading;
using Platform.IO;
using Platform.Communication.Protocol.Udp;

namespace Platform.Examples
{
    public class TerminalCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            try
            {
                using (var cancellation = new ConsoleCancellation())
                using (var receiver = new UdpClient(8888))
                using (var sender = new UdpSender(7777))
                {
                    Console.WriteLine("Welcome to terminal.");
                    Console.WriteLine("Press CTRL+C or enter empty line to stop terminal.");

                    // Read input in a separate thread to avoid blocking
                    var inputThread = new System.Threading.Thread(() =>
                    {
                        try
                        {
                            while (cancellation.NotRequested)
                            {
                                var line = Console.ReadLine();
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    sender.Send(line);
                                }
                                else
                                {
                                    cancellation.ForceCancellation();
                                    break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Thread is being terminated, ignore
                        }
                    })
                    {
                        IsBackground = true
                    };
                    inputThread.Start();

                    while (cancellation.NotRequested)
                    {
                        while (receiver.Available > 0)
                        {
                            var message = receiver.ReceiveString();
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                Console.WriteLine($"<- {message}");
                            }
                        }
                        ThreadHelpers.Sleep();
                    }

                    // Wait for input thread to finish
                    if (inputThread.IsAlive)
                    {
                        inputThread.Join(1000);
                    }
                    Console.WriteLine("Terminal stopped.");
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToStringWithAllInnerExceptions());
            }
        }
    }
}
