//@author Shardul Vaidya
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch
    {
        [XmlIgnore]
        public DiscordGuild Guild { get; }
        [XmlIgnore]
        public DiscordChannel Channel { get; }
        [XmlIgnore]
        public DiscordMessage Message { get; }
        [XmlIgnore]
        public DiscordEmoji Emoji { get; }
        [XmlIgnore]
        public DiscordRole Role { get; }
        
        [XmlElement("guild")]
        public string GuildId { get; set; }
        
        [XmlElement("channel")]
        public string ChannelId { get; set; }
        
        [XmlElement("message")]
        public string MessageId { get; set; }
        
        [XmlElement("emoji")]
        public string EmojiId { get; set; }
        
        [XmlElement("role")]
        public string RoleId { get; set; }

        public RoleWatch() { }

        // Constructor
        public RoleWatch(DiscordGuild guild, DiscordChannel channel, DiscordMessage message, DiscordEmoji emoji, DiscordRole role)
        {
            Guild = guild;
            Channel = channel;
            Message = message;
            Emoji = emoji;
            Role = role;
        }

        public RoleWatch(string guild, string channel, string message, string emoji, string role)
        {
            Guild = Bot.Client.GetGuildAsync(UInt64.Parse(guild)).Result;
            Channel = Guild.GetChannel(UInt64.Parse(channel));
            Message = Channel.GetMessageAsync(UInt64.Parse(message)).Result;
            Emoji = Guild.GetEmojiAsync(UInt64.Parse(emoji)).Result;
            Role = Guild.GetRole(UInt64.Parse(role));
        }

        [OnSerializing]
        private void Serialize()
        {
            GuildId = Guild.Id.ToString();
            MessageId = Message.Id.ToString();
            ChannelId = Channel.Id.ToString();
            EmojiId = Emoji.Id.ToString();
            RoleId = Role.Id.ToString();
        }
    }
}