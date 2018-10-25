using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace RoleBot
{
    // marked as admin group commands
    [Description("Administrative Commands")] // created for helpformatter
    [RequirePermissions(Permissions.ManageRoles)]
    internal class RoleCommands
    {
        // all commands will need to be executed as such <prefix> admin <command> <args>
        
        // adds a role to watch for rolebot
        [Command("addrole"), Description("Constructs a new Role to watch"),
         RequirePermissions(Permissions.ManageRoles)]
        public async Task AddRole(CommandContext context,
            [Description("Channel to watch")] DiscordChannel channel,
            [Description("Message to Watch")] DiscordMessage message,
            [Description("Emote to watch corresponding to Role")] DiscordEmoji emoji,
            [Description("Role to watch")] DiscordRole role)
        {
            // adds to list of roles being watched
            Bot.RolesToWatch.Add(new RoleWatch(context.Guild, channel, message, emoji, role));

            await context.TriggerTypingAsync();
            
            // string to be used for embedbuilder
            var description = new StringBuilder();

            description.AppendLine(Formatter.Bold("Role: ")).AppendLine(role.Name).AppendLine()
                .AppendLine(Formatter.Bold("Emoji: ")).AppendLine(emoji).AppendLine()
                .AppendLine(Formatter.Bold("Channel: ")).AppendLine(channel.Name).AppendLine()
                .AppendLine(Formatter.Bold("Message: ")).AppendLine(message.Content + " from " + message.Author.Username);
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Reaction Role Created",
                Description = description.ToString()
            };
            
            await LogPrinter.Role_Created(role, emoji);
            await context.RespondAsync("", false, embed);
            await Bot.UpdateConfigFile();
        }
        
        // stops an existing role from being watched
        [Command("removerole"), Description("Stops an existing Role from being watched"),
         RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveRole(CommandContext context,
            [Description("Role that should be stopped watching")]
            DiscordRole role)
        {
            var toRemove = (from roles in Bot.RolesToWatch
                where roles.Role.Equals(role) && context.Guild.Equals(roles.Guild) select roles).First();

            Bot.RolesToWatch.Remove(toRemove);
            
            await context.TriggerTypingAsync();
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Reaction Role Removed",
                Description = $"{role.Name} is no longer being watched by RoleBot"
            };

            await LogPrinter.Role_Removed(role);
            await context.RespondAsync("", false, embed);
            await Bot.UpdateConfigFile();
            
        }
    }
    
    // commands restricted to the owner of the guild
    [Group("owner")]
    [Description("Owner Commands")]
    [Hidden]
    internal class OwnerCommands
    {
        // toggle AutoRemove to let existing servers keep their roles
        [Command("autoremove"), Description("Toggles the auto-removal of members who haven't reacted"), RequireOwner]
        public async Task ToggleAutoRemove(CommandContext context)
        {
            Bot.AutoRemoveFlag = !Bot.AutoRemoveFlag;
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Toggle Auto Remove",
                Description = $"The Bot will {(Bot.AutoRemoveFlag ? "no longer" : "")} revoke member roles automatically"
            };

            await context.TriggerTypingAsync();
            await context.RespondAsync("", false, embed);
            await Bot.UpdateConfigFile();
        }
    }
}