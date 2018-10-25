// Author: Shardul Vaidya

using System.Threading.Tasks;

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