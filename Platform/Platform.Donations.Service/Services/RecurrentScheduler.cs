using System;
using System.Threading.Tasks;
using Platform.Donations.Service.Models;

namespace Platform.Donations.Service.Services
{
    /// <summary>
    /// Scheduler for processing recurrent donations
    /// In production, this would be a background service or scheduled job
    /// </summary>
    public class RecurrentScheduler : IRecurrentScheduler
    {
        private readonly IDonationService _donationService;
        private readonly IPaymentService _paymentService;

        public RecurrentScheduler(IDonationService donationService, IPaymentService paymentService)
        {
            _donationService = donationService;
            _paymentService = paymentService;
        }

        public async Task ProcessRecurrentDonationsAsync()
        {
            var pendingDonations = await _donationService.GetPendingRecurrentDonationsAsync();

            foreach (var donation in pendingDonations)
            {
                try
                {
                    var result = await _paymentService.ProcessPaymentAsync(donation);

                    if (result.Success)
                    {
                        // Create a new donation record for this payment
                        var newDonation = new DonationModel
                        {
                            DonorName = donation.DonorName,
                            DonorEmail = donation.DonorEmail,
                            RecipientName = donation.RecipientName,
                            Amount = donation.Amount,
                            Currency = donation.Currency,
                            IsRecurrent = true,
                            RecurrenceType = donation.RecurrenceType,
                            PaymentMethod = donation.PaymentMethod,
                            PaymentTransactionId = result.TransactionId,
                            Status = DonationStatus.Completed,
                            Message = donation.Message
                        };

                        await _donationService.CreateDonationAsync(newDonation);

                        // Update next payment date
                        donation.NextPaymentDate = CalculateNextPaymentDate(donation.RecurrenceType.Value);
                    }
                    else
                    {
                        // Log failure, retry logic would go here
                        Console.WriteLine($"Recurrent donation {donation.Id} failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing recurrent donation {donation.Id}: {ex.Message}");
                }
            }
        }

        private DateTime CalculateNextPaymentDate(RecurrenceType recurrenceType)
        {
            var now = DateTime.UtcNow;

            switch (recurrenceType)
            {
                case RecurrenceType.Weekly:
                    return now.AddDays(7);

                case RecurrenceType.Monthly:
                    return now.AddMonths(1);

                case RecurrenceType.Yearly:
                    return now.AddYears(1);

                default:
                    return now;
            }
        }
    }
}
