//@author Shardul Vaidya
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace RoleBot
{
    public static class LogPrinter
    {
        // Log Maintenance
        // Client Ready
        internal static Task Client_Ready(ReadyEventArgs e)
        {
            // log - client ready
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot", "Client is ready to process events.",
                DateTime.Now);
            return Task.CompletedTask;
        }

        // Client has Guild Available and prints the Guild Name
        internal static async Task Guild_Available(GuildCreateEventArgs e)
        {
            await Bot.RefreshConfig();
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot", $"Guild available: {e.Guild.Name}", DateTime.Now);
        }

        // Client has erred and cannot act
        internal static Task Client_Error(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "Rolebot",
                $"Exception Occured: {e.Exception.GetType()} {e.Exception.Message})", DateTime.Now);

            if (!(e.Exception is AggregateException aex)) return Task.CompletedTask;
            foreach (var iex in aex.InnerExceptions)
                e.Client.DebugLogger.LogMessage(LogLevel.Error, "RoleBot",
                    $"Inner Exceptions are: {iex.GetType()} {iex.Message}", DateTime.Now);

            return Task.CompletedTask;
        }
        
        // Client has assigned a role
        internal static Task Role_Assigned(MessageReactionAddEventArgs e, DiscordMember member, DiscordRole role)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Debug, "Rolebot", $"User: {member} Role Assigned: {role}",
                DateTime.Now);

            return Task.CompletedTask;
        }

        // Client has revoked a role
        internal static Task Role_Revoked(MessageReactionRemoveEventArgs e, DiscordMember member, DiscordRole role)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Debug, "Rolebot", $"User: {member} Role Revoked: {role}",
                DateTime.Now);

            return Task.CompletedTask;
        }
        
        // user executed a command
        internal static Task CommandExecuted(CommandExecutionEventArgs e)
        {
            // logs the name and the command executed
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot",
                $"{e.Context.User.Username} successfully executed {e.Command.QualifiedName}", DateTime.Now);
            
            // not async so
            return Task.CompletedTask;
        }
        
        // user attempted to execute a command
        internal static async Task CommandErred(CommandErrorEventArgs e)
        {
            // logs name and command erred
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "RoleBot",
                $"{e.Context.User.Username} attempted to execute {e.Command.QualifiedName}", DateTime.Now);
            
            // if it's due to a lack of required permissions on behalf of the user
            if (e.Exception is ChecksFailedException)
            {
                var embedded = new DiscordEmbedBuilder
                {
                    Title = "Lack of Permission",
                    Description =
                        $"{e.Context.User} does not have the required permissions to execute this command " +
                        $"{Formatter.Bold(e.Context.Command.QualifiedName)}"
                };

                await e.Context.RespondAsync("", false, embedded);
            }
        }
        
        // Reaction Role was created
        internal static Task Role_Created(DiscordRole role, DiscordEmoji emoji)
        {
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot",
                $"{role.Name} is being watched through the emoji {emoji.Name}", DateTime.Now);
            return Task.CompletedTask;
        }
        
        // Reaction role deleted
        internal static Task Role_Removed(DiscordRole role)
        {
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot", $"{role.Name} is no longer being watched",
                DateTime.Now);
            
            return Task.CompletedTask;
        }
    }
}