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
			// Set target Channel and message to track through ReactionRole duties
			Bot.TargetChannel = Bot.Client.GetChannelAsync(UInt64.Parse(Bot.Config.Root?.Element("TargetChannel")?.Value)).Result;
			Bot.TargetMessage = Bot.TargetChannel.GetMessageAsync(UInt64.Parse(Bot.Config.Root?.Element("TargetMessage")?.Value)).Result;

			// Gets roles to be watched
			var roleId = Bot.Config.Root?.Element("Roles")?.Value.Split(",");
			Bot.RolesToAssign = new List<DiscordRole>();
			foreach (var id in roleId)
			{
				var toAssign = Bot.TargetChannel.Guild.GetRole(ulong.Parse(id));
				Bot.RolesToAssign.Add(toAssign);
			}

				// Gets emotes to watch
			var emojiId = Bot.Config.Root?.Element("Emotes")?.Value.Split(",");
			Bot.EmojisToAssign = new List<DiscordEmoji>();
			foreach (var id in emojiId)
			{
				var toAssign = Bot.TargetChannel.Guild.GetEmojiAsync(UInt64.Parse(id)).Result;
				Bot.EmojisToAssign.Add(toAssign);
			}
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