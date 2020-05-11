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

            Globals._discCapacityLimit = _config.GetSection("DiscCapacityLimit").Get<long>();
            Globals._discStagingDir = _config["DiscStagingDir"];
            Globals._cdbxpPath = _config["CdbxpPath"];
            Globals._ddPath = _config["DdPath"];
            Globals._tapeDrive = _config["Tape:Drive"];
            Globals._tapeBlockingFactor = _config.GetSection("Tape:BlockingFactor").Get<int>();
            Globals._excludeFiles = _config.GetSection("ExcludeFiles").Get<string[]>().ToList();
            Globals._sourcePaths = _config.GetSection("SourcePaths").Get<string[]>();
            Array.Sort(Globals._sourcePaths);

            foreach (string excPath in _config.GetSection("ExcludePaths").Get<string[]>())
            {
                string cleanExcPath = Helpers.CleanPath(excPath);

                if (File.Exists(cleanExcPath) || Directory.Exists(cleanExcPath))
                    Globals._excludePaths.Add(cleanExcPath);
            }

            Globals._indexDiscDir = Globals._discStagingDir + $"/index";
        }
    }
}