using System;
using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch
    {
        public DiscordGuild Guild { get; }
        public DiscordChannel Channel { get; }
        public DiscordMessage Message { get; }
        public DiscordEmoji Emoji { get; }
        public DiscordRole Role { get; }

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
    }
}