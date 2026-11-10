using System;
using System.IO;

namespace DataBaseGenerator.UI.Wpf.Service
{
    internal static class UserDataPaths
    {
        private const string AppFolderName = "DataBaseGeneratorSettings";

        public static string SettingsDirectory
        {
            get
            {
                var path = Path.Combine(AppContext.BaseDirectory, AppFolderName);
                Directory.CreateDirectory(path);
                return path;
            }
        }
    }
}
