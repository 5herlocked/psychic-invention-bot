using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch
    {
        private DiscordGuild Guild { get; set; }
        private DiscordChannel Channel { get; set; }
        private DiscordMessage Message { get; set; }
        private DiscordEmoji Emoji { get; set; }
        private DiscordRole Role { get; set; }

        // Constructor
        public RoleWatch(DiscordGuild guild, DiscordChannel channel, DiscordMessage message, DiscordEmoji emoji, DiscordRole role)
        {
            Guild = guild;
            Channel = channel;
            Message = message;
            Emoji = emoji;
            Role = role;
        }
    }
}