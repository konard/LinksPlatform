using System;
using System.Collections.Generic;
using System.Threading;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// A web crawler that collects chat data (messages) and stores them in links storage.
    /// This data can later be used in search engines.
    /// </summary>
    public class ChatCrawler
    {
        private readonly SequenceIndex<ulong> _index;
        private readonly SynchronizedLinks<ulong> _links;
        private readonly ulong _messageMarker;
        private readonly ulong _userMarker;
        private readonly ulong _timestampMarker;
        private readonly ulong _chatMarker;

        public ChatCrawler(SynchronizedLinks<ulong> links, SequenceIndex<ulong> indexer)
        {
            _links = links;
            _index = indexer;

            // Create semantic markers for chat data structure
            _messageMarker = CreateMarker("message");
            _userMarker = CreateMarker("user");
            _timestampMarker = CreateMarker("timestamp");
            _chatMarker = CreateMarker("chat");
        }

        private ulong CreateMarker(string name)
        {
            var linkArray = UnicodeMap.FromStringToLinkArray(name);
            _index.Add(linkArray);
            // Return the link representing this sequence
            // Since we're using the index just to store, we'll use a convention
            // to create a unique marker by getting/creating a link pair
            var firstChar = UnicodeMap.FromCharToLink(name[0]);
            var lastChar = UnicodeMap.FromCharToLink(name[name.Length - 1]);
            return _links.GetOrCreate(firstChar, lastChar);
        }

        /// <summary>
        /// Stores a chat message in the links storage.
        /// Structure: (message_marker, (user_link, (timestamp_link, message_text_link)))
        /// </summary>
        public ulong StoreMessage(string username, string messageText, DateTime timestamp, string chatId)
        {
            // Convert username to links and add to index
            var userLinkArray = UnicodeMap.FromStringToLinkArray(username);
            _index.Add(userLinkArray);
            var userLink = _links.GetOrCreate(UnicodeMap.FromCharToLink(username[0]),
                username.Length > 1 ? UnicodeMap.FromCharToLink(username[username.Length - 1]) : UnicodeMap.FromCharToLink(username[0]));

            // Convert message text to links and add to index
            var messageLinkArray = UnicodeMap.FromStringToLinkArray(messageText);
            _index.Add(messageLinkArray);
            var messageTextLink = _links.GetOrCreate(UnicodeMap.FromCharToLink(messageText[0]),
                messageText.Length > 1 ? UnicodeMap.FromCharToLink(messageText[messageText.Length - 1]) : UnicodeMap.FromCharToLink(messageText[0]));

            // Convert timestamp to string and then to links
            var timestampStr = timestamp.ToString("o"); // ISO 8601 format
            var timestampLinkArray = UnicodeMap.FromStringToLinkArray(timestampStr);
            _index.Add(timestampLinkArray);
            var timestampLink = _links.GetOrCreate(UnicodeMap.FromCharToLink(timestampStr[0]), UnicodeMap.FromCharToLink(timestampStr[timestampStr.Length - 1]));

            // Convert chat ID to links and add to index
            var chatLinkArray = UnicodeMap.FromStringToLinkArray(chatId);
            _index.Add(chatLinkArray);
            var chatLink = _links.GetOrCreate(UnicodeMap.FromCharToLink(chatId[0]),
                chatId.Length > 1 ? UnicodeMap.FromCharToLink(chatId[chatId.Length - 1]) : UnicodeMap.FromCharToLink(chatId[0]));

            // Create structure: user -> message_text
            var userToMessageLink = _links.GetOrCreate(userLink, messageTextLink);

            // Create structure: timestamp -> (user -> message_text)
            var timestampToUserMessageLink = _links.GetOrCreate(timestampLink, userToMessageLink);

            // Create structure: chat -> (timestamp -> (user -> message_text))
            var chatToTimestampUserMessageLink = _links.GetOrCreate(chatLink, timestampToUserMessageLink);

            // Create final structure: message_marker -> (chat -> (timestamp -> (user -> message_text)))
            var messageLink = _links.GetOrCreate(_messageMarker, chatToTimestampUserMessageLink);

            return messageLink;
        }

        /// <summary>
        /// Stores multiple chat messages in batch.
        /// </summary>
        public void StoreMessages(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
        {
            var count = 0;
            foreach (var message in messages)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                StoreMessage(message.Username, message.Text, message.Timestamp, message.ChatId);
                count++;

                if (count % 100 == 0)
                {
                    Console.WriteLine($"Stored {count} messages, total links: {_links.Count() - UnicodeMap.MapSize}");
                }
            }

            Console.WriteLine($"Finished storing {count} messages, total links: {_links.Count() - UnicodeMap.MapSize}");
        }

        /// <summary>
        /// Retrieves the total count of stored messages.
        /// </summary>
        public long GetMessageCount()
        {
            // Count links that have _messageMarker as source
            long count = 0;
            var query = new Link<ulong>(_messageMarker, _links.Constants.Any);
            _links.Each(link =>
            {
                // Check if this link has _messageMarker as its source (first element)
                if (link[0].Equals(_messageMarker))
                {
                    count++;
                }
                return _links.Constants.Continue;
            }, query);
            return count;
        }
    }

    /// <summary>
    /// Represents a chat message with its metadata.
    /// </summary>
    public class ChatMessage
    {
        public string Username { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public string ChatId { get; set; }

        public ChatMessage(string username, string text, DateTime timestamp, string chatId)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Timestamp = timestamp;
            ChatId = chatId ?? throw new ArgumentNullException(nameof(chatId));
        }
    }
}
