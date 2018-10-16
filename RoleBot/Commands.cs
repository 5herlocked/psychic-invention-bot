using System.Text;
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
    internal class Commands
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
            Bot.RolesToWatch.Add(new RoleWatch(context.Guild, channel, message, emoji, role));

            await context.TriggerTypingAsync();

            var description = new StringBuilder();

            description.AppendLine(Formatter.Bold("Role: ")).AppendLine(role.Name).AppendLine()
                .AppendLine(Formatter.Bold("Emoji: ")).AppendLine(emoji).AppendLine()
                .AppendLine(Formatter.Bold("Channel: ")).AppendLine(channel.Name).AppendLine()
                .AppendLine(Formatter.Bold("Message: ")).AppendLine(message.Content + " from " + message.Author);
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Reaction Role Created",
                Description = description.ToString()
            };

            await Bot.UpdateConfigFile();
            await context.RespondAsync("", false, embed);
        }
    }
}