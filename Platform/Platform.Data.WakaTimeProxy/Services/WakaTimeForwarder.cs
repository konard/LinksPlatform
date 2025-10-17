using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Platform.Data.WakaTimeProxy.Models;

namespace Platform.Data.WakaTimeProxy
{
    public class WakaTimeForwarder : IWakaTimeForwarder
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly bool _forwardingEnabled;

        public WakaTimeForwarder(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _forwardingEnabled = _configuration.GetValue<bool>("WakaTime:ForwardingEnabled", false);

            _httpClient.BaseAddress = new Uri("https://api.wakatime.com/");
        }

        public async Task<bool> ForwardHeartbeatAsync(string user, Heartbeat heartbeat, string apiKey)
        {
            if (!_forwardingEnabled || string.IsNullOrEmpty(apiKey))
                return false;

            return await ForwardHeartbeatsAsync(user, new[] { heartbeat }, apiKey);
        }

        public async Task<bool> ForwardHeartbeatsAsync(string user, IEnumerable<Heartbeat> heartbeats, string apiKey)
        {
            if (!_forwardingEnabled || string.IsNullOrEmpty(apiKey))
                return false;

            try
            {
                var heartbeatsList = heartbeats.ToList();
                var endpoint = heartbeatsList.Count > 1
                    ? $"api/v1/users/{user}/heartbeats.bulk"
                    : $"api/v1/users/{user}/heartbeats";

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:")));

                var content = heartbeatsList.Count > 1
                    ? JsonConvert.SerializeObject(heartbeatsList)
                    : JsonConvert.SerializeObject(heartbeatsList.First());

                request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Log error but don't fail the local storage
                return false;
            }
        }
    }
}
