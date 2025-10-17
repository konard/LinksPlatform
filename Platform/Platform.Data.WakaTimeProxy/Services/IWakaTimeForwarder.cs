using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Data.WakaTimeProxy.Models;

namespace Platform.Data.WakaTimeProxy
{
    public interface IWakaTimeForwarder
    {
        Task<bool> ForwardHeartbeatAsync(string user, Heartbeat heartbeat, string apiKey);
        Task<bool> ForwardHeartbeatsAsync(string user, IEnumerable<Heartbeat> heartbeats, string apiKey);
    }
}
