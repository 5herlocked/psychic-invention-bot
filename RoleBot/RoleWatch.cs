using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleWatch
    {
        public DiscordGuild Guild { get; private set; }
        public DiscordChannel Channel { get; private set; }
        public DiscordMessage Message { get; private set; }
        public DiscordEmoji Emoji { get; private set; }
        public DiscordRole Role { get; private set; }

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