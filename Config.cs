using System;
using System.IO;
using System.Linq;
using Archiver.Utilities.Shared;
using Microsoft.Extensions.Configuration;

namespace Archiver
{
    public static class Config
    {
        public static string TapeDrive { get; set; }
        public static int TapeBlockingFactor { get; set; }

        public static void ReadConfig()
        {
            IConfiguration _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./config/appsettings.json", true, true)
                .Build();

            DiscGlobals._discCapacityLimit = _config.GetSection("Disc:CapacityLimit").Get<long>();
            DiscGlobals._discStagingDir = _config["Disc:StagingDir"];
            Globals._cdbxpPath = _config["CdbxpPath"];
            Globals._ddPath = _config["DdPath"];
            Archiver.Config.TapeDrive = _config["Tape:Drive"];
            Archiver.Config.TapeBlockingFactor = _config.GetSection("Tape:BlockingFactor").Get<int>();
            DiscGlobals._discExcludeFiles = _config.GetSection("Disc:ExcludeFiles").Get<string[]>().ToList();
            DiscGlobals._discSourcePaths = _config.GetSection("Disc:SourcePaths").Get<string[]>();
            Array.Sort(DiscGlobals._discSourcePaths);

            foreach (string excPath in _config.GetSection("Disc:ExcludePaths").Get<string[]>())
            {
                string cleanExcPath = Helpers.CleanPath(excPath);

                if (File.Exists(cleanExcPath) || Directory.Exists(cleanExcPath))
                    DiscGlobals._discExcludePaths.Add(cleanExcPath);
            }

            Globals._indexDiscDir = DiscGlobals._discStagingDir + $"/index";
        }
    }
}