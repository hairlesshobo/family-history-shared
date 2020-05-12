using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Archiver.Utilities
{
    public static class Config
    {
        public static void ReadConfig()
        {
            IConfiguration _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./config/appsettings.json", true, true)
                .Build();

            Globals._discCapacityLimit = _config.GetSection("Disc:CapacityLimit").Get<long>();
            Globals._discStagingDir = _config["Disc:StagingDir"];
            Globals._cdbxpPath = _config["CdbxpPath"];
            Globals._ddPath = _config["DdPath"];
            Globals._tapeDrive = _config["Tape:Drive"];
            Globals._tapeBlockingFactor = _config.GetSection("Tape:BlockingFactor").Get<int>();
            Globals._discExcludeFiles = _config.GetSection("Disc:ExcludeFiles").Get<string[]>().ToList();
            Globals._discSourcePaths = _config.GetSection("Disc:SourcePaths").Get<string[]>();
            Array.Sort(Globals._discSourcePaths);

            foreach (string excPath in _config.GetSection("Disc:ExcludePaths").Get<string[]>())
            {
                string cleanExcPath = Helpers.CleanPath(excPath);

                if (File.Exists(cleanExcPath) || Directory.Exists(cleanExcPath))
                    Globals._discExcludePaths.Add(cleanExcPath);
            }

            Globals._indexDiscDir = Globals._discStagingDir + $"/index";
        }
    }
}