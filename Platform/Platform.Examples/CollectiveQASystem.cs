using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Collective decision making and Q&A system.
    /// Instead of opinions, participants share facts and data that led them to conclusions.
    /// </summary>
    public class CollectiveQASystem
    {
        private readonly List<Question> _questions = new List<Question>();
        private readonly List<Participant> _participants = new List<Participant>();

        public IReadOnlyList<Question> Questions => _questions.AsReadOnly();
        public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

        /// <summary>
        /// Register a new participant in the system.
        /// </summary>
        public Participant RegisterParticipant(string name, string email)
        {
            var participant = new Participant(name, email);
            _participants.Add(participant);
            return participant;
        }

        /// <summary>
        /// Create a new question in the system.
        /// </summary>
        public Question CreateQuestion(string questionText, Participant author)
        {
            var question = new Question(questionText, author);
            _questions.Add(question);
            return question;
        }

        /// <summary>
        /// Add a fact to a question.
        /// </summary>
        public Fact AddFact(Question question, string description, string source, Participant contributor)
        {
            var fact = new Fact(description, source, contributor);
            question.AddFact(fact);
            return fact;
        }

        /// <summary>
        /// Add or update a conclusion based on existing facts.
        /// </summary>
        public Conclusion AddOrUpdateConclusion(Question question, string conclusionText, List<Fact> supportingFacts, Participant author)
        {
            var conclusion = new Conclusion(conclusionText, supportingFacts, author);
            question.SetConclusion(conclusion);
            return conclusion;
        }

        /// <summary>
        /// Challenge existing conclusion with a contradicting fact.
        /// This will notify all participants who contributed to the current conclusion.
        /// </summary>
        public void ChallengeConclusion(Question question, Fact contradictingFact, Participant challenger)
        {
            if (question.CurrentConclusion == null)
            {
                throw new InvalidOperationException("Cannot challenge a conclusion that doesn't exist.");
            }

            question.AddContradictingFact(contradictingFact);

            // Notify all participants who contributed to the current conclusion
            var participantsToNotify = new HashSet<Participant>();
            participantsToNotify.Add(question.CurrentConclusion.Author);

            foreach (var fact in question.CurrentConclusion.SupportingFacts)
            {
                participantsToNotify.Add(fact.Contributor);
            }

            foreach (var participant in participantsToNotify)
            {
                participant.Notify(new Notification(
                    $"Question '{question.QuestionText}' has a new contradicting fact.",
                    $"User {challenger.Name} added a contradicting fact: {contradictingFact.Description}",
                    DateTime.UtcNow,
                    question
                ));
            }
        }

        /// <summary>
        /// Vote on whether to accept a contradicting fact and update the conclusion.
        /// </summary>
        public void VoteOnContradictingFact(Question question, Fact contradictingFact, Participant voter, bool accept)
        {
            var vote = new Vote(voter, accept);
            contradictingFact.AddVote(vote);

            // Check if consensus is reached (all participants voted to accept)
            if (CheckConsensus(question, contradictingFact))
            {
                // Update conclusion to reflect the new fact
                NotifyConsensusReached(question, contradictingFact);
            }
        }

        private bool CheckConsensus(Question question, Fact contradictingFact)
        {
            if (question.CurrentConclusion == null) return false;

            // Get all participants involved in the current conclusion
            var involvedParticipants = new HashSet<Participant>();
            involvedParticipants.Add(question.CurrentConclusion.Author);

            foreach (var fact in question.CurrentConclusion.SupportingFacts)
            {
                involvedParticipants.Add(fact.Contributor);
            }

            // Check if all involved participants voted to accept
            var acceptVotes = new HashSet<Participant>(contradictingFact.Votes.Where(v => v.Accept).Select(v => v.Voter));
            return involvedParticipants.All(p => acceptVotes.Contains(p));
        }

        private void NotifyConsensusReached(Question question, Fact contradictingFact)
        {
            foreach (var participant in _participants)
            {
                participant.Notify(new Notification(
                    $"Consensus reached on question '{question.QuestionText}'",
                    $"The conclusion has been updated based on the new fact: {contradictingFact.Description}",
                    DateTime.UtcNow,
                    question
                ));
            }
        }

        /// <summary>
        /// Get all questions with their current conclusions.
        /// </summary>
        public List<QuestionSummary> GetQuestionsSummary()
        {
            return _questions.Select(q => new QuestionSummary
            {
                QuestionText = q.QuestionText,
                Author = q.Author.Name,
                FactsCount = q.Facts.Count,
                CurrentConclusion = q.CurrentConclusion?.ConclusionText,
                HasContradictingFacts = q.ContradictingFacts.Any()
            }).ToList();
        }
    }

    public class Question
    {
        private readonly List<Fact> _facts = new List<Fact>();
        private readonly List<Fact> _contradictingFacts = new List<Fact>();

        public string QuestionText { get; }
        public Participant Author { get; }
        public DateTime CreatedAt { get; }
        public Conclusion CurrentConclusion { get; private set; }
        public IReadOnlyList<Fact> Facts => _facts.AsReadOnly();
        public IReadOnlyList<Fact> ContradictingFacts => _contradictingFacts.AsReadOnly();

        public Question(string questionText, Participant author)
        {
            QuestionText = questionText;
            Author = author;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddFact(Fact fact)
        {
            _facts.Add(fact);
        }

        public void AddContradictingFact(Fact fact)
        {
            _contradictingFacts.Add(fact);
        }

        public void SetConclusion(Conclusion conclusion)
        {
            CurrentConclusion = conclusion;
        }
    }

    public class Fact
    {
        private readonly List<Vote> _votes = new List<Vote>();

        public string Description { get; }
        public string Source { get; }
        public Participant Contributor { get; }
        public DateTime AddedAt { get; }
        public IReadOnlyList<Vote> Votes => _votes.AsReadOnly();

        public Fact(string description, string source, Participant contributor)
        {
            Description = description;
            Source = source;
            Contributor = contributor;
            AddedAt = DateTime.UtcNow;
        }

        public void AddVote(Vote vote)
        {
            _votes.Add(vote);
        }
    }

    public class Conclusion
    {
        public string ConclusionText { get; }
        public List<Fact> SupportingFacts { get; }
        public Participant Author { get; }
        public DateTime CreatedAt { get; }

        public Conclusion(string conclusionText, List<Fact> supportingFacts, Participant author)
        {
            ConclusionText = conclusionText;
            SupportingFacts = supportingFacts ?? new List<Fact>();
            Author = author;
            CreatedAt = DateTime.UtcNow;
        }
    }

    public class Participant
    {
        private readonly List<Notification> _notifications = new List<Notification>();

        public string Name { get; }
        public string Email { get; }
        public IReadOnlyList<Notification> Notifications => _notifications.AsReadOnly();

        public Participant(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public void Notify(Notification notification)
        {
            _notifications.Add(notification);
        }
    }

    public class Notification
    {
        public string Title { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public Question RelatedQuestion { get; }

        public Notification(string title, string message, DateTime timestamp, Question relatedQuestion)
        {
            Title = title;
            Message = message;
            Timestamp = timestamp;
            RelatedQuestion = relatedQuestion;
        }
    }

    public class Vote
    {
        public Participant Voter { get; }
        public bool Accept { get; }
        public DateTime VotedAt { get; }

        public Vote(Participant voter, bool accept)
        {
            Voter = voter;
            Accept = accept;
            VotedAt = DateTime.UtcNow;
        }
    }

    public class QuestionSummary
    {
        public string QuestionText { get; set; }
        public string Author { get; set; }
        public int FactsCount { get; set; }
        public string CurrentConclusion { get; set; }
        public bool HasContradictingFacts { get; set; }
    }
}
