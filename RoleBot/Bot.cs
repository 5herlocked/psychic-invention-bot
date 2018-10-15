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
            var root = Config.Root;

            if (root != null)
            {
                foreach (var roles in root.Elements())
                {
                    var guild = Client.GetGuildAsync(UInt64.Parse(roles.Element("Guild")?.Value)).Result;
                    var channel = guild.GetChannel(UInt64.Parse(roles.Element("Channel")?.Value));
                    var message = channel.GetMessageAsync(UInt64.Parse(roles.Element("Message")?.Value));
                    var emoji = guild.GetEmojiAsync(UInt64.Parse(roles.Element("Emoji")?.Value))
                    
                    RolesToWatch.Add(new RoleWatch());
                }

                var commandsFlag = root?.Element("Commands")?.ToString().ToLower();
                if (commandsFlag != null) CommandsFlag = commandsFlag.Equals("true");
            }

            return Task.CompletedTask;
        }

        private static async Task Reaction_Added(MessageReactionAddEventArgs e)
        {
            var roleExists = RolesToWatch.Select(r => r.Message).ToList().Contains(e.Message);
            
            // filters out spare emotes
            if (roleExists)
            {
                // get the role to assign
                var roleToAssign = from roles in RolesToWatch
                    where roles.Emoji.Equals(e.Emoji)
                    select roles;
                roleToAssign = roleToAssign.ToList();
                
                // get members who've reacted
                var membersReacted = from discordUser in e.Message.GetReactionsAsync(e.Emoji).Result
                    select roleToAssign.First().Guild.GetMemberAsync(discordUser.Id).Result;
                
                // retroactively assigns roles
                foreach (var member in membersReacted)
                {
                    // optimisation to reduce iterations if member already has the role
                    if(member.Roles.Contains(roleToAssign.First().Role)) continue;
                    
                    await member.GrantRoleAsync(roleToAssign.First().Role);
                    await LogPrinter.Role_Assigned(e, member, roleToAssign.First().Role);
                }
            }
        }

        private static async Task Reaction_Removed(MessageReactionRemoveEventArgs e)
        {
            var roleExists = RolesToWatch.Select(r => r.Message).ToList().Contains(e.Message);
            
            if (roleExists)
            {
                var roleToRevoke = from roles in RolesToWatch
                    where roles.Emoji.Equals(e.Emoji)
                    select roles;
                roleToRevoke = roleToRevoke.ToList();
                
                // retro actively tries to remove roles (created in case bot goes offline)
                var guildMembers = roleToRevoke.First().Guild.GetAllMembersAsync().Result;

                var membersReacted = from discordUser in e.Message.GetReactionsAsync(e.Emoji).Result
                    select roleToRevoke.First().Guild.GetMemberAsync(discordUser.Id).Result;

                // filters members to remove
                var membersToRemove = from discordMember in guildMembers
                    where !membersReacted.Contains(discordMember)
                    select discordMember;

                // retroactively removes roles
                foreach (var member in membersToRemove)
                {
                    if (member.Roles.Contains(roleToRevoke.First().Role)) continue;

                    await member.RevokeRoleAsync(roleToRevoke.First().Role);
                    await LogPrinter.Role_Revoked(e, member, roleToRevoke.First().Role);
                }
            }
        }
        
        // updates config files whenever needed
        internal static Task UpdateConfigFile()
        {
            // root of the XML file
            var root = Config.Root;
            
            Config.Save(Assembly.GetExecutingAssembly().Location + "config.xml");

            return Task.CompletedTask;
        }
    }
}