﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace RoleBot
{
    internal class Bot
    {
        private static readonly XDocument Config = XDocument.Load("config.xml");
        private static DiscordClient Client { get; set; }
        private static DiscordMessage TargetMessage { get; set; }
        private static DiscordChannel TargetChannel { get; set; }
        private static List<DiscordRole> RolesToAssign { get; set; } // split from configuration file
        private static List<DiscordEmoji> EmojisToAssign { get; set; } // split from configuration file

        internal static async Task RunBotAsync()
        {
            if (Config.Root != null)
            {
                var clientConfig = new DiscordConfiguration
                {
                    Token = Config.Root.Element("Token")?.Value,
                    TokenType = TokenType.Bot,

                    LogLevel = LogLevel.Debug,
                    UseInternalLogHandler = true
                };

                // instantiated the client
                Client = new DiscordClient(clientConfig);
            }

            // logs before bot is live
            Client.Ready += LogPrinter.Client_Ready;
            Client.GuildAvailable += LogPrinter.Guild_Available;
            Client.ClientErrored += LogPrinter.Client_Error;

            // Set target Channel and message to track through ReactionRole duties
            TargetChannel = Client.GetChannelAsync(ulong.Parse(Config.Root?.Element("TargetChannel")?.Value)).Result;
            TargetMessage = TargetChannel.GetMessageAsync(ulong.Parse(Config.Root?.Element("TargetMessage")?.Value))
                .Result;

            Client.MessageReactionAdded += Reaction_Added;
            Client.MessageReactionRemoved += Reaction_Removed;

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        internal static Task RefreshConfig()
        {
            // Set target Channel and message to track through ReactionRole duties
            TargetChannel = Client.GetChannelAsync(ulong.Parse(Config.Root?.Element("TargetChannel")?.Value)).Result;
            TargetMessage = TargetChannel.GetMessageAsync(ulong.Parse(Config.Root?.Element("TargetMessage")?.Value))
                .Result;

            // Gets roles to be watched
            var roleId = Config.Root?.Element("Roles")?.Value.Split(",");
            RolesToAssign = new List<DiscordRole>();
            if (roleId == null) return Task.FromException(new Exception("Role unrecognised"));
            {
                foreach (var id in roleId)
                {
                    var toAssign = TargetChannel.Guild.GetRole(ulong.Parse(id));
                    RolesToAssign.Add(toAssign);
                }
            }

            // Gets emotes to watch
            var emojiId = Config.Root?.Element("Emotes")?.Value.Split(",");
            EmojisToAssign = new List<DiscordEmoji>();
            if (emojiId == null) return Task.FromException(new Exception("Emote unrecognised"));
            {
                foreach (var id in emojiId)
                {
                    var toAssign = TargetChannel.Guild.GetEmojiAsync(ulong.Parse(id)).Result;
                    EmojisToAssign.Add(toAssign);
                }
            }

            return Task.CompletedTask;
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