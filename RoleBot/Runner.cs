using System.IO;

namespace RoleBot
{
    internal class Runner
    {
        internal static void Main()
        {
            Bot.RunBotAsync().GetAwaiter().GetResult();
            
            FileSystemWatcher configWatcher = new FileSystemWatcher("config.xml", ".xml");
            
            configWatcher.Changed += OnChanged;
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Bot.RefreshConfig();
        }
    }
}