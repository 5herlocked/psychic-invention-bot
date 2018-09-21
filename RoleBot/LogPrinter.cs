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
			// Gets roles to be assigned/revoked
			var roleId = ConfigurationManager.AppSettings.Get("rolesToAssign").Trim().Split(',');
			Bot.RolesToAssign = new List<DiscordRole>();
			foreach (var id in roleId)
			{
				var toAssign = Bot.TargetChannel.Guild.GetRole(UInt64.Parse(id));
				Bot.RolesToAssign.Add(toAssign);
			}
			
			// Gets emotes to watch
			var emojiId = ConfigurationManager.AppSettings.Get("emotesToRoles").Trim().Split(',');
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

		// Client has successfully executed a command
		internal static Task Commands_CommandExecuted(CommandExecutionEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot",
				$"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

			return Task.CompletedTask;
		}

		// Client has failed to execute a command
		internal static async Task Commands_CommandError(CommandErrorEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "Rolebot",
				$"{e.Context.User.Username} attempted to execute '{e.Command.QualifiedName ?? "<no message>"}' but it threw: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
				DateTime.Now);

			if (e.Exception is ChecksFailedException)
			{
				// yes, the user lacks required permissions, 
				// let them know

				var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

				// let's wrap the response into an embed
				var embed = new DiscordEmbedBuilder
				{
					Title = "Access denied",
					Description = $"{emoji} You do not have the permissions required to execute this command.",
					Color = new DiscordColor(0xFF0000) // red
					// there are also some pre-defined colors available
					// as static members of the DiscordColor struct
				};
				await e.Context.RespondAsync("", embed: embed);
			}
		}
    }
}