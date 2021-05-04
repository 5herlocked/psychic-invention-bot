// @author Shardul Vaidya

using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace RoleBot
{
    internal static class Runner
    {
        internal static async Task Main()
        {
            var tempConfig = new Config();
            var discordToken = Environment.GetEnvironmentVariable("DiscordToken");

            if (discordToken == null)
            {
                // No token found throw error
                Environment.Exit(0);
            }

            tempConfig.Token = discordToken;

            var clientConfig = new DiscordConfiguration
            {
                Token = tempConfig.Token,
                TokenType = TokenType.Bot,

                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            // Terminates bot if the API Token is wrong
            try
            {
                Bot.Client = new DiscordClient(clientConfig);
            }
            catch (Exception)
            {
                Console.WriteLine("It seems your API token is invalid, terminating setup");
                Environment.Exit(0);
            }

            tempConfig.AutoRemoveFlag = false;

            var commandPrefix = Environment.GetEnvironmentVariable("CommandPrefix");
            if (commandPrefix == null)
            {
                // Default value
                commandPrefix = "r!";
            }
            tempConfig.CommandPrefix = commandPrefix;

            Bot.Config = tempConfig;

            var bot = Bot.RunBotAsync();
            _ = await bot;
        }
    }
}