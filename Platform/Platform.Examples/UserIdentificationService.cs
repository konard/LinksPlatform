using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Singletons;

namespace Platform.Examples
{
    /// <summary>
    /// Service for identifying real users based on connected accounts and reputation data.
    /// Provides scoring mechanism to determine the probability that a user is real.
    /// </summary>
    /// <typeparam name="TLink">The type of link address.</typeparam>
    public class UserIdentificationService<TLink>
    {
        private static readonly LinksConstants<TLink> _constants = Default<LinksConstants<TLink>>.Instance;
        private readonly ILinks<TLink> _links;
        private readonly Dictionary<TLink, UserProfile> _userProfiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserIdentificationService{TLink}"/> class.
        /// </summary>
        /// <param name="links">The links storage for persisting user data.</param>
        public UserIdentificationService(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _userProfiles = new Dictionary<TLink, UserProfile>();
        }

        /// <summary>
        /// Represents a user profile with multiple connected service accounts.
        /// </summary>
        public class UserProfile
        {
            /// <summary>
            /// Gets or sets the unique identifier for the user profile.
            /// </summary>
            public TLink UserId { get; set; }

            /// <summary>
            /// Gets or sets the list of connected service accounts.
            /// </summary>
            public List<ServiceAccount> ConnectedAccounts { get; set; } = new List<ServiceAccount>();

            /// <summary>
            /// Gets or sets the overall reputation score across all services.
            /// </summary>
            public double TotalReputation { get; set; }

            /// <summary>
            /// Gets or sets the date when the first account was created.
            /// </summary>
            public DateTime EarliestAccountCreation { get; set; }
        }

        /// <summary>
        /// Represents a connected service account (e.g., GitHub, Twitter, Stack Overflow).
        /// </summary>
        public class ServiceAccount
        {
            /// <summary>
            /// Gets or sets the name of the service (e.g., "GitHub", "Twitter").
            /// </summary>
            public string ServiceName { get; set; }

            /// <summary>
            /// Gets or sets the account identifier on the service.
            /// </summary>
            public string AccountId { get; set; }

            /// <summary>
            /// Gets or sets the reputation or score on this service.
            /// </summary>
            public double Reputation { get; set; }

            /// <summary>
            /// Gets or sets the date when the account was created.
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the account is verified.
            /// </summary>
            public bool IsVerified { get; set; }
        }

        /// <summary>
        /// Creates a new user profile.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>The created user profile.</returns>
        public UserProfile CreateUserProfile(TLink userId)
        {
            if (_userProfiles.ContainsKey(userId))
            {
                throw new InvalidOperationException($"User profile with ID {userId} already exists.");
            }

            var profile = new UserProfile
            {
                UserId = userId,
                ConnectedAccounts = new List<ServiceAccount>(),
                TotalReputation = 0,
                EarliestAccountCreation = DateTime.MaxValue
            };

            _userProfiles[userId] = profile;
            return profile;
        }

        /// <summary>
        /// Adds a connected service account to a user profile.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="account">The service account to add.</param>
        public void AddServiceAccount(TLink userId, ServiceAccount account)
        {
            if (!_userProfiles.TryGetValue(userId, out var profile))
            {
                throw new KeyNotFoundException($"User profile with ID {userId} not found.");
            }

            profile.ConnectedAccounts.Add(account);
            profile.TotalReputation += account.Reputation;

            if (account.CreatedAt < profile.EarliestAccountCreation)
            {
                profile.EarliestAccountCreation = account.CreatedAt;
            }
        }

        /// <summary>
        /// Calculates the "realness" score for a user based on their profile data.
        /// Returns a probability value between 0 and 1, where 1 indicates highest confidence the user is real.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A score between 0 and 1 indicating the probability the user is real.</returns>
        public double CalculateRealnessScore(TLink userId)
        {
            if (!_userProfiles.TryGetValue(userId, out var profile))
            {
                throw new KeyNotFoundException($"User profile with ID {userId} not found.");
            }

            if (profile.ConnectedAccounts.Count == 0)
            {
                return 0.0; // No accounts means no way to verify
            }

            // Calculate component scores
            var accountCountScore = CalculateAccountCountScore(profile.ConnectedAccounts.Count);
            var reputationScore = CalculateReputationScore(profile.TotalReputation);
            var ageScore = CalculateAccountAgeScore(profile.EarliestAccountCreation);
            var verificationScore = CalculateVerificationScore(profile.ConnectedAccounts);

            // Weighted average of all factors
            var totalScore = (accountCountScore * 0.25) +
                            (reputationScore * 0.30) +
                            (ageScore * 0.25) +
                            (verificationScore * 0.20);

            return Math.Min(1.0, totalScore);
        }

        /// <summary>
        /// Calculates score based on number of connected accounts.
        /// More accounts generally indicates a real user.
        /// </summary>
        private double CalculateAccountCountScore(int accountCount)
        {
            // Logarithmic scale: 1 account = 0.3, 3 accounts = 0.7, 5+ accounts = 1.0
            return Math.Min(1.0, 0.3 + (Math.Log(accountCount) / Math.Log(5)) * 0.7);
        }

        /// <summary>
        /// Calculates score based on total reputation across all services.
        /// Higher reputation indicates more active, engaged user.
        /// </summary>
        private double CalculateReputationScore(double totalReputation)
        {
            // Logarithmic scale with diminishing returns
            if (totalReputation <= 0)
                return 0.0;

            // Score approaches 1.0 as reputation approaches 10000
            return Math.Min(1.0, Math.Log10(totalReputation + 1) / 4.0);
        }

        /// <summary>
        /// Calculates score based on account age.
        /// Older accounts are more likely to be real users.
        /// </summary>
        private double CalculateAccountAgeScore(DateTime earliestCreation)
        {
            if (earliestCreation == DateTime.MaxValue)
                return 0.0;

            var accountAge = DateTime.UtcNow - earliestCreation;
            var daysSinceCreation = accountAge.TotalDays;

            // New accounts (< 30 days) are suspicious: 0.2
            // 1 year old: 0.7
            // 2+ years: 1.0
            if (daysSinceCreation < 30)
                return 0.2;
            if (daysSinceCreation < 365)
                return 0.2 + (daysSinceCreation / 365.0) * 0.5;
            if (daysSinceCreation < 730)
                return 0.7 + ((daysSinceCreation - 365) / 365.0) * 0.3;

            return 1.0;
        }

        /// <summary>
        /// Calculates score based on verified status across accounts.
        /// </summary>
        private double CalculateVerificationScore(List<ServiceAccount> accounts)
        {
            if (accounts.Count == 0)
                return 0.0;

            var verifiedCount = accounts.Count(a => a.IsVerified);
            return (double)verifiedCount / accounts.Count;
        }

        /// <summary>
        /// Gets a user profile by ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user profile if found, null otherwise.</returns>
        public UserProfile GetUserProfile(TLink userId)
        {
            return _userProfiles.TryGetValue(userId, out var profile) ? profile : null;
        }

        /// <summary>
        /// Gets a detailed report of the realness score calculation for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A formatted string with score breakdown.</returns>
        public string GetScoreReport(TLink userId)
        {
            if (!_userProfiles.TryGetValue(userId, out var profile))
            {
                throw new KeyNotFoundException($"User profile with ID {userId} not found.");
            }

            var accountCountScore = CalculateAccountCountScore(profile.ConnectedAccounts.Count);
            var reputationScore = CalculateReputationScore(profile.TotalReputation);
            var ageScore = CalculateAccountAgeScore(profile.EarliestAccountCreation);
            var verificationScore = CalculateVerificationScore(profile.ConnectedAccounts);
            var totalScore = CalculateRealnessScore(userId);

            return $@"User Realness Score Report for User {userId}
===========================================
Connected Accounts: {profile.ConnectedAccounts.Count} (Score: {accountCountScore:F2})
Total Reputation: {profile.TotalReputation:F0} (Score: {reputationScore:F2})
Account Age: {(DateTime.UtcNow - profile.EarliestAccountCreation).TotalDays:F0} days (Score: {ageScore:F2})
Verified Accounts: {profile.ConnectedAccounts.Count(a => a.IsVerified)}/{profile.ConnectedAccounts.Count} (Score: {verificationScore:F2})

Overall Realness Score: {totalScore:F2} ({totalScore * 100:F0}%)

Connected Services:
{string.Join("\n", profile.ConnectedAccounts.Select(a => $"  - {a.ServiceName}: {a.AccountId} (Rep: {a.Reputation:F0}, Verified: {a.IsVerified})"))}";
        }
    }
}
