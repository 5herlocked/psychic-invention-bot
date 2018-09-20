using System;
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
	internal class Bot
	{
		internal static DiscordClient Client { get; private set; }
		private static CommandsNextModule Commands { get; set; }
		private static DiscordMessage TargetMessage { get; set; }
		internal static DiscordChannel TargetChannel { get; private set; }
		internal static List<DiscordRole> RolesToAssign { get; set; } // split from configuration file
		internal static List<DiscordEmoji> EmojisToAssign { get; set; } // split from configuration file

		internal static async Task RunBotAsync()
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
			Client.Ready += LogPrinter.Client_Ready;
			Client.GuildAvailable += LogPrinter.Guild_Available;
			Client.ClientErrored += LogPrinter.Client_Error;

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
			Commands.CommandExecuted += LogPrinter.Commands_CommandExecuted;
			Commands.CommandErrored += LogPrinter.Commands_CommandError;
			
			// Register the commands for usage with RoleBot
			Commands.RegisterCommands<Commands>();
			Commands.SetHelpFormatter<HelpFormatter>();

			// Set target Channel and message to track through ReactionRole duties
			TargetChannel = Client.GetChannelAsync(ulong.Parse(ConfigurationManager.AppSettings.Get("targetChannel")))
				.Result;
			TargetMessage = TargetChannel
				.GetMessageAsync(ulong.Parse(ConfigurationManager.AppSettings.Get("targetMessage"))).Result;

			Client.MessageReactionAdded += Reaction_Added;
			Client.MessageReactionRemoved += Reaction_Removed;
			
			await Client.ConnectAsync();
			await Task.Delay(-1);
		}

		private static async Task Reaction_Added(MessageReactionAddEventArgs e)
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

		private static async Task Reaction_Removed(MessageReactionRemoveEventArgs e)
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
					for (var i = 0; i < EmojisToAssign.Count; i++)
						if (e.Emoji.Equals((EmojisToAssign[i])))
							await member.RevokeRoleAsync(RolesToAssign[i]);
				}
			}
		}
	}
}