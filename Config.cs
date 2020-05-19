using System;
using System.IO;
using System.Linq;
using Archiver.Utilities.Shared;
using Archiver.Utilities.Tape;
using Microsoft.Extensions.Configuration;

namespace Archiver
{
    public static class Config
    {
        public static string TapeDrive { get; set; }
        public static int TapeBlockingFactor { get; set; }
        public static int TapeMemoryBufferBlockCount { get; set; }
        public static int TapeMemoryBufferMinFill { get; set; }
        public static int TapeTextBlockSize { get; set; }
        public static bool TapeAutoEject { get; set; }
        public static bool TapeDrivePresent 
        { 
            get
            {
                return _tapeDrivePresent;
            }
        }

        private static bool _tapeDrivePresent;

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
            Archiver.Config.TapeMemoryBufferBlockCount = _config.GetSection("Tape:MemoryBufferBlockCount").Get<int>();
            Archiver.Config.TapeMemoryBufferMinFill = _config.GetSection("Tape:MemoryBufferMinFill").Get<int>();
            Archiver.Config.TapeTextBlockSize = _config.GetSection("Tape:TextBlockSize").Get<int>();
            Archiver.Config.TapeAutoEject = _config.GetSection("Tape:AutoEject").Get<bool>();
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

            _tapeDrivePresent = TapeUtils.TapeDrivePresent();
        }
    }
}