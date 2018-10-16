// Author: Shardul Vaidya
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;

namespace RoleBot
{
    internal static class Runner
    {
        internal static async Task Main()
        {
            var bot = Bot.RunBotAsync();
            
            //Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "Rolebot", $"Log file : {Bot.Path}", DateTime.Now); // Declaring where the log file is
            
            // To allow for continuous watching of the Config file in an async environment
            var unused = await bot;
        }
    }
}