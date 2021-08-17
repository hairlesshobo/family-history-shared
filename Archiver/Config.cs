using System;
using System.IO;
using System.Linq;
using Archiver.Shared.Utilities;
using Microsoft.Extensions.Configuration;

namespace Archiver
{
    public static class Config
    {
        public static void ReadConfig()
        {
            // we set the directory to where the exe is
            Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
            
            string configDir = Path.Join(Directory.GetCurrentDirectory(), "config/");

            // lets check the current dir and the parent dir for a config directory

            if (!Directory.Exists(configDir))
            {
                configDir = Path.Join(Directory.GetCurrentDirectory(), "../", "config/");

                if (Directory.Exists(configDir))
                    Directory.SetCurrentDirectory(Path.Join(Directory.GetCurrentDirectory(), "../"));
            }

            IConfiguration _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./config/appsettings.json", true, true)
                .Build();

            // disc config
            DiscGlobals._discCapacityLimit = _config.GetSection("Disc:CapacityLimit").Get<long>();
            DiscGlobals._discStagingDir = _config["Disc:StagingDir"];
            DiscGlobals._discExcludeFiles = _config.GetSection("Disc:ExcludeFiles").Get<string[]>().ToList();
            DiscGlobals._discSourcePaths = _config.GetSection("Disc:SourcePaths").Get<string[]>();
            Array.Sort(DiscGlobals._discSourcePaths);

            foreach (string excPath in _config.GetSection("Disc:ExcludePaths").Get<string[]>())
            {
                string cleanExcPath = PathUtils.CleanPath(excPath);

                if (File.Exists(cleanExcPath) || Directory.Exists(cleanExcPath))
                    DiscGlobals._discExcludePaths.Add(cleanExcPath);
            }
        }

        
    }
}