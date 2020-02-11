//@author Shardul Vaidya
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System.Collections.Generic;

namespace RoleBot
{
    internal class Bot
    {
        private const string ConfigPath = "config.json";
        
        internal static Config Config { get; set; } // Config Class for the Bot

        internal static DiscordClient Client { get; set; } // Discord API Client
        
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
                UseInternalLogHandler = true,
                AutoReconnect = true,
            };

            /*
             * Instantiates the client
             */
             if (Client == null)
                Client = new DiscordClient(clientConfig);

            
            /*
             * Configures the Commands Module with the Command Prefix and states that the bot is able to send DMs to
             * members.
             * Also configures it to be possible to use a bot mention as a Command Prefix
             */
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefix = Config.CommandPrefix,

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
            Commands.RegisterCommands<AdminCommands>();
            Commands.SetHelpFormatter<HelpFormatter>();
            
            // logs before bot is live
            Client.Ready += LogPrinter.Client_Ready;
            Client.GuildAvailable += LogPrinter.Guild_Available;
            Client.ClientErrored += LogPrinter.Client_Error;
            
            // The actual event handlers for Role Management
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

            Client.DebugLogger.LogMessage(LogLevel.Info, "RoleBot", "Log dump completed, Config file updated", DateTime.UtcNow);

            // Updates the config file with latest changes to make them permanent
            await UpdateConfigFile();
            
            // Releases the Log file from the Program
            Log.Close();
            FileStream.Close();
            
            return "Bot done";
        }
        
        /* Reaction_Added Event Handler
         *
         * Checks if the message and emoji are being watched to improve efficiency
         *
         * If emoji and message are being watched, then selects all members who've reacted to the message with the emoji
         * that is being watched, they are then assigned the role being managed
         *
         */
        private static async Task Reaction_Added(MessageReactionAddEventArgs e)
        {
            if (!Config.RolesToWatch.Select(r => r.GetEmoji()).ToList().Contains(e.Emoji)) return;
            
            var roleExists = Config.RolesToWatch.Select(r => r.GetMessage()).ToList().Contains(e.Message);
            
            // filters out spare emotes
            if (roleExists)
            {
                // get the role to assign
                // select role where emoji is the emoji added
                var roleToAssign = GetRoleWatches(e).ToList();
                
                // get members who've reacted from the users who've reacted
                var membersReacted = from discordUser in e.Message.GetReactionsAsync(e.Emoji).Result
                    select roleToAssign.First().GetGuild().GetMemberAsync(discordUser.Id).Result;
                
                // retroactively assigns roles
                foreach (var member in membersReacted)
                {
                    // optimisation to reduce iterations if member already has the role
                    if(member.Roles.Contains(roleToAssign.First().GetRole())) continue;
                    
                    await member.GrantRoleAsync(roleToAssign.First().GetRole());
                    await LogPrinter.Role_Assigned(e, member, roleToAssign.First().GetRole());
                }
            }
        }
        
        /* Reaction_Removed Event Handler
         * Checks if the emoji and the message is being watched
         *
         * Then gets roles that need to be revoked based on emoji.
         *
         * If admin has enabled autoremoval of members based on reactions, all members who haven't reacted with a
         * particular emote AND have the specified role, have their role revoked
         *
         * If admin has disabled autoremoval of members based on reactions, each member is revoked on a per event base
         */
        private static async Task Reaction_Removed(MessageReactionRemoveEventArgs e)
        {
            // increase efficiency by ensuring that emotes that aren't being watched don't trigger unnecessary exceptions
            if (!Config.RolesToWatch.Select(r => r.GetEmoji()).ToList().Contains(e.Emoji)) return;
            
            var roleExists = Config.RolesToWatch.Select(r => r.GetMessage()).ToList().Contains(e.Message);
            
            if (roleExists)
            {
                // selects the role that is supposed to be revoked
                // select role where emoji is emoji removed
                var roleToRevoke = GetRoleWatches(e).ToList();
                
                if (!Config.AutoRemoveFlag)
                {
                    // retro actively tries to remove roles (created in case bot goes offline)
                    var guildMembers = roleToRevoke.First().GetGuild().GetAllMembersAsync().Result;
                    
                    // gets all the members reacted then gets members from the guild
                    var membersReacted = from discordUser in e.Message.GetReactionsAsync(e.Emoji).Result
                        select roleToRevoke.First().GetGuild().GetMemberAsync(discordUser.Id).Result;

                    // filters members to remove
                    // select member from guild members where reacted members doesn't contain the member
                    var membersToRemove = from discordMember in guildMembers
                        where !membersReacted.Contains(discordMember)
                        select discordMember;

                    // retroactively removes roles
                    foreach (var member in membersToRemove)
                    {
                        // optimisation to reduce iterations if member doesn't have the role
                        if (!member.Roles.Contains(roleToRevoke.First().GetRole())) continue;

                        await member.RevokeRoleAsync(roleToRevoke.First().GetRole());
                        await LogPrinter.Role_Revoked(e, member, roleToRevoke.First().GetRole());
                    }   
                }
                // If autoremoval of members if off
                else
                {
                    var member = roleToRevoke.First().GetGuild().GetMemberAsync(e.User.Id).Result;
                    await member.RevokeRoleAsync(roleToRevoke.First().GetRole());
                    await LogPrinter.Role_Revoked(e, member, roleToRevoke.First().GetRole());
                }
            }
        }

        private static IEnumerable<RoleWatch> GetRoleWatches (MessageReactionAddEventArgs addE)
        {
            return from roles in Config.RolesToWatch
                   where roles.GetEmoji().Equals(addE.Emoji)
                   select roles;
        }

        private static IEnumerable<RoleWatch> GetRoleWatches (MessageReactionRemoveEventArgs removeE)
        {
            return from roles in Config.RolesToWatch
                   where roles.GetEmoji().Equals(removeE.Emoji)
                   select roles;
        }

        /* Refresh Config Method
        * Used to load the configuration into the assembly
        */
        internal async static Task RefreshConfig ()
        {
            using (var reader = new StreamReader(ConfigPath))
                Config = JsonConvert.DeserializeObject<Config>((await reader.ReadToEndAsync()).Trim());
        }

        /*
         * Updates the config file for permanent storage of settings and roles to watch
         */
        internal async static Task UpdateConfigFile()
        {
            using (var writer = new StreamWriter(ConfigPath))
                await writer.WriteAsync(JsonConvert.SerializeObject(Config, Formatting.Indented));
        }
    }
}