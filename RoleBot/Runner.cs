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
        private static FileSystemWatcher _configWatcher;
            
        internal static async Task Main()
        {
            var bot = Bot.RunBotAsync();
            
            // Config needs to be placed in the same file as the assembly
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            _configWatcher = new FileSystemWatcher(assemblyLocation, "config.xml")
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                               NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            }; // Watches file based on Command Line Args

            _configWatcher.Changed += OnChanged;

            _configWatcher.EnableRaisingEvents = true;
            
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "Rolebot", $"Watching folder: {_configWatcher.Path}, Watching file: {_configWatcher.Filter}", DateTime.Now); // Telling user what file is being watched as config
            Bot.Client.DebugLogger.LogMessage(LogLevel.Info, "Rolebot", $"Log file : {Bot.Path}", DateTime.Now); // Declaring where the log file is
            
            // To allow for continuous watching the Config file in an async environment
            var unused = await bot;
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Bot.RefreshConfig();
            
            Bot.Client.DebugLogger.LogMessage(LogLevel.Debug, "RoleBot", "Config file Changed", DateTime.Now);
        }
    }
}