using System;
using System.IO;
using System.Reflection;
using IWshRuntimeLibrary;
using File = System.IO.File;


namespace DrawAnywhere.Sys
{
    class WindowsShell
    {
        private static string _startupShortcutPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "DrawAnywhere.lnk");

        public static void CreateShortcut(string targetFile, string shortcutLocation)
        {
            var shell = new WshShellClass();

            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "A link for Draw Anywhere! software";
            shortcut.TargetPath = targetFile;

            shortcut.Save();
        }

        public static void AddStartup()
        {
            if (!File.Exists(_startupShortcutPath))
            {
                var me = Path.Combine(AppContext.BaseDirectory, AppDomain.CurrentDomain.FriendlyName + ".exe"); // Literally me
                CreateShortcut(me, _startupShortcutPath);
            }
        }

        public static void RemoveStartup()
        {
            if (File.Exists(_startupShortcutPath))
            {
                File.Delete(_startupShortcutPath);
            }
        }
    }
}
