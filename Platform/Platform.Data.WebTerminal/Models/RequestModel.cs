using System;
using System.Collections.Generic;
using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal.Models
{
    /// <summary>
    /// Represents a request (question/query) in the knowledge base
    /// </summary>
    public class RequestModel
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsHidden { get; set; }
        public long? ParentRequestId { get; set; } // For nested requests (interpretations)
        public List<ResponseModel> Responses { get; set; }
        public Link Link { get; set; }

        public RequestModel()
        {
            Responses = new List<ResponseModel>();
            CreatedAt = DateTime.UtcNow;
            IsHidden = false;
        }

        public RequestModel(long id, string content) : this()
        {
            Id = id;
            Content = content;
        }
    }
}
