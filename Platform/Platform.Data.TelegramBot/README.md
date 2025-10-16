# Platform.Data.TelegramBot

Telegram Bot interface for LinksPlatform.

## Overview

This project provides a Telegram Bot interface that connects Telegram users with the LinksPlatform data store. The bot acts as a bridge between Telegram messages and the Platform's UDP communication protocol.

## Features

- Receives messages from Telegram users
- Forwards messages to LinksPlatform via UDP
- Sends responses from Platform back to Telegram users
- Simple echo functionality for testing

## Setup

### Prerequisites

1. .NET Core 2.2 SDK or later
2. A Telegram Bot Token (obtain from [@BotFather](https://t.me/botfather) on Telegram)

### Creating a Telegram Bot

1. Open Telegram and search for [@BotFather](https://t.me/botfather)
2. Send `/newbot` command
3. Follow the instructions to create your bot
4. Copy the bot token provided by BotFather

### Configuration

Edit `appsettings.json` and replace `YOUR_BOT_TOKEN_HERE` with your actual bot token:

```json
{
  "TelegramBot": {
    "BotToken": "YOUR_BOT_TOKEN_HERE",
    "ServerAddress": "localhost",
    "ServerPort": 7777,
    "ReceivePort": 8888
  }
}
```

### Running

```bash
dotnet run --project Platform.Data.TelegramBot
```

The bot will start and listen for incoming messages. Press CTRL+C to stop.

## Architecture

The bot operates in two modes:

1. **Telegram → Platform**: Receives messages from Telegram users and forwards them to the Platform via UDP on port 7777
2. **Platform → Telegram**: Listens for messages from Platform on UDP port 8888 and sends them to Telegram users

Message format for Platform → Telegram: `chatId|text`

## Usage Example

1. Start the Platform server (e.g., Platform.Data.MasterServer)
2. Start the Telegram bot
3. Send a message to your bot on Telegram
4. The bot will echo the message and forward it to the Platform

## Dependencies

- Telegram.Bot 15.7.1
- Platform.Examples
- Microsoft.Extensions.Configuration

## License

See the LICENSE file in the root of the repository.
