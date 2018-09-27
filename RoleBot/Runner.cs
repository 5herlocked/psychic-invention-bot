using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;

namespace RoleBot
{
    internal class Runner
    {
        private static FileSystemWatcher _configWatcher;
            
        internal static async Task Main()
        {
            var bot = Bot.RunBotAsync();
            
            var commandLineArgs = Environment.GetCommandLineArgs();

            _configWatcher = new FileSystemWatcher(commandLineArgs[1], "config.xml")
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                               NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            }; // Watches file based on Command Line Args

            _configWatcher.Changed += OnChanged;

            _configWatcher.EnableRaisingEvents = true;
            
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "Rolebot", $"Watching file: {_configWatcher.Path}", DateTime.Now);

            var getBot = await bot;
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Bot.RefreshConfig();
            
            Bot.Client.DebugLogger.LogMessage(LogLevel.Debug, "RoleBot", "Config file Changed", DateTime.Now);
        }
    }
}