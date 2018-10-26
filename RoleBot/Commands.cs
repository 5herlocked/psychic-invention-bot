// @author Shardul Vaidya

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace RoleBot
{
    [Description("Role Management Commands")] // created for helpformatter
    [RequirePermissions(Permissions.ManageRoles)]
    internal class RoleCommands
    {
        /* AddRole Method
         * 
         * This method creates a new RoleWatch Object so that RoleBot can watch it.
         *
         * For doing so, the method needs, the channel #<channel>, the message uID, the emote to be watched :<emote>:,
         * finally the role to be assigned or revoked @<role>
         *
         * The Bot then creates the RoleWatch Object and adds it to the list RolesToWatch so it can be included in the
         * next reaction added for the bot.
         * To confirm, sends an embedded message including the role, channel, message contents and message author that is
         * going to be watched.
         *
         * Lastly, the bot updates it's config file for the changes to be permanent
         */
        [Command("addrole"), Description("Constructs a new Role to watch"),
         RequirePermissions(Permissions.ManageRoles)]
        public async Task AddRole(CommandContext context,
            [Description("Channel to watch")] DiscordChannel channel,
            [Description("Message to Watch")] DiscordMessage message,
            [Description("Emote to watch corresponding to Role")] DiscordEmoji emoji,
            [Description("Role to watch")] DiscordRole role)
        {
            // adds to list of roles being watched
            Bot.Config.RolesToWatch.Add(new RoleWatch(context.Guild, channel, message, emoji, role));

            await context.TriggerTypingAsync();
            
            // string to be used for embedbuilder
            var description = new StringBuilder();
            
            /*
             * Creates the description to be embedded in the response for the addrole command
             * Format:
             * Role: <RoleName>
             * Emoji: <Emoji>
             * Channel: <Channel>
             * Message: <Message + Author>
             */
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
        
        /* RemoveRole Method
         * 
         * This method removes an existing RoleWatch Object so the bot no longer watches it.
         * For doing so, the method needs only the role @<role> to be removed.
         *
         * The Bot uses a linq statement that:
         * Selects the first role which belongs to the same guild and returns true for the equality method
         * Then removes it from RolesToWatch list so it doesn't need to watched the next time a reaction is removed.
         *
         * The Bot also responds to confirm that the role is no longer being watched
         *
         * Finally, it updates it config file so that the removal is permanent.
         */
        [Command("removerole"), Description("Stops an existing Role from being watched"),
         RequirePermissions(Permissions.ManageRoles)]
        public async Task RemoveRole(CommandContext context,
            [Description("Role that should be stopped watching")]
            DiscordRole role)
        {
            var toRemove = (from roles in Bot.Config.RolesToWatch
                where roles.Role.Equals(role) && context.Guild.Equals(roles.Guild) select roles).First();

            if (toRemove == null)
            {
                await context.TriggerTypingAsync();
                await context.RespondAsync("Looks like this role is not being watched");
            }
            
            Bot.Config.RolesToWatch.Remove(toRemove);
            
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
    
    // commands restricted to the admin of the guild
    [Group("admin")]
    [Description("Admin Commands")]
    [Hidden]
    [RequirePermissions(Permissions.Administrator)]
    internal class AdminCommands
    {
        /* ToggleAutoRemove Method
         * 
         * This method toggle's the autoremove function of the bot which retroactively revokes the role being watched
         * from all users who haven't reacted to the message.
         *
         * Mainly present to let pre-existing servers implement this bot without needing to have all its users react to
         * a message.
         */
        [Command("autoremove"), Description("Toggles the auto-removal of members who haven't reacted"), RequireOwner]
        public async Task ToggleAutoRemove(CommandContext context)
        {
            Bot.Config.AutoRemoveFlag = !Bot.Config.AutoRemoveFlag;
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Toggle Auto Remove",
                Description = $"The Bot will {(Bot.Config.AutoRemoveFlag ? "no longer" : "")} revoke member roles automatically"
            };

            await context.TriggerTypingAsync();
            await context.RespondAsync("", false, embed);
            await Bot.UpdateConfigFile();
        }
    }
}