namespace RoleBot
{
    internal class Runner
    {
        internal static void Main()
        {
            Bot.RunBotAsync().GetAwaiter().GetResult();
        }
    }
}