using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Data.WakaTimeProxy.Models;

namespace Platform.Data.WakaTimeProxy
{
    public interface IHeartbeatStorage
    {
        Task StoreHeartbeatAsync(Heartbeat heartbeat);
        Task StoreHeartbeatsAsync(IEnumerable<Heartbeat> heartbeats);
        Task<IEnumerable<Heartbeat>> GetHeartbeatsByDateAsync(string date);
    }
}
