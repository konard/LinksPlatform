using System;
using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal.Models
{
    /// <summary>
    /// Represents an upvote on a response (only upvotes allowed, one per user per request)
    /// </summary>
    public class VoteModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long RequestId { get; set; }
        public long ResponseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Link Link { get; set; }

        public VoteModel()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public VoteModel(long id, long userId, long requestId, long responseId) : this()
        {
            Id = id;
            UserId = userId;
            RequestId = requestId;
            ResponseId = responseId;
        }
    }
}
