//@author Shardul Vaidya

using System;

using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch
    {
        public DiscordGuild GetGuild() => Bot.Client.GetGuildAsync(Guild).Result;
        public void SetGuild(DiscordGuild value) => Guild = value.Id;

        public DiscordChannel GetChannel() => GetGuild().GetChannel(Channel);
        public void SetChannel(DiscordChannel value) => Channel = value.Id;

        public DiscordMessage GetMessage() => GetChannel().GetMessageAsync(Message).Result;
        public void SetMessage(DiscordMessage value) => Message = value.Id;

        public DiscordEmoji GetEmoji() => DiscordEmoji.FromName(Bot.Client, Emoji);
        public void SetEmoji(DiscordEmoji value) => Emoji = value.GetDiscordName();

        public ulong Guild { get; set; }
        public ulong Channel { get; set; }
        public ulong Message { get; set; }
        public string Emoji { get; set; }
        public ulong Role { get; set; }

        public DiscordRole GetRole() => GetGuild().GetRole(Role);
        public void SetRole(DiscordRole value) => Role = value.Id;

        public RoleWatch() { }

        // Constructor
        public RoleWatch(DiscordGuild guild, DiscordChannel channel, string message, DiscordEmoji emoji, DiscordRole role)
        {
            SetGuild(guild);
            SetChannel(channel);
            SetMessage(GetChannel().GetMessageAsync(UInt64.Parse(message)).Result);
            SetEmoji(emoji);
            SetRole(role);
        }

        public RoleWatch(string guild, string channel, string message, string emoji, string role)
        {
            SetGuild(Bot.Client.GetGuildAsync(UInt64.Parse(guild)).Result);
            SetChannel(GetGuild().GetChannel(UInt64.Parse(channel)));
            SetMessage(GetChannel().GetMessageAsync(UInt64.Parse(message)).Result);
            SetEmoji(DiscordEmoji.FromName(Bot.Client, emoji));
            SetRole(GetGuild().GetRole(UInt64.Parse(role)));
        }
    }
}