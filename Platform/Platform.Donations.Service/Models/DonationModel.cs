using System;
using System.ComponentModel.DataAnnotations;

namespace Platform.Donations.Service.Models
{
    public class DonationModel
    {
        public int Id { get; set; }

        [Required]
        public string DonorName { get; set; }

        [Required]
        [EmailAddress]
        public string DonorEmail { get; set; }

        [Required]
        public string RecipientName { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "RUB";

        public bool IsRecurrent { get; set; }

        public RecurrenceType? RecurrenceType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? NextPaymentDate { get; set; }

        public DonationStatus Status { get; set; } = DonationStatus.Pending;

        public PaymentMethod PaymentMethod { get; set; }

        public string PaymentTransactionId { get; set; }

        public string Message { get; set; }
    }

    public enum RecurrenceType
    {
        None = 0,
        Weekly = 1,
        Monthly = 2,
        Yearly = 3
    }

    public enum DonationStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3,
        Scheduled = 4
    }

    public enum PaymentMethod
    {
        Mir = 1,
        YooMoney = 2,
        QIWI = 3,
        WebMoney = 4,
        BankTransfer = 5
    }
}
