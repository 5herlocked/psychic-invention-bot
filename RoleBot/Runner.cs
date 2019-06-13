// @author Shardul Vaidya

using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using Newtonsoft.Json;

namespace RoleBot
{
    internal static class Runner
    {
        internal static async Task Main()
        {
            if(!File.Exists("config.json"))
            {
                var tempConfig = new Config();
                Console.WriteLine("There is no config file found, this setup will create one for the bot to function: ");
                Console.WriteLine("Please enter the Discord API Token from the Developer Portal: ");
                tempConfig.Token = Console.ReadLine().Trim();

                var clientConfig = new DiscordConfiguration
                {
                    Token = tempConfig.Token,
                    TokenType = TokenType.Bot,

                    LogLevel = LogLevel.Debug,
                    UseInternalLogHandler = true
                };
                try
                {
                    Bot.Client = new DiscordClient(clientConfig);
                } catch (Exception)
                {
                    Console.WriteLine("It seems your API token is invalid, terminating setup");
                    Environment.Exit(0);
                }

                Console.WriteLine("Do you want to auto-remove users who have roles and have not reacted (not recommended for severs with previously assigned roles)?");
                Console.WriteLine("Answer in (Y/N)");
                switch (Console.ReadLine().Trim().ToLower())
                {
                    case "y":
                        tempConfig.AutoRemoveFlag = true;
                        break;
                    case "n":
                        tempConfig.AutoRemoveFlag = false;
                        break;
                    default:
                        Console.WriteLine("Please answer in (Y/N)");
                        break;
                }

                do
                {
                    Console.WriteLine("What do you want the command prefix to be without any spaces.");
                    var tempPrefix = Console.ReadLine().Trim();
                    if (tempPrefix.Contains(" "))
                    {
                        Console.WriteLine("This Prefix contains spaces, please enter a prefix with no spaces");
                        continue;
                    }
                    else
                        tempConfig.CommandPrefix = Console.ReadLine().Trim();
                } while (tempConfig.CommandPrefix == null);

                Console.WriteLine("What do you want the command prefix to be without any spaces.");
                

                using (var configwriter = new StreamWriter("config.json"))
                    await configwriter.WriteAsync(JsonConvert.SerializeObject(tempConfig));
            }
            var bot = Bot.RunBotAsync();
            _ = await bot;
        }
    }
}