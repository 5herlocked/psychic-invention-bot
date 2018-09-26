using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace RoleBot
{
	internal class Bot
	{
		private static DiscordClient Client { get; set; }
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

			// Set target Channel and message to track through ReactionRole duties
			TargetChannel = Client.GetChannelAsync(UInt64.Parse(ConfigurationManager.AppSettings.Get("targetChannel")))
				.Result;
			TargetMessage = TargetChannel
				.GetMessageAsync(UInt64.Parse(ConfigurationManager.AppSettings.Get("targetMessage"))).Result;

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
					for (var i = 0; i < EmojisToAssign.Count; i++)
						if (e.Emoji.Equals(EmojisToAssign[i]))
						{
							await member.GrantRoleAsync(RolesToAssign[i]);
							await LogPrinter.Role_Assigned(e, member, RolesToAssign[i]);
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
					for (var i = 0; i < EmojisToAssign.Count; i++)
						if (e.Emoji.Equals(EmojisToAssign[i]))
						{
							await member.RevokeRoleAsync(RolesToAssign[i]);
							await LogPrinter.Role_Revoked(e, member, RolesToAssign[i]);
						}
			}
		}
	}
}