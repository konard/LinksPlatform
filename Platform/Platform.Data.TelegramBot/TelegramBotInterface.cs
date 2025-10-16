using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Platform.Communication.Protocol.Udp;
using Platform.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace Platform.Data.TelegramBot
{
    public class TelegramBotInterface
    {
        private readonly IConfiguration _configuration;
        private TelegramBotClient _botClient;
        private UdpSender _sender;
        private UdpClient _receiver;
        private CancellationTokenSource _cancellationTokenSource;

        public TelegramBotInterface(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task RunAsync()
        {
            try
            {
                var botToken = _configuration["TelegramBot:BotToken"];
                if (string.IsNullOrWhiteSpace(botToken) || botToken == "YOUR_BOT_TOKEN_HERE")
                {
                    throw new InvalidOperationException("Bot token is not configured. Please set TelegramBot:BotToken in appsettings.json");
                }

                var serverPort = int.Parse(_configuration["TelegramBot:ServerPort"] ?? "7777");
                var receivePort = int.Parse(_configuration["TelegramBot:ReceivePort"] ?? "8888");

                _cancellationTokenSource = new CancellationTokenSource();
                _botClient = new TelegramBotClient(botToken);
                _sender = new UdpSender(serverPort);
                _receiver = new UdpClient(receivePort);

                var me = await _botClient.GetMeAsync();
                Console.WriteLine($"Telegram bot @{me.Username} is running.");
                Console.WriteLine("Press CTRL+C to stop the bot.");

                _botClient.OnMessage += OnMessageReceived;
                _botClient.StartReceiving();

                // Listen for messages from the Platform
                await ListenToPlatformAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToStringWithAllInnerExceptions()}");
            }
            finally
            {
                _botClient?.StopReceiving();
                _sender?.Dispose();
                _receiver?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
        }

        private async void OnMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;
                if (message?.Text != null)
                {
                    Console.WriteLine($"Received from @{message.Chat.Username ?? message.Chat.Id.ToString()}: {message.Text}");

                    // Forward message to Platform
                    _sender.Send($"{message.Chat.Id}|{message.Text}");

                    // Simple echo for testing
                    await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Received: {message.Text}"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        private async Task ListenToPlatformAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_receiver.Available > 0)
                    {
                        var message = _receiver.ReceiveString();
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            Console.WriteLine($"Received from Platform: {message}");

                            // Parse format: "chatId|text"
                            var parts = message.Split('|', 2);
                            if (parts.Length == 2 && long.TryParse(parts[0], out var chatId))
                            {
                                await _botClient.SendTextMessageAsync(
                                    chatId: new ChatId(chatId),
                                    text: parts[1]
                                );
                            }
                        }
                    }
                    await Task.Delay(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving from Platform: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
