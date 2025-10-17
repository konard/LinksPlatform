using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.WakaTimeProxy.Models;

namespace Platform.Data.WakaTimeProxy.Controllers
{
    [Route("api/v1/users/{user}")]
    [ApiController]
    public class HeartbeatsController : ControllerBase
    {
        private readonly IHeartbeatStorage _storage;
        private readonly IWakaTimeForwarder _forwarder;

        public HeartbeatsController(IHeartbeatStorage storage, IWakaTimeForwarder forwarder)
        {
            _storage = storage;
            _forwarder = forwarder;
        }

        // POST api/v1/users/{user}/heartbeats
        [HttpPost("heartbeats")]
        public async Task<IActionResult> PostHeartbeat(string user, [FromBody] Heartbeat heartbeat)
        {
            if (heartbeat == null)
                return BadRequest("Invalid heartbeat data");

            // Store locally first (this is the priority according to the issue)
            await _storage.StoreHeartbeatAsync(heartbeat);

            // Try to forward to WakaTime API (optional)
            var apiKey = Request.Headers["Authorization"].ToString().Replace("Basic ", "");
            _ = Task.Run(async () => await _forwarder.ForwardHeartbeatAsync(user, heartbeat, apiKey));

            return Ok(new { success = true });
        }

        // POST api/v1/users/{user}/heartbeats.bulk
        [HttpPost("heartbeats.bulk")]
        public async Task<IActionResult> PostHeartbeatsBulk(string user, [FromBody] List<Heartbeat> heartbeats)
        {
            if (heartbeats == null || heartbeats.Count == 0)
                return BadRequest("Invalid heartbeat data");

            // Store locally first (this is the priority according to the issue)
            await _storage.StoreHeartbeatsAsync(heartbeats);

            // Try to forward to WakaTime API (optional)
            var apiKey = Request.Headers["Authorization"].ToString().Replace("Basic ", "");
            _ = Task.Run(async () => await _forwarder.ForwardHeartbeatsAsync(user, heartbeats, apiKey));

            return Ok(new { success = true });
        }

        // GET api/v1/users/{user}/heartbeats?date=yyyy-MM-dd
        [HttpGet("heartbeats")]
        public async Task<IActionResult> GetHeartbeats(string user, [FromQuery] string date)
        {
            if (string.IsNullOrEmpty(date))
                return BadRequest("Date parameter is required");

            var heartbeats = await _storage.GetHeartbeatsByDateAsync(date);
            return Ok(new { data = heartbeats });
        }
    }
}
