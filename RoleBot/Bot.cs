//@author Shardul Vaidya
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;

namespace RoleBot
{
    internal class Bot
    {
        private const string ConfigPath = "config.xml";
        
        internal static Config Config { get; set; }

        internal static DiscordClient Client { get; private set; } // Discord API Client
        
        private static CommandsNextModule Commands { get; set; } // Commands Next Module for interactivity and config
        
        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false); // End Signal Watcher
        
        // instance vars for logs
        private static readonly string
            LogPath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");
        private static readonly FileStream FileStream = new FileStream(LogPath, FileMode.Append); //file stream for printing
        private static readonly StreamWriter Log = new StreamWriter(FileStream);

        internal static async Task<string> RunBotAsync()
        {
            /*
             * Sets up the End Signal Watcher to allow for safe termination of the bot
             */
            Console.CancelKeyPress += (sender, args) =>
            {
                QuitEvent.Set();
                args.Cancel = true;
            };

            await RefreshConfig();
            
            /*
             * Creates the Discord Configuration and instantiates the DiscordClient if the Config File is not empty
             *
             * Get Token from Config File
             * States that user is a bot
             * Then states the log level for the instantiation and creation of the client
             */
            var clientConfig = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,

                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            /*
             * Instantiates the client
             */
            Client = new DiscordClient(clientConfig);

            
            /*
             * Configures the Commands Module with the Command Prefix and states that the bot is able to send DMs to
             * members.
             * Also configures it to be possible to use a bot mention as a Command Prefix
             */
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefix = "r!",

                EnableDms = true,

                EnableMentionPrefix = true
            };

            // Command Modules and construction
            Commands = Client.UseCommandsNext(commandsConfig);

            // Command events Log
            Commands.CommandExecuted += LogPrinter.CommandExecuted;
            Commands.CommandErrored += LogPrinter.CommandErred;

            // registering the commands
            Commands.RegisterCommands<RoleCommands>();
            Commands.RegisterCommands<OwnerCommands>();
            Commands.SetHelpFormatter<HelpFormatter>();
            
            // logs before bot is live
            Client.Ready += LogPrinter.Client_Ready;
            Client.GuildAvailable += LogPrinter.Guild_Available;
            Client.ClientErrored += LogPrinter.Client_Error;
            
            // The actual event handlers for Reaction Management
            Client.MessageReactionAdded += Reaction_Added;
            Client.MessageReactionRemoved += Reaction_Removed;

            // Writing log to file
            Client.DebugLogger.LogMessageReceived += (sender, e) =>
            {
                Log.WriteLineAsync(
                    $"[{e.Timestamp.ToString(CultureInfo.CurrentCulture)}][{e.Application}][{e.Level}] {e.Message}");
            };
            
            
            // async operation of the bot allows for consistent performance regardless of the load
            await Client.ConnectAsync();
            
            // Puts current thread to sleep until an End Signal is received (Ctrl + C)
            QuitEvent.WaitOne();
            
            // Indicates that bot is being terminated
            Client.DebugLogger.LogMessage(LogLevel.Critical, "RoleBot", "End Signal Received Bot Terminating", DateTime.UtcNow);
            
            // Updates the config file with latest changes to make them permanent
            await UpdateConfigFile();
            
            // Releases the Log file from the Program
            Log.Close();
            FileStream.Close();
            
            return "Bot done";
        }
        
        /* Refresh Config Method
         *
         * Used to load the configuration into the assembly
         * Gets each <Role> Node from config.xml and deserializes them into RoleWatch Objects
         */
        internal static Task RefreshConfig()
        {
            using (var reader = new StreamReader(ConfigPath))
            {
                var serializer  = new XmlSerializer(typeof(Config));
                
                Config = serializer.Deserialize(reader) as Config;
            }
            
            return Task.CompletedTask;
        }

        private static async Task Reaction_Added(MessageReactionAddEventArgs e)
        {
            var roleExists = Config.RolesToWatch.Select(r => r.Message).ToList().Contains(e.Message);
            
            // filters out spare emotes
            if (roleExists)
            {
                // get the role to assign
                var roleToAssign = from roles in Config.RolesToWatch
                    where roles.Emoji.Equals(e.Emoji)
                    select roles;
                roleToAssign = roleToAssign.ToList();
                
                // get members who've reacted from the users who've reacted
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
            var roleExists = Config.RolesToWatch.Select(r => r.Message).ToList().Contains(e.Message);
            
            if (roleExists)
            {
                // selects the role that is supposed to be revoked
                var roleToRevoke = from roles in Config.RolesToWatch
                    where roles.Emoji.Equals(e.Emoji)
                    select roles;
                roleToRevoke = roleToRevoke.ToList();
                
                if (!Config.AutoRemoveFlag)
                {
                    // retro actively tries to remove roles (created in case bot goes offline)
                    var guildMembers = roleToRevoke.First().Guild.GetAllMembersAsync().Result;
                    
                    // gets all the members reacted then gets members from the guild
                    var membersReacted = from discordUser in e.Message.GetReactionsAsync(e.Emoji).Result
                        select roleToRevoke.First().Guild.GetMemberAsync(discordUser.Id).Result;

                    // filters members to remove
                    // select member from guild members where reacted members doesn't contain the member
                    var membersToRemove = from discordMember in guildMembers
                        where !membersReacted.Contains(discordMember)
                        select discordMember;

                    // retroactively removes roles
                    foreach (var member in membersToRemove)
                    {
                        // optimisation to reduce iterations if member doesn't have the role
                        if (!member.Roles.Contains(roleToRevoke.First().Role)) continue;

                        await member.RevokeRoleAsync(roleToRevoke.First().Role);
                        await LogPrinter.Role_Revoked(e, member, roleToRevoke.First().Role);
                    }   
                }
                else
                {
                    var member = roleToRevoke.First().Guild.GetMemberAsync(e.User.Id).Result;
                    await member.RevokeRoleAsync(roleToRevoke.First().Role);
                    await LogPrinter.Role_Revoked(e, member, roleToRevoke.First().Role);
                }
            }
        }
        
        // updates config files whenever needed
        /*
         internal static Task UpdateConfigFile()
        {
            using (var writer = XmlWriter.Create("config.xml", new XmlWriterSettings {Indent = true}))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Config");
                writer.WriteElementString("Token", Config.Root?.Element("Token")?.Value);
                writer.WriteElementString("AutoRemove", Config.AutoRemoveFlag.ToString());
                
                foreach (var role in Config.RolesToWatch)
                {
                    writer.WriteStartElement("Roles");
                    writer.WriteComment("Guild: " + role.Guild.Name);
                    writer.WriteComment("Channel: " + role.Channel);
                    writer.WriteComment("Message: " + role.Message);
                    writer.WriteComment("Emoji: " + role.Emoji);
                    writer.WriteComment("Role: " + role.Role.Name);
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
        */

        internal static Task UpdateConfigFile()
        {
            var serializer = new XmlSerializer(typeof(Config), new[]{typeof(RoleWatch)});
            //var rSerializer = new XmlSerializer(typeof(RoleWatch));
            
            using (var writer = XmlWriter.Create("config.xml", new XmlWriterSettings {Indent = true}))
            {
                writer.WriteStartDocument();
                serializer.Serialize(writer, Config);
                writer.WriteEndDocument();
            }
            
            return Task.CompletedTask;
        }
    }
}