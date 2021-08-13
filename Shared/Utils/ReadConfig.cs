using System.IO;
using Microsoft.Extensions.Configuration;

namespace Archiver.Shared
{
    public static partial class Utils
    {
        public static string GetConfigDirectory()
        {
            // we set the directory to where the exe is
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            
            string configDir = Path.Join(dir, "config/");

            // lets check the current dir and the parent dir for a config directory

            if (Directory.Exists(configDir))
                dir = configDir;
            else
            {
                configDir = Path.Join(dir, "../", "config/");

                if (Directory.Exists(configDir))
                    dir = configDir;
            }

            return dir;
        }

        public static IConfiguration ReadConfig()
        {
            string configDir = GetConfigDirectory();

            // we set the directory to where the exe is
            Directory.SetCurrentDirectory(configDir);
            
            IConfiguration _config = new ConfigurationBuilder()
                .SetBasePath(configDir)
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return _config;
        }
    }
}