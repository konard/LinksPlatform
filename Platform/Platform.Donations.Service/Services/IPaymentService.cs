using System.Threading.Tasks;
using Platform.Donations.Service.Models;

namespace Platform.Donations.Service.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(DonationModel donation);
        Task<bool> ValidatePaymentMethodAsync(PaymentMethod method, string accountInfo);
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
