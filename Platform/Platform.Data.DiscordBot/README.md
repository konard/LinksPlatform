# Discord Error Search Bot

A Discord bot that automates error message search across multiple platforms: Google, Bing, GitHub Code Search, and GitHub Issues Search.

## Features

- 🔍 Generates search links for error messages across multiple platforms
- 💬 Supports both text commands and slash commands
- 📝 Respects Discord's 2000 character message limit
- 🚀 Easy to deploy and configure

## Setup

### Prerequisites

- .NET 6.0 or higher
- A Discord bot token (see [Discord Developer Portal](https://discord.com/developers/applications))

### Configuration

1. Create a Discord bot at the [Discord Developer Portal](https://discord.com/developers/applications)
2. Enable the following bot permissions:
   - Send Messages
   - Use Slash Commands
   - Read Messages/View Channels
3. Enable the following privileged gateway intents:
   - Message Content Intent
4. Copy your bot token
5. Set the bot token using one of these methods:

#### Option 1: Environment Variable
```bash
# Linux/macOS
export DISCORD_BOT_TOKEN=your_token_here

# Windows
set DISCORD_BOT_TOKEN=your_token_here
```

#### Option 2: Configuration File
Edit `appsettings.json`:
```json
{
  "DiscordBotToken": "your_token_here"
}
```

### Running the Bot

```bash
cd Platform/Platform.Data.DiscordBot
dotnet run
```

## Usage

### Text Commands

- `!search <error message>` - Generate search links for an error message
- `!error <error message>` - Same as !search
- `!help` - Show help message

### Slash Commands

- `/search <error message>` - Generate search links (slash command version)

### Examples

```
!search NullReferenceException: Object reference not set to an instance of an object
```

```
/search TypeError: Cannot read property 'length' of undefined
```

The bot will respond with search links for:
- 🌐 Google
- 🔷 Bing
- 💻 GitHub Code Search
- 📝 GitHub Issues Search

## Development

### Building

```bash
dotnet build Platform/Platform.Data.DiscordBot/Platform.Data.DiscordBot.csproj
```

### Project Structure

- `Program.cs` - Entry point and configuration loading
- `DiscordBot.cs` - Main bot logic and command handlers
- `SearchLinkGenerator.cs` - Search link generation utility
- `appsettings.json` - Configuration file (optional)

## License

This project is part of the LinksPlatform and is licensed under the same terms.
