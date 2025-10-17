using System;
using System.Collections.Generic;

namespace Platform.Examples.ProfileSync
{
    /// <summary>
    /// Represents account data that can be synced/backed up across platforms
    /// </summary>
    public class AccountData
    {
        /// <summary>
        /// Platform name (e.g., "WhatsApp", "Twitter")
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Account identifier
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Profile information
        /// </summary>
        public ProfileInfo Profile { get; set; }

        /// <summary>
        /// Messages/posts
        /// </summary>
        public List<Message> Messages { get; set; }

        /// <summary>
        /// Contacts/friends
        /// </summary>
        public List<Contact> Contacts { get; set; }

        /// <summary>
        /// Media files (photos, videos, etc.)
        /// </summary>
        public List<MediaItem> Media { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Last sync timestamp
        /// </summary>
        public DateTime LastSyncTime { get; set; }

        public AccountData()
        {
            Messages = new List<Message>();
            Contacts = new List<Contact>();
            Media = new List<MediaItem>();
            Metadata = new Dictionary<string, object>();
            LastSyncTime = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Profile information
    /// </summary>
    public class ProfileInfo
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureUrl { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }

        public ProfileInfo()
        {
            CustomFields = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Represents a message or post
    /// </summary>
    public class Message
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }
        public List<string> Attachments { get; set; }

        public Message()
        {
            Attachments = new List<string>();
        }
    }

    /// <summary>
    /// Message types
    /// </summary>
    public enum MessageType
    {
        Text,
        Image,
        Video,
        Audio,
        Document,
        Post,
        Comment
    }

    /// <summary>
    /// Represents a contact
    /// </summary>
    public class Contact
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }

        public Contact()
        {
            CustomFields = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Represents a media item
    /// </summary>
    public class MediaItem
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string LocalPath { get; set; }
        public MediaType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public MediaItem()
        {
            Metadata = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Media types
    /// </summary>
    public enum MediaType
    {
        Image,
        Video,
        Audio,
        Document,
        Other
    }
}
