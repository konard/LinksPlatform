using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpSelfSendTest
{
    /// <summary>
    /// Test to verify if UDP socket can send to itself to interrupt blocking Receive() call.
    /// This tests the original issue #55 from 2012 on modern runtime environments.
    /// </summary>
    public class Program
    {
        private const int TestPort = 15555;
        private static volatile bool stopRequested = false;
        private static volatile bool receiveThreadExited = false;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== UDP Self-Send Stop Test ===");
            Console.WriteLine($"Testing on: {Environment.OSVersion}");
            Console.WriteLine($"Runtime: {Environment.Version}");
            Console.WriteLine();

            // Test 1: Original blocking approach (the one that failed on Mono)
            Console.WriteLine("Test 1: Blocking Receive with Self-Send Stop");
            TestBlockingWithSelfSend();
            Console.WriteLine();

            // Test 2: Current polling approach (the workaround)
            Console.WriteLine("Test 2: Polling with Available Check (Current Workaround)");
            TestPollingApproach();
            Console.WriteLine();

            Console.WriteLine("=== Tests Complete ===");
        }

        private static void TestBlockingWithSelfSend()
        {
            stopRequested = false;
            receiveThreadExited = false;
            UdpClient udp = null;
            Thread receiverThread = null;

            try
            {
                udp = new UdpClient(TestPort);

                receiverThread = new Thread(() => BlockingReceiver(udp));
                receiverThread.Start();

                // Wait a bit to ensure receiver is in blocking state
                Thread.Sleep(500);

                // Now try to stop by sending to self
                Console.WriteLine("  Attempting to stop by sending packet to self...");
                stopRequested = true;

                // This is the approach that failed on Mono in 2012
                udp.Connect(IPAddress.Loopback, TestPort);
                udp.Send(new byte[] { 0xFF }, 1); // Send stop signal

                // Wait for thread to exit
                bool exited = receiverThread.Join(TimeSpan.FromSeconds(3));

                if (exited)
                {
                    Console.WriteLine("  ✓ SUCCESS: Thread stopped gracefully");
                    Console.WriteLine("  The self-send approach WORKS on this runtime!");
                }
                else
                {
                    Console.WriteLine("  ✗ FAILED: Thread did not stop within timeout");
                    Console.WriteLine("  The self-send approach FAILS on this runtime (bug confirmed)");
                    Console.WriteLine("  NOTE: Skipping Test 2 to avoid hanging process");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ ERROR: {ex.Message}");
            }
            finally
            {
                udp?.Close();
            }
        }

        private static void BlockingReceiver(UdpClient udp)
        {
            try
            {
                while (!stopRequested)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udp.Receive(ref remoteEP); // Blocking call

                    if (data.Length > 0 && data[0] == 0xFF)
                    {
                        Console.WriteLine("  Received stop signal");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"  Received data: {Encoding.UTF8.GetString(data)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Receiver thread exception: {ex.GetType().Name}");
            }
            finally
            {
                receiveThreadExited = true;
            }
        }

        private static void TestPollingApproach()
        {
            stopRequested = false;
            receiveThreadExited = false;
            UdpClient udp = null;
            Thread receiverThread = null;

            try
            {
                udp = new UdpClient(TestPort + 1);

                receiverThread = new Thread(() => PollingReceiver(udp));
                receiverThread.Start();

                // Wait a bit
                Thread.Sleep(500);

                // Stop by setting flag
                Console.WriteLine("  Stopping by setting flag...");
                stopRequested = true;

                // Wait for thread to exit
                bool exited = receiverThread.Join(TimeSpan.FromSeconds(3));

                if (exited)
                {
                    Console.WriteLine("  ✓ SUCCESS: Thread stopped gracefully");
                    Console.WriteLine("  The polling approach works reliably");
                }
                else
                {
                    Console.WriteLine("  ✗ FAILED: Thread did not stop within timeout");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ ERROR: {ex.Message}");
            }
            finally
            {
                udp?.Close();
            }
        }

        private static void PollingReceiver(UdpClient udp)
        {
            try
            {
                while (!stopRequested)
                {
                    if (udp.Available > 0)
                    {
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = udp.Receive(ref remoteEP);
                        Console.WriteLine($"  Received data: {Encoding.UTF8.GetString(data)}");
                    }
                    else
                    {
                        Thread.Sleep(10); // Small sleep to avoid busy-waiting
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Receiver thread exception: {ex.GetType().Name}");
            }
            finally
            {
                receiveThreadExited = true;
            }
        }
    }
}
