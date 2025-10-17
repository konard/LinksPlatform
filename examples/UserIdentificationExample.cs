using System;
using Platform.Examples;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Examples
{
    /// <summary>
    /// Example demonstrating the usage of UserIdentificationService.
    /// Shows how to create user profiles, add connected accounts, and calculate realness scores.
    /// </summary>
    public class UserIdentificationExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("User Identification Service Example");
            Console.WriteLine("=====================================\n");

            // Initialize links storage
            using var links = new UInt64Links(new UInt64LinksOptions());
            var service = new UserIdentificationService(links);

            // Example 1: New user with minimal presence
            Console.WriteLine("Example 1: New user with single account");
            Console.WriteLine("----------------------------------------");
            var newUserId = 1UL;
            var newUser = service.CreateUserProfile(newUserId);
            service.AddServiceAccount(newUserId, new UserIdentificationService.ServiceAccount
            {
                ServiceName = "GitHub",
                AccountId = "newuser123",
                Reputation = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                IsVerified = false
            });

            var newUserScore = service.CalculateRealnessScore(newUserId);
            Console.WriteLine(service.GetScoreReport(newUserId));
            Console.WriteLine($"\nConclusion: Score {newUserScore:F2} - Likely a new or bot account\n\n");

            // Example 2: Established user with multiple accounts
            Console.WriteLine("Example 2: Established user with multiple verified accounts");
            Console.WriteLine("-------------------------------------------------------------");
            var establishedUserId = 2UL;
            var establishedUser = service.CreateUserProfile(establishedUserId);

            service.AddServiceAccount(establishedUserId, new UserIdentificationService.ServiceAccount
            {
                ServiceName = "GitHub",
                AccountId = "developer_pro",
                Reputation = 2500,
                CreatedAt = DateTime.UtcNow.AddYears(-3),
                IsVerified = true
            });

            service.AddServiceAccount(establishedUserId, new UserIdentificationService.ServiceAccount
            {
                ServiceName = "StackOverflow",
                AccountId = "dev_pro_999",
                Reputation = 15000,
                CreatedAt = DateTime.UtcNow.AddYears(-4),
                IsVerified = true
            });

            service.AddServiceAccount(establishedUserId, new UserIdentificationService.ServiceAccount
            {
                ServiceName = "Twitter",
                AccountId = "@devpro",
                Reputation = 500,
                CreatedAt = DateTime.UtcNow.AddYears(-5),
                IsVerified = true
            });

            var establishedUserScore = service.CalculateRealnessScore(establishedUserId);
            Console.WriteLine(service.GetScoreReport(establishedUserId));
            Console.WriteLine($"\nConclusion: Score {establishedUserScore:F2} - Highly likely a real, active user\n\n");

            // Example 3: Suspicious account
            Console.WriteLine("Example 3: Suspicious account with high reputation but very new");
            Console.WriteLine("------------------------------------------------------------------");
            var suspiciousUserId = 3UL;
            var suspiciousUser = service.CreateUserProfile(suspiciousUserId);

            service.AddServiceAccount(suspiciousUserId, new UserIdentificationService.ServiceAccount
            {
                ServiceName = "Reddit",
                AccountId = "definitely_real_user",
                Reputation = 10000,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsVerified = false
            });

            var suspiciousUserScore = service.CalculateRealnessScore(suspiciousUserId);
            Console.WriteLine(service.GetScoreReport(suspiciousUserId));
            Console.WriteLine($"\nConclusion: Score {suspiciousUserScore:F2} - Red flag: high reputation but very new account\n\n");

            // Example 4: Average user
            Console.WriteLine("Example 4: Average user with moderate activity");
            Console.WriteLine("-----------------------------------------------");
            var averageUserId = 4UL;
            var averageUser = service.CreateUserProfile(averageUserId);

            service.AddServiceAccount(averageUserId, new UserIdentificationService.ServiceAccount
            {
                ServiceName = "GitHub",
                AccountId = "casual_coder",
                Reputation = 150,
                CreatedAt = DateTime.UtcNow.AddYears(-1).AddMonths(-6),
                IsVerified = false
            });

            service.AddServiceAccount(averageUserId, new UserIdentificationService.ServiceAccount
            {
                ServiceName = "LinkedIn",
                AccountId = "john_doe_dev",
                Reputation = 80,
                CreatedAt = DateTime.UtcNow.AddYears(-2),
                IsVerified = true
            });

            var averageUserScore = service.CalculateRealnessScore(averageUserId);
            Console.WriteLine(service.GetScoreReport(averageUserId));
            Console.WriteLine($"\nConclusion: Score {averageUserScore:F2} - Moderate confidence, likely a real casual user\n");

            Console.WriteLine("\n=====================================");
            Console.WriteLine("Example completed successfully!");
        }
    }
}
