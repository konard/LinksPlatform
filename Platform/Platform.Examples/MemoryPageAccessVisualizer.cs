using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Platform.Examples
{
    /// <summary>
    /// Visualizes memory page access patterns to help optimize database memory structure.
    /// Tracks which memory pages are accessed during operations to identify hotspots and access patterns.
    /// </summary>
    public class MemoryPageAccessVisualizer
    {
        private readonly int _pageSize;
        private readonly Dictionary<long, PageAccessInfo> _pageAccesses;
        private readonly object _lock = new object();
        private long _totalAccesses;

        public class PageAccessInfo
        {
            public long PageAddress { get; set; }
            public int AccessCount { get; set; }
            public DateTime FirstAccess { get; set; }
            public DateTime LastAccess { get; set; }
            public List<string> OperationTypes { get; set; } = new List<string>();
        }

        public MemoryPageAccessVisualizer()
        {
            _pageSize = GetSystemPageSize();
            _pageAccesses = new Dictionary<long, PageAccessInfo>();
        }

        /// <summary>
        /// Gets the system memory page size.
        /// </summary>
        private static int GetSystemPageSize()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var output = ExecuteCommand("getconf", "PAGESIZE");
                    if (int.TryParse(output.Trim(), out int pageSize))
                    {
                        return pageSize;
                    }
                }
                catch
                {
                    // Fall back to common default
                }
            }
            // Default page size for most systems
            return 4096;
        }

        /// <summary>
        /// Executes a shell command and returns the output.
        /// </summary>
        private static string ExecuteCommand(string command, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        /// <summary>
        /// Records an access to a memory address.
        /// </summary>
        public void RecordAccess(IntPtr address, string operationType = "unknown")
        {
            long addr = address.ToInt64();
            long pageAddress = (addr / _pageSize) * _pageSize;

            lock (_lock)
            {
                if (!_pageAccesses.ContainsKey(pageAddress))
                {
                    _pageAccesses[pageAddress] = new PageAccessInfo
                    {
                        PageAddress = pageAddress,
                        AccessCount = 0,
                        FirstAccess = DateTime.UtcNow
                    };
                }

                var info = _pageAccesses[pageAddress];
                info.AccessCount++;
                info.LastAccess = DateTime.UtcNow;

                if (!info.OperationTypes.Contains(operationType))
                {
                    info.OperationTypes.Add(operationType);
                }

                _totalAccesses++;
            }
        }

        /// <summary>
        /// Records accesses for a memory range.
        /// </summary>
        public void RecordRangeAccess(IntPtr startAddress, long size, string operationType = "unknown")
        {
            long start = startAddress.ToInt64();
            long end = start + size;
            long currentPage = (start / _pageSize) * _pageSize;

            while (currentPage <= end)
            {
                RecordAccess(new IntPtr(currentPage), operationType);
                currentPage += _pageSize;
            }
        }

        /// <summary>
        /// Gets current process memory information on Linux.
        /// </summary>
        public Dictionary<string, long> GetProcessMemoryInfo()
        {
            var info = new Dictionary<string, long>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    var statusPath = $"/proc/{Process.GetCurrentProcess().Id}/status";
                    if (File.Exists(statusPath))
                    {
                        var lines = File.ReadAllLines(statusPath);
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("VmSize:") || line.StartsWith("VmRSS:") ||
                                line.StartsWith("VmData:") || line.StartsWith("VmStk:") ||
                                line.StartsWith("VmPeak:") || line.StartsWith("VmHWM:"))
                            {
                                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 2 && long.TryParse(parts[1], out long value))
                                {
                                    info[parts[0].TrimEnd(':')] = value;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not read process memory info: {ex.Message}");
                }
            }

            return info;
        }

        /// <summary>
        /// Exports visualization data to CSV format.
        /// </summary>
        public void ExportToCSV(string filePath)
        {
            lock (_lock)
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("PageAddress,PageNumber,AccessCount,AccessFrequency,FirstAccess,LastAccess,OperationTypes");

                    var sortedPages = _pageAccesses.Values.OrderByDescending(p => p.AccessCount);

                    foreach (var page in sortedPages)
                    {
                        var frequency = _totalAccesses > 0 ? (double)page.AccessCount / _totalAccesses : 0;
                        var pageNumber = page.PageAddress / _pageSize;
                        var operations = string.Join(";", page.OperationTypes);

                        writer.WriteLine($"0x{page.PageAddress:X},{pageNumber},{page.AccessCount},{frequency:F6},{page.FirstAccess:O},{page.LastAccess:O},{operations}");
                    }
                }
            }
        }

        /// <summary>
        /// Generates a text-based heatmap visualization.
        /// </summary>
        public string GenerateHeatmap(int width = 80, int height = 20)
        {
            lock (_lock)
            {
                if (_pageAccesses.Count == 0)
                {
                    return "No page access data recorded.";
                }

                var sortedPages = _pageAccesses.Values.OrderBy(p => p.PageAddress).ToList();
                var maxAccess = sortedPages.Max(p => p.AccessCount);
                var minPage = sortedPages.First().PageAddress;
                var maxPage = sortedPages.Last().PageAddress;
                var pageRange = maxPage - minPage + _pageSize;

                var heatmap = new int[height, width];

                foreach (var page in sortedPages)
                {
                    var relativePos = (double)(page.PageAddress - minPage) / pageRange;
                    var x = (int)(relativePos * (width - 1));
                    var intensity = (int)((double)page.AccessCount / maxAccess * (height - 1));

                    for (int y = height - 1; y >= height - 1 - intensity && y >= 0; y--)
                    {
                        heatmap[y, x] = Math.Max(heatmap[y, x], page.AccessCount);
                    }
                }

                var result = new System.Text.StringBuilder();
                result.AppendLine($"Memory Page Access Heatmap (Page Size: {_pageSize} bytes)");
                result.AppendLine($"Total Pages: {_pageAccesses.Count}, Total Accesses: {_totalAccesses}");
                result.AppendLine($"Address Range: 0x{minPage:X} - 0x{maxPage:X}");
                result.AppendLine();

                var chars = new[] { ' ', '░', '▒', '▓', '█' };
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var value = heatmap[y, x];
                        var charIndex = value == 0 ? 0 : Math.Min((int)((double)value / maxAccess * (chars.Length - 1)) + 1, chars.Length - 1);
                        result.Append(chars[charIndex]);
                    }
                    result.AppendLine();
                }

                result.AppendLine();
                result.AppendLine("Legend: █ = High Access, ░ = Low Access,   = No Access");

                return result.ToString();
            }
        }

        /// <summary>
        /// Prints a summary report of page access statistics.
        /// </summary>
        public void PrintSummary()
        {
            lock (_lock)
            {
                Console.WriteLine("=== Memory Page Access Summary ===");
                Console.WriteLine($"Page Size: {_pageSize} bytes");
                Console.WriteLine($"Total Unique Pages Accessed: {_pageAccesses.Count}");
                Console.WriteLine($"Total Accesses: {_totalAccesses}");

                if (_pageAccesses.Count > 0)
                {
                    var avgAccesses = (double)_totalAccesses / _pageAccesses.Count;
                    var maxAccess = _pageAccesses.Values.Max(p => p.AccessCount);
                    var minAccess = _pageAccesses.Values.Min(p => p.AccessCount);

                    Console.WriteLine($"Average Accesses per Page: {avgAccesses:F2}");
                    Console.WriteLine($"Max Accesses (single page): {maxAccess}");
                    Console.WriteLine($"Min Accesses (single page): {minAccess}");
                    Console.WriteLine();

                    Console.WriteLine("Top 10 Most Accessed Pages:");
                    var topPages = _pageAccesses.Values
                        .OrderByDescending(p => p.AccessCount)
                        .Take(10);

                    foreach (var page in topPages)
                    {
                        var frequency = (double)page.AccessCount / _totalAccesses * 100;
                        var operations = string.Join(", ", page.OperationTypes);
                        Console.WriteLine($"  Page 0x{page.PageAddress:X16}: {page.AccessCount} accesses ({frequency:F2}%) - Operations: {operations}");
                    }
                }

                var memInfo = GetProcessMemoryInfo();
                if (memInfo.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Process Memory Info:");
                    foreach (var kvp in memInfo)
                    {
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value} kB");
                    }
                }
            }
        }

        /// <summary>
        /// Clears all recorded access data.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _pageAccesses.Clear();
                _totalAccesses = 0;
            }
        }

        /// <summary>
        /// Gets statistics about memory page access patterns.
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            lock (_lock)
            {
                var stats = new Dictionary<string, object>
                {
                    ["PageSize"] = _pageSize,
                    ["UniquePages"] = _pageAccesses.Count,
                    ["TotalAccesses"] = _totalAccesses,
                    ["AverageAccessesPerPage"] = _pageAccesses.Count > 0 ? (double)_totalAccesses / _pageAccesses.Count : 0,
                };

                if (_pageAccesses.Count > 0)
                {
                    stats["MaxAccessCount"] = _pageAccesses.Values.Max(p => p.AccessCount);
                    stats["MinAccessCount"] = _pageAccesses.Values.Min(p => p.AccessCount);
                }

                return stats;
            }
        }
    }
}
