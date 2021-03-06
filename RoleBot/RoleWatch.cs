//@author Shardul Vaidya

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch : IXmlSerializable
    {
        public DiscordGuild Guild { get; set; }
        
        public DiscordChannel Channel { get; set;  }
        
        public DiscordMessage Message { get; set; }
        
        public DiscordEmoji Emoji { get; set; }
        
        public DiscordRole Role { get; set; }

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

        public XmlSchema GetSchema()
        {
            return null;
        }
        
        /*
         * Custom implementation of the Read method in IXmlSerializable to cater to a descriptive yet consise
         * XML config file
         */
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var guild = reader.ReadElementString("Guild");
            var channel = reader.ReadElementString("Message");
            var emoji = reader.ReadElementString("Emoji");
            var message = reader.ReadElementString("Message");
            var role = reader.ReadElementString("RoleID");
            
            Guild = Bot.Client.GetGuildAsync(UInt64.Parse(guild)).Result;
            Channel = Guild.GetChannel(UInt64.Parse(channel));
            Message = Channel.GetMessageAsync(UInt64.Parse(message)).Result;
            Emoji = Guild.GetEmojiAsync(UInt64.Parse(emoji)).Result;
            Role = Guild.GetRole(UInt64.Parse(role));
        }
        
        /*
         * Custom implementation of WriteXml method in IXmlSerializable to cater to a descriptive XML file so clients
         * can configure without needing to refer to documentation.
         */
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Role");
            writer.WriteComment("Guild: " + Guild.Name);
            writer.WriteComment("Channel: " + Channel);
            writer.WriteComment("Message: " + Message);
            writer.WriteComment("Emoji: " + Emoji);
            writer.WriteComment("Role: " + Role.Name);
            writer.WriteElementString("Guild", Guild.Id.ToString());
            writer.WriteElementString("Channel", Channel.Id.ToString());
            writer.WriteElementString("Message", Message.Id.ToString());
            writer.WriteElementString("Emoji", Emoji.Id.ToString());
            writer.WriteElementString("RoleID", Role.Id.ToString());
            writer.WriteEndElement();
        }
    }
}