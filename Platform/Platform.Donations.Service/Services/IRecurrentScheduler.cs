using System.Threading.Tasks;

namespace Platform.Donations.Service.Services
{
    public interface IRecurrentScheduler
    {
        Task ProcessRecurrentDonationsAsync();
    }
}
