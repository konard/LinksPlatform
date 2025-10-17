using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Donations.Service.Models;

namespace Platform.Donations.Service.Services
{
    public interface IDonationService
    {
        Task<DonationModel> CreateDonationAsync(DonationModel donation);
        Task<DonationModel> GetDonationAsync(int id);
        Task<List<DonationModel>> GetDonationsByDonorAsync(string donorEmail);
        Task<List<DonationModel>> GetDonationsByRecipientAsync(string recipientName);
        Task<bool> CancelDonationAsync(int id);
        Task<List<DonationModel>> GetPendingRecurrentDonationsAsync();
    }
}
