using System;
using System.Collections.Generic;

namespace Platform.Data.TDDStackOverflow.Models
{
    /// <summary>
    /// Represents a question with automated test suite
    /// </summary>
    public class Question
    {
        public ulong Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public TestSuite TestSuite { get; set; }

        public bool IsFrozen { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? FrozenAt { get; set; }

        public List<Solution> Solutions { get; set; }

        public List<ulong> NarrowerThanLinks { get; set; }

        public List<ulong> BroaderThanLinks { get; set; }

        public Question()
        {
            Solutions = new List<Solution>();
            NarrowerThanLinks = new List<ulong>();
            BroaderThanLinks = new List<ulong>();
            CreatedAt = DateTime.UtcNow;
        }

        public void Freeze()
        {
            if (!IsFrozen)
            {
                IsFrozen = true;
                FrozenAt = DateTime.UtcNow;
            }
        }
    }
}
