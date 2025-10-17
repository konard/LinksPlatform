using System;
using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal.Models
{
    /// <summary>
    /// Represents an edit to a response (for tracking and reverting)
    /// </summary>
    public class EditHistoryModel
    {
        public long Id { get; set; }
        public long ResponseId { get; set; }
        public string PreviousContent { get; set; }
        public string NewContent { get; set; }
        public DateTime EditedAt { get; set; }
        public long EditedByUserId { get; set; }
        public string EditReason { get; set; }
        public Link Link { get; set; }

        public EditHistoryModel()
        {
            EditedAt = DateTime.UtcNow;
        }

        public EditHistoryModel(long id, long responseId, string previousContent, string newContent, long userId) : this()
        {
            Id = id;
            ResponseId = responseId;
            PreviousContent = previousContent;
            NewContent = newContent;
            EditedByUserId = userId;
        }
    }
}
