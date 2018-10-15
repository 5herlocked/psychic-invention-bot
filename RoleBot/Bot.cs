// Author: Shardul Vaidya
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace RoleBot
{
    internal static class Bot
    {
        private static readonly XDocument Config = XDocument.Load("config.xml"); // Config file loaded
        
        internal static DiscordClient Client { get; private set; } // Discord API Client
        
        internal static List<RoleWatch> RolesToWatch { get; set; } // Roles To Watch
        
        private static bool CommandsFlag { get; set; } // Flag to enable or disable commands
        
        private static CommandsNextModule Commands { get; set; }
        
        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);
        
        // instance vars for logs
//        internal static readonly string Path =
//            System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"/\"log{DateTime.UtcNow}.txt\""; //log file path
//        private static FileStream _fileStream; //file stream for printing
//        private static StreamWriter _log;

        internal static async Task<string> RunBotAsync()
        {
            Console.CancelKeyPress += (sender, args) =>
            {
                QuitEvent.Set();
                args.Cancel = true;
            };
            
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

            if (CommandsFlag)
            {
                var commandsConfig = new CommandsNextConfiguration
                {
                    StringPrefix = "r!",

                    EnableDms = true,

                    EnableMentionPrefix = true
                };
            
                // Command Modules and construction
                Commands = Client.UseCommandsNext(commandsConfig);
            
                // command events
                Commands.CommandExecuted += LogPrinter.CommandExecuted;
                Commands.CommandErrored += LogPrinter.CommandErred;
            
                // registering the commands
                Commands.RegisterCommands<Commands>();
                Commands.SetHelpFormatter<HelpFormatter>();    
            }
            
            // logs before bot is live
            Client.Ready += LogPrinter.Client_Ready;
            Client.GuildAvailable += LogPrinter.Guild_Available;
            Client.ClientErrored += LogPrinter.Client_Error;
            
            // The actual event handlers
            Client.MessageReactionAdded += Reaction_Added;
            Client.MessageReactionRemoved += Reaction_Removed;

            // Writing log to file
//            Client.DebugLogger.LogMessageReceived += (sender, e) =>
//            {
//                if (!File.Exists(Path)) File.CreateText(Path);
//                _fileStream = new FileStream(Path, FileMode.Append);
//                _log = new StreamWriter(_fileStream);
//                
//                _log.WriteLineAsync(
//                    $"[{e.Timestamp.ToString(CultureInfo.CurrentCulture)}][{e.Application}][{e.Level}][{e.Message}]");
//            };
            
            await Client.ConnectAsync();
            QuitEvent.WaitOne();
            
            Client.DebugLogger.LogMessage(LogLevel.Critical, "RoleBot", "End Signal Received Bot Terminating", DateTime.UtcNow);
            
            await UpdateConfigFile();
            
            return "Bot done";
        }

        internal static Task RefreshConfig()
        {
            // Sets Guilds to observe
            var guildId = Config.Root?.Element("Guilds")?.Value.Split(",");
            Guilds = new List<DiscordGuild>();
            if (guildId == null) return Task.FromException(new Exception("Pleases set Guilds"));
            {
                foreach (var id in guildId)
                {
                    var toWatch = Client.GetGuildAsync(UInt64.Parse(id)).Result;
                    Guilds.Add(toWatch);
                }
            }
            
            // Sets Channels to observe
            var channelId = Config.Root?.Element("Channels")?.Value.Split(",");
            Channels = new List<DiscordChannel>();
            if (channelId == null) return Task.FromException(new Exception("Please set Channels"));
            {
                for (var i = 0; i < channelId.Length; i++)
                {
                    var toWatch = Guilds[i].GetChannel(UInt64.Parse(channelId[i]));
                    Channels.Add(toWatch);
                }
            }
            
            // Sets Messages to observe
            var messageId = Config.Root?.Element("Messages")?.Value.Split(",");
            Messages = new List<DiscordMessage>();
            if (messageId == null) return Task.FromException(new Exception("Please set Messages"));
            {
                for (var i = 0; i < messageId.Length; i++)
                {
                    var toWatch = Channels[i].GetMessageAsync(UInt64.Parse(messageId[i])).Result;
                    Messages.Add(toWatch);
                }
            }
            
            // Sets Roles to manage
            var roleId = Config.Root?.Element("Roles")?.Elements("Channel").ToArray();
            Roles = new List<List<DiscordRole>>();
            if (roleId == null) return Task.FromException(new Exception("Please set Roles"));
            {
                for (var i = 0; i < roleId.Length; i++)
                {
                    var channelRoles = roleId[i].Value.Split(",");
                    
                    // Linq Gets Role from uID using select
                    var channelRole = channelRoles.Select(id => Guilds[i].GetRole(UInt64.Parse(id))).ToList();
                    Roles.Add(channelRole);
                }
            }
            
            // Sets Emojis to watch
            var emoteId = Config.Root?.Element("Emotes")?.Elements("Channel").ToArray();
            Emotes = new List<List<DiscordEmoji>>();
            if (emoteId == null) return Task.FromException(new Exception("Please set Emotes"));
            {
                for (var i = 0; i < emoteId.Length; i++)
                {
                    var channelEmotes = emoteId[i].Value.Split(",");
                    
                    // Linq gets list of Discord Guild Emotes based on uid and casts selected as DiscordEmojis
                    var channelEmote = channelEmotes.Select(id => Guilds[i].GetEmojiAsync(UInt64.Parse(id)).Result)
                        .Cast<DiscordEmoji>().ToList();
                    Emotes.Add(channelEmote);
                }
            }

            var commandsFlag = Config.Root?.Element("Commands")?.ToString().ToLower();
            if (commandsFlag != null) CommandsFlag = commandsFlag.Equals("true");

            return Task.CompletedTask;
        }

        private static async Task Reaction_Added(MessageReactionAddEventArgs e)
        {
            if (Messages.Contains(e.Message))
            {
                // gets the reference of the guild, message and channel to be used from config file
                var index = Messages.FindIndex(a => a.Id == e.Message.Id);
                var guild = Guilds[index];
                
                // gets the members who've reacted
                var membersReacted = from discordUser in e.Message.GetReactionsAsync(e.Emoji).Result
                    select guild.GetMemberAsync(discordUser.Id).Result;

                // Grants roles retroactively
                foreach (var member in membersReacted)
                    for (var i = 0; i < Emotes[index].Count; i++)
                        if (e.Emoji.Equals(Emotes[index][i]))
                        {
                            if (member.Roles.Contains(Roles[index][i])) continue;
                            await member.GrantRoleAsync(Roles[index][i]);
                            await LogPrinter.Role_Assigned(e, member, Roles[index][i]);
                        }

            }
        }

        private static async Task Reaction_Removed(MessageReactionRemoveEventArgs e)
        {
            if (Messages.Contains(e.Message))
            {
                // gets the index used to access the right guild/message/channel
                var index = Messages.FindIndex(a => a.Id == e.Message.Id);
                var guild = Guilds[index];

                // retro actively tries to remove roles (created in case bot goes offline)
                var guildMembers = guild.GetAllMembersAsync().Result;

                var membersReacted = from discordUser in e.Message.GetReactionsAsync(e.Emoji).Result
                    select guild.GetMemberAsync(discordUser.Id).Result;

                // filters members to remove
                var membersToRemove = from discordMember in guildMembers
                    where !membersReacted.Contains(discordMember)
                    select discordMember;

                // retroactively removes roles
                foreach (var member in membersToRemove)
                    for (var i = 0; i < Emotes[index].Count; i++)
                        if (e.Emoji.Equals(Emotes[index][i]))
                        {
                            if (!member.Roles.Contains(Roles[index][i])) continue;
                            await member.RevokeRoleAsync(Roles[index][i]);
                            await LogPrinter.Role_Revoked(e, member, Roles[index][i]);
                        }
            }
        }
        
        // updates config files whenever needed
        internal static Task UpdateConfigFile()
        {
            // root of the XML file
            var root = Config.Root;
            
            // updating channels
            var channels = root?.Element("Channels");
            if (channels != null) channels.Value = Channels.Select(chn => chn.Id).ToString();
            
            // updating roles
            var role = root?.Element("Roles");
            for (var i = 0; i < Roles.Count; i++)
            {
                var guildSpec = role?.Elements().ToList();

                if (guildSpec != null) guildSpec[i].Value = Roles[i].Select(c => c.Id).ToString();
            }
            
            // updating emotes
            var emotes = root?.Element("Emotes");
            for (var i = 0; i < Emotes.Count; i++)
            {
                var guildSpec = role?.Elements().ToList();

                if (guildSpec != null) guildSpec[i].Value = Emotes[i].Select(c => c.Id).ToString();
            }

            Config.Save(Assembly.GetExecutingAssembly().Location + "config.xml");

            return Task.CompletedTask;
        }
    }
}