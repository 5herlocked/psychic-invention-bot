using System.Linq;
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
        
        // kick a member using bot
        [Command("kick"), Description("Kicks a guild member from server"), RequirePermissions(Permissions.KickMembers)]
        public async Task KickMember(CommandContext context,
            [Description("Member that should be kicked")] DiscordMember member,
            [Description("Reason for kick")] string reason)
        {
            await context.TriggerTypingAsync();
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Member Kicked",
                Description = $"{context.User} you've kicked {member} for {reason}"
            };

            await context.RespondAsync("", false, embed);
            await member.SendMessageAsync($"You've been kicked from {context.Guild} for {reason}");
            await context.Guild.RemoveMemberAsync(member, reason);
        }
        
        // ban a member using bot
        [Command("ban"), Description("Bans a guild member from the guild"), RequirePermissions(Permissions.BanMembers)]
        public async Task BanMember(CommandContext context,
            [Description("Member that is to be banned")]
            DiscordMember member,
            [Description("Reason for the ban")] string reason)
        {
            await context.TriggerTypingAsync();
            
            var embed = new DiscordEmbedBuilder
            {
                Title = "Member banned",
                Description = $"{context.User} you used :hammer: on {member} for {reason}"
            };

            await context.RespondAsync("", false, embed);
            await member.SendMessageAsync($"You've been banned from {context.Guild} for {reason}");
            await context.Guild.BanMemberAsync(member, 0, reason);
        }
    }
}