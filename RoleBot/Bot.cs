// Author: Shardul Vaidya
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;

namespace RoleBot
{
    internal static class Bot
    {
        private static readonly XDocument Config = XDocument.Load("config.xml"); // Config file loaded
        
        internal static DiscordClient Client { get; private set; } // Discord API Client
        
        internal static List<RoleWatch> RolesToWatch { get; private set; } // Roles To Watch
        
        private static CommandsNextModule Commands { get; set; }
        
        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);
        
        // instance vars for logs
        private static readonly string
            LogPath = Path.Combine(Directory.GetCurrentDirectory(), $"log.txt");
        private static readonly FileStream FileStream = new FileStream(LogPath, FileMode.Append); //file stream for printing
        private static readonly StreamWriter Log = new StreamWriter(FileStream);

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
            
            // logs before bot is live
            Client.Ready += LogPrinter.Client_Ready;
            Client.GuildAvailable += LogPrinter.Guild_Available;
            Client.ClientErrored += LogPrinter.Client_Error;
            
            // The actual event handlers for Reaction Managing
            Client.MessageReactionAdded += Reaction_Added;
            Client.MessageReactionRemoved += Reaction_Removed;

            // Writing log to file
            Client.DebugLogger.LogMessageReceived += (sender, e) =>
            {
                Log.WriteLineAsync(
                    $"[{e.Timestamp.ToString(CultureInfo.CurrentCulture)}][{e.Application}][{e.Level}] {e.Message}");
            };
            
            await Client.ConnectAsync();
            QuitEvent.WaitOne();
            
            Client.DebugLogger.LogMessage(LogLevel.Critical, "RoleBot", "End Signal Received Bot Terminating", DateTime.UtcNow);
            
            await UpdateConfigFile();
            
            Log.Close();
            FileStream.Close();
            
            return "Bot done";
        }

        internal static Task RefreshConfig()
        {
            if (RolesToWatch == null) RolesToWatch = new List<RoleWatch>();
            else RolesToWatch.Clear();
            
            var root = Config.Root;

            if (root == null) return Task.FromException(new NullReferenceException("Looks like config is empty"));
            foreach (var roles in root.Elements("Roles"))
            {
                var guild = roles.Element("Guild")?.Value;
                var channel = roles.Element("Channel")?.Value;
                var message = roles.Element("Message")?.Value;
                var emoji = roles.Element("Emoji")?.Value;
                var role = roles.Element("Role")?.Value;
                    
                RolesToWatch.Add(new RoleWatch(guild, channel, message, emoji, role));
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
                    if (!member.Roles.Contains(roleToRevoke.First().Role)) continue;

                    await member.RevokeRoleAsync(roleToRevoke.First().Role);
                    await LogPrinter.Role_Revoked(e, member, roleToRevoke.First().Role);
                }
            }
        }
        
        // updates config files whenever needed
        internal static Task UpdateConfigFile()
        {
            using (var writer = XmlWriter.Create("config.xml", new XmlWriterSettings {Indent = true}))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Config");
                writer.WriteElementString("Token", Config.Root?.Element("Token")?.Value);
                
                foreach (var role in RolesToWatch)
                {
                    writer.WriteStartElement("Roles");
                    writer.WriteComment(role.Role.Name);
                    writer.WriteComment(role.Guild.Name);
                    writer.WriteElementString("Guild", role.Guild.Id.ToString());
                    writer.WriteElementString("Channel", role.Channel.Id.ToString());
                    writer.WriteElementString("Message", role.Message.Id.ToString());
                    writer.WriteElementString("Emoji", role.Emoji.Id.ToString());
                    writer.WriteElementString("Role", role.Role.Id.ToString());
                    writer.WriteEndElement();
                }
                
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            
            return Task.CompletedTask;
        }
    }
}