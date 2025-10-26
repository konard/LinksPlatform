using System;

namespace LinksPlatform.NeedsOffersSearch
{
    /// <summary>
    /// Type of listing
    /// </summary>
    public enum ListingType
    {
        /// <summary>
        /// User needs something
        /// </summary>
        Need,

        /// <summary>
        /// User offers something
        /// </summary>
        Offer
    }

    /// <summary>
    /// Represents a need or offer registered in the system
    /// </summary>
    public class NeedOffer
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User identifier who created this listing
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Type of listing (Need or Offer)
        /// </summary>
        public ListingType Type { get; set; }

        /// <summary>
        /// Description of the need or offer
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Keywords or tags for better matching
        /// </summary>
        public string[] Keywords { get; set; }

        /// <summary>
        /// When this listing was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this listing was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Whether this listing is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether search should be performed for this listing
        /// </summary>
        public bool EnableSearch { get; set; }

        /// <summary>
        /// Budget for search operations (in local currency)
        /// </summary>
        public decimal SearchBudget { get; set; }

        /// <summary>
        /// Contact information for this listing
        /// </summary>
        public string ContactInfo { get; set; }

        public NeedOffer()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
            EnableSearch = false;
            Keywords = Array.Empty<string>();
        }
    }
}
