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
            var unused = await bot;
        }
    }
}