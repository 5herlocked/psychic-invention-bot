﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;

namespace RoleBot
{
	internal class Program
	{
		public static DiscordClient Client { get; private set; }
		private CommandsNextModule Commands { get; set; }
		private DiscordMessage TargetMessage { get; set; }
		private DiscordChannel TargetChannel { get; set; }
		private List<DiscordRole> RolesToAssign { get; set; } // split from configuration file
		private List<DiscordEmoji> EmojisToAssign { get; set; } // split from configuration file


		private static void Main()
		{
			var proc = new Program();
			proc.RunBotAsync().GetAwaiter().GetResult();
		}

		private async Task RunBotAsync()
		{
			var clientConfig = (new DiscordConfiguration
			{
				AutoReconnect = true,
				Token = ConfigurationManager.AppSettings.Get("token"),
				TokenType = TokenType.Bot,

				LogLevel = LogLevel.Debug,
				UseInternalLogHandler = true
			});

			// instantiated the client
			Client = new DiscordClient(clientConfig);
			
			// logs before bot is live
			Client.Ready += Client_Ready;
			Client.GuildAvailable += Guild_Available;
			Client.ClientErrored += Client_Error;

			var commandsConfig = new CommandsNextConfiguration
			{
				// sets the prefix for messages to be treated as commands
				StringPrefix = ConfigurationManager.AppSettings.Get("commandPrefix"),
				// sets the ability of the bot to send DMS to people in the server
				EnableDms = true,
				// allows Bot-Mention to be a valid command prefix
				EnableMentionPrefix = true
			};

			// connects discord client to commands module
			Commands = Client.UseCommandsNext(commandsConfig);

			// making Console a little more lively with debug-style
			Commands.CommandExecuted += Commands_CommandExecuted;
			Commands.CommandErrored += Commands_CommandError;
			
			// Register the commands for usage with RoleBot
			Commands.RegisterCommands<Commands>();
			Commands.SetHelpFormatter<HelpFormatter>();

			// Set target Channel and message to track through ReactionRole duties
			TargetChannel = Client.GetChannelAsync(UInt64.Parse(ConfigurationManager.AppSettings.Get("targetChannel")))
				.Result;
			TargetMessage = TargetChannel
				.GetMessageAsync(UInt64.Parse(ConfigurationManager.AppSettings.Get("targetMessage"))).Result;

			Client.MessageReactionAdded += Reaction_Added;
			Client.MessageReactionRemoved += Reaction_Removed;
			
			var roleId = ConfigurationManager.AppSettings.Get("rolesToAssign").Trim().Split(',');
			RolesToAssign = new List<DiscordRole>();
			foreach (var id in roleId)
			{
				var toAssign = TargetChannel.Guild.GetRole(UInt64.Parse(id));
				RolesToAssign.Add(toAssign);
			}

			var emojiId = ConfigurationManager.AppSettings.Get("emotesToRoles").Trim().Split(',');
			EmojisToAssign = new List<DiscordEmoji>();
			foreach (var id in emojiId)
			{
				EmojisToAssign.Add(DiscordEmoji.FromName(Client, id));
			}
			
			await Client.ConnectAsync();
			await Task.Delay(-1);
		}

		private async Task Reaction_Added(MessageReactionAddEventArgs e)
		{
			if (e.Message.Equals(TargetMessage))
			{
				var guild = TargetChannel.Guild;
				
				// all members who reacted
				var membersReacted = from discordUser in TargetMessage.GetReactionsAsync(e.Emoji).Result
					select guild.GetMemberAsync(discordUser.Id).Result;

				// Grants roles retroactively through the use of a switch based on the emote used
				foreach (var member in membersReacted)
				{
					for (var i = 0; i < EmojisToAssign.Count; i++)
					{
						if (e.Emoji.Equals(EmojisToAssign[i]))
							await member.GrantRoleAsync(RolesToAssign[i]);
					}
				}
			}
		}

		private async Task Reaction_Removed(MessageReactionRemoveEventArgs e)
		{
			if (e.Message.Equals(TargetMessage))
			{
				var guild = TargetChannel.Guild;
				
				// retro actively tries to remove roles (created in case bot goes offline)
				var guildMembers = guild.GetAllMembersAsync().Result;

				var membersReacted = from discordUser in TargetMessage.GetReactionsAsync(e.Emoji).Result
					select guild.GetMemberAsync(discordUser.Id).Result;

				// filters members to remove
				var membersToRemove = from discordMember in guildMembers
					where !membersReacted.Contains(discordMember)
					select discordMember;
				
				// retroactively removes roles
				foreach (var member in membersToRemove)
				{
					switch (e.Emoji.Name)
					{
						case "nut":
							await member.RevokeRoleAsync(guild.GetRole(485989904132603927), "by choice"); // nut role
							break;
						case "scientist":
							await member.RevokeRoleAsync(guild.GetRole(486006468034822164),
								"by choice"); // scientist role
							break;
					}
				}
			}
		}

		// Log Maintenance

		// Client Ready
		private static Task Client_Ready(ReadyEventArgs e)
		{
			// log - client ready
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot", "Client is ready to process events.",
				DateTime.Now);

			return Task.CompletedTask;
		}

		// Client has Guild Available and prints the Guild Name
		private static Task Guild_Available(GuildCreateEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot", $"Guild available: {e.Guild.Name}", DateTime.Now);

			return Task.CompletedTask;
		}

		// Client has erred and cannot act
		private static Task Client_Error(ClientErrorEventArgs e)
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
		private static Task Commands_CommandExecuted(CommandExecutionEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot",
				$"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

			return Task.CompletedTask;
		}

		// Client has failed to execute a command
		private static async Task Commands_CommandError(CommandErrorEventArgs e)
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