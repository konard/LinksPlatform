using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Platform.Data.DiscordBot
{
    /// <summary>
    /// Discord bot that automates error message search across multiple platforms
    /// </summary>
    public class DiscordBot
    {
        private readonly DiscordSocketClient _client;
        private readonly string _botToken;

        public DiscordBot(string botToken)
        {
            if (string.IsNullOrWhiteSpace(botToken))
            {
                throw new ArgumentException("Bot token cannot be empty", nameof(botToken));
            }

            _botToken = botToken;

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.MessageReceived += MessageReceivedAsync;
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"Bot is connected as {_client.CurrentUser}");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // Ignore bot messages
            if (message.Author.IsBot)
                return;

            // Check if the message starts with !search or !error
            if (message.Content.StartsWith("!search ") || message.Content.StartsWith("!error "))
            {
                var errorMessage = message.Content.Substring(message.Content.IndexOf(' ') + 1);
                var searchLinks = SearchLinkGenerator.GenerateCompactSearchLinks(errorMessage);

                await message.Channel.SendMessageAsync(searchLinks);
            }
            // Check if message starts with !help
            else if (message.Content.Equals("!help", StringComparison.OrdinalIgnoreCase) ||
                     message.Content.Equals("!search", StringComparison.OrdinalIgnoreCase))
            {
                var helpMessage = @"**Error Search Bot Help**

Use this bot to quickly generate search links for error messages across multiple platforms.

**Commands:**
• `!search <error message>` - Generate search links for an error message
• `!error <error message>` - Same as !search
• `/search <error message>` - Slash command version (if registered)
• `!help` - Show this help message

**Example:**
```
!search NullReferenceException: Object reference not set to an instance of an object
```

The bot will generate search links for:
🌐 Google
🔷 Bing
💻 GitHub Code Search
📝 GitHub Issues Search";

                await message.Channel.SendMessageAsync(helpMessage);
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (command.CommandName == "search")
            {
                var errorMessage = command.Data.Options.FirstOrDefault()?.Value?.ToString();

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    await command.RespondAsync("Please provide an error message to search for.");
                    return;
                }

                var searchLinks = SearchLinkGenerator.GenerateCompactSearchLinks(errorMessage);
                await command.RespondAsync(searchLinks);
            }
        }

        /// <summary>
        /// Starts the Discord bot
        /// </summary>
        public async Task StartAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();

            // Register slash commands
            try
            {
                var searchCommand = new SlashCommandBuilder()
                    .WithName("search")
                    .WithDescription("Search for an error message across multiple platforms")
                    .AddOption("error", ApplicationCommandOptionType.String,
                              "The error message to search for", isRequired: true);

                await _client.CreateGlobalApplicationCommandAsync(searchCommand.Build());
                Console.WriteLine("Slash command registered successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register slash command: {ex.Message}");
            }

            // Keep the bot running
            await Task.Delay(-1);
        }

        /// <summary>
        /// Stops the Discord bot
        /// </summary>
        public async Task StopAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}
