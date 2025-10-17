using System;
using System.Collections.Generic;
using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal.Models
{
    /// <summary>
    /// Represents a response (answer) to a request in the knowledge base
    /// </summary>
    public class ResponseModel
    {
        public long Id { get; set; }
        public long RequestId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastEditedAt { get; set; }
        public bool IsHidden { get; set; }
        public int VoteCount { get; set; }
        public List<ReferenceModel> References { get; set; }
        public List<EditHistoryModel> EditHistory { get; set; }
        public Link Link { get; set; }

        public ResponseModel()
        {
            References = new List<ReferenceModel>();
            EditHistory = new List<EditHistoryModel>();
            CreatedAt = DateTime.UtcNow;
            IsHidden = false;
            VoteCount = 0;
        }

        public ResponseModel(long id, long requestId, string content) : this()
        {
            Id = id;
            RequestId = requestId;
            Content = content;
        }
    }
}
