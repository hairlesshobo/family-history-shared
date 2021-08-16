using System.IO;
using Archiver.Shared.Models.Config;
using Microsoft.Extensions.Configuration;

namespace Archiver.Shared.Utilities
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

        public static IConfiguration ReadConfigFile()
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

        public static ArchiverConfig ReadConfig()
        {
            IConfiguration _config = Utils.ReadConfigFile();

            ArchiverConfig config = new ArchiverConfig();
            _config.Bind(config);
            
            return config;
        }
    }
}