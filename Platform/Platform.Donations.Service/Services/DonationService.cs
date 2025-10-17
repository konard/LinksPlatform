using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platform.Donations.Service.Models;

namespace Platform.Donations.Service.Services
{
    public class DonationService : IDonationService
    {
        // In-memory storage for the MVP
        // In production, this would be replaced with a proper database (SQL Server, PostgreSQL, etc.)
        private static readonly List<DonationModel> _donations = new List<DonationModel>();
        private static int _nextId = 1;
        private static readonly object _lock = new object();

        public async Task<DonationModel> CreateDonationAsync(DonationModel donation)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                donation.Id = _nextId++;
                donation.CreatedAt = DateTime.UtcNow;

                if (donation.IsRecurrent && donation.RecurrenceType.HasValue)
                {
                    donation.NextPaymentDate = CalculateNextPaymentDate(donation.RecurrenceType.Value);
                    donation.Status = DonationStatus.Scheduled;
                }

                _donations.Add(donation);
                return donation;
            }
        }

        public async Task<DonationModel> GetDonationAsync(int id)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                return _donations.FirstOrDefault(d => d.Id == id);
            }
        }

        public async Task<List<DonationModel>> GetDonationsByDonorAsync(string donorEmail)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                return _donations
                    .Where(d => d.DonorEmail.Equals(donorEmail, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        public async Task<List<DonationModel>> GetDonationsByRecipientAsync(string recipientName)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                return _donations
                    .Where(d => d.RecipientName.Equals(recipientName, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
            }
        }

        public async Task<bool> CancelDonationAsync(int id)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                var donation = _donations.FirstOrDefault(d => d.Id == id);
                if (donation != null && donation.Status == DonationStatus.Scheduled)
                {
                    donation.Status = DonationStatus.Cancelled;
                    return true;
                }
                return false;
            }
        }

        public async Task<List<DonationModel>> GetPendingRecurrentDonationsAsync()
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                return _donations
                    .Where(d => d.IsRecurrent &&
                               d.Status == DonationStatus.Scheduled &&
                               d.NextPaymentDate.HasValue &&
                               d.NextPaymentDate.Value <= DateTime.UtcNow)
                    .ToList();
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
