using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace RoleBot
{
    [Group("admin")] // marked as admin group commands
    [Description("Administrative Commands")] // created for helpformatter
    [RequirePermissions(Permissions.ManageRoles)]
    public class Commands
    {
        // all commands will need to be executed as such <prefix> admin <command> <args>

        [Command("addrole"), Description("Constructs a new Role to watch"),
         RequirePermissions(Permissions.ManageRoles)]
        public async Task AddRole(CommandContext context,
            [Description("Channel to watch")] DiscordChannel channel,
            [Description("Message to Watch")] DiscordMessage message,
            [Description("Emote to watch corresponding to Role")] DiscordEmoji emoji,
            [Description("Role to watch")] DiscordRole role)
        {
            Bot.Channels.Add(channel);
            Bot.Messages.Add(message);
            
            
        }
    }
}