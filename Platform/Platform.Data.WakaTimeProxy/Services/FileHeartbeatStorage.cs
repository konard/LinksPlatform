using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platform.Data.WakaTimeProxy.Models;

namespace Platform.Data.WakaTimeProxy
{
    public class FileHeartbeatStorage : IHeartbeatStorage
    {
        private readonly string _storageDirectory;
        private readonly object _lockObject = new object();

        public FileHeartbeatStorage()
        {
            _storageDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WakaTimeProxy",
                "heartbeats"
            );

            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }

        public async Task StoreHeartbeatAsync(Heartbeat heartbeat)
        {
            await StoreHeartbeatsAsync(new[] { heartbeat });
        }

        public async Task StoreHeartbeatsAsync(IEnumerable<Heartbeat> heartbeats)
        {
            var heartbeatsList = heartbeats.ToList();
            if (!heartbeatsList.Any())
                return;

            var date = DateTimeOffset.FromUnixTimeSeconds((long)heartbeatsList.First().Time).ToString("yyyy-MM-dd");
            var filePath = GetFilePathForDate(date);

            var jsonLines = heartbeatsList.Select(h => JsonConvert.SerializeObject(h));

            lock (_lockObject)
            {
                File.AppendAllLines(filePath, jsonLines);
            }

            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Heartbeat>> GetHeartbeatsByDateAsync(string date)
        {
            var filePath = GetFilePathForDate(date);

            if (!File.Exists(filePath))
            {
                return Enumerable.Empty<Heartbeat>();
            }

            var lines = await File.ReadAllLinesAsync(filePath);
            return lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonConvert.DeserializeObject<Heartbeat>(line))
                .ToList();
        }

        private string GetFilePathForDate(string date)
        {
            return Path.Combine(_storageDirectory, $"{date}.jsonl");
        }
    }
}
