using System;
using System.IO;
using System.Linq;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using Archiver.Utilities.Tape;
using Microsoft.Extensions.Configuration;

namespace Archiver
{
    public static class Config
    {
        public static string TapeDriver { get; set; }
        public static string TapeDrive { get; set; }
        public static int TapeBlockingFactor { get; set; }
        public static int TapeMemoryBufferBlockCount { get; set; }
        public static int TapeMemoryBufferMinFill { get; set; }
        public static int TapeTextBlockSize { get; set; }
        public static bool TapeAutoEject { get; set; }

        public static long CsdReservedCapacity { get; set; }
        public static int CsdAutoSaveInterval { get; set; }


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

            // global config
            Globals._cdbxpPath = _config["CdbxpPath"];
            Globals._ddPath = _config["DdPath"];
            Globals._indexDiscDir = Directory.GetCurrentDirectory();

            // Tape config
            Archiver.Config.TapeDriver = _config["Tape:Driver"];
            Archiver.Config.TapeDrive = _config["Tape:Drive"];
            Archiver.Config.TapeBlockingFactor = _config.GetSection("Tape:BlockingFactor").Get<int>();
            Archiver.Config.TapeMemoryBufferBlockCount = _config.GetSection("Tape:MemoryBufferBlockCount").Get<int>();
            Archiver.Config.TapeMemoryBufferMinFill = _config.GetSection("Tape:MemoryBufferMinFill").Get<int>();
            Archiver.Config.TapeTextBlockSize = _config.GetSection("Tape:TextBlockSize").Get<int>();
            Archiver.Config.TapeAutoEject = _config.GetSection("Tape:AutoEject").Get<bool>();
            
            // CSD Config
            Archiver.Config.CsdReservedCapacity = _config.GetSection("CSD:ReservedCapacityBytes").Get<long>();
            Archiver.Config.CsdAutoSaveInterval = _config.GetSection("CSD:AutoSaveInterval").Get<int>();

            CsdGlobals._csdExcludeFiles = _config.GetSection("CSD:ExcludeFiles").Get<string[]>().ToList();
            CsdGlobals._csdSourcePaths = _config.GetSection("CSD:SourcePaths").Get<string[]>();
            Array.Sort(CsdGlobals._csdSourcePaths);

            foreach (string excPath in _config.GetSection("CSD:ExcludePaths").Get<string[]>())
            {
                string cleanExcPath = PathUtils.CleanPath(excPath);

                if (File.Exists(cleanExcPath) || Directory.Exists(cleanExcPath))
                    CsdGlobals._csdExcludePaths.Add(cleanExcPath);
            }

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

            _tapeDrivePresent = TapeUtils.TapeDrivePresent();
        }

        
    }
}