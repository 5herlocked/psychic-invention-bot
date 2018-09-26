using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace RoleBot
{
    public class LogPrinter
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
		internal static Task Guild_Available(GuildCreateEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot", $"Guild available: {e.Guild.Name}", DateTime.Now);

			return Task.CompletedTask;
		}

		// Client has erred and cannot act
		internal static Task Client_Error(ClientErrorEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Error, "Rolebot",
				$"Exception Occured: {e.Exception.GetType()} {e.Exception.Message})", DateTime.Now);

			if (!(e.Exception is AggregateException aex)) return Task.CompletedTask;
			foreach (var iex in aex.InnerExceptions)
			{
				e.Client.DebugLogger.LogMessage(LogLevel.Error, "RoleBot", $"Inner Exceptions are: {iex.GetType()} {iex.Message}", DateTime.Now);
			}

			return Task.CompletedTask;
		}
		
	    // Client has assigned a role
	    internal static Task Role_Assigned(MessageReactionAddEventArgs e, DiscordMember member, DiscordRole role)
	    {
		    e.Client.DebugLogger.LogMessage(LogLevel.Info, "Rolebot", $"User: {member} Role Assigned: {role}", DateTime.Now);
		    
		    return Task.CompletedTask;
	    }
	
	    // Client has revoked a role
	    internal static Task Role_Revoked(MessageReactionRemoveEventArgs e, DiscordMember member, DiscordRole role)
	    {
		    e.Client.DebugLogger.LogMessage(LogLevel.Info, "Rolebot", $"User: {member} Role Revoked: {role}", DateTime.Now);
		    
		    return Task.CompletedTask;
	    }
    }
}