//@author Shardul Vaidya

using System;

using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch
    {
        public DiscordGuild Guild { get; set; }
        
        public DiscordChannel Channel { get; set;  }
        
        public DiscordMessage Message { get; set; }
        
        public DiscordEmoji Emoji { get; set; }
        
        public DiscordRole Role { get; set; }

        public RoleWatch() { }

        // Constructor
        public RoleWatch(DiscordGuild guild, DiscordChannel channel, string message, DiscordEmoji emoji, DiscordRole role)
        {
            Guild = guild;
            Channel = channel;
            Message = Channel.GetMessageAsync(UInt64.Parse(message)).Result;
            Emoji = emoji;
            Role = role;
        }

        public RoleWatch(string guild, string channel, string message, string emoji, string role)
        {
            Guild = Bot.Client.GetGuildAsync(UInt64.Parse(guild)).Result;
            Channel = Guild.GetChannel(UInt64.Parse(channel));
            Message = Channel.GetMessageAsync(UInt64.Parse(message)).Result;
            Emoji = DiscordEmoji.FromName(Bot.Client, emoji);
            Role = Guild.GetRole(UInt64.Parse(role));
        }
    }
}