using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Platform.Donations.Service.Models
{
    public class RecipientModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Description { get; set; }

        public string Website { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsVerified { get; set; }

        public List<PaymentMethod> AcceptedPaymentMethods { get; set; } = new List<PaymentMethod>();

        public decimal TotalReceived { get; set; }

        public int DonorCount { get; set; }
    }
}
