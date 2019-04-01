// @author Shardul Vaidya

using System;
using System.IO;
using System.Threading.Tasks;

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

                Console.WriteLine("Do you want to auto-remove users who have roles and have not reacted (No is default, not recommended for large servers)?");
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

                Console.WriteLine("What do you want the command prefix to be without any spaces.");
                tempConfig.CommandPrefix = Console.ReadLine().Trim();

                using (var configwriter = new StreamWriter("config.json"))
                    await configwriter.WriteAsync(JsonConvert.SerializeObject(tempConfig));
            }
            var bot = Bot.RunBotAsync();
            var unused = await bot;
        }
    }
}