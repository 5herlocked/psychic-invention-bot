//@author Shardul Vaidya

using System;

using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch
    {
        private UInt64 guild;

        public DiscordGuild GetGuild() => Bot.Client.GetGuildAsync(guild).Result;
        public void SetGuild(DiscordGuild value) => guild = value.Id;

        private UInt64 channel;

        public DiscordChannel GetChannel() => GetGuild().GetChannel(channel);
        public void SetChannel(DiscordChannel value) => channel = value.Id;

        private UInt64 message;

        public DiscordMessage GetMessage() => GetChannel().GetMessageAsync(message).Result;
        public void SetMessage(DiscordMessage value) => message = value.Id;

        private string emoji;

        public DiscordEmoji GetEmoji() => DiscordEmoji.FromName(Bot.Client, emoji);
        public void SetEmoji(DiscordEmoji value) => emoji = value.GetDiscordName();

        private UInt64 role;

        public DiscordRole GetRole() => GetGuild().GetRole(role);
        public void SetRole(DiscordRole value) => role = value.Id;

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