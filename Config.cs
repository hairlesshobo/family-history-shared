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
        public static bool OpticalDrivePresent
        {
            get
            {
                return _opticalDrivePresent;
            }
        }

        public static bool ReadOnlyFilesystem
        {
            get
            {
                return _readOnlyFilesystem;
            }
        }

        private static bool _tapeDrivePresent;
        private static bool _opticalDrivePresent;
        private static bool _readOnlyFilesystem;

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

            Globals._indexDiscDir = Directory.GetCurrentDirectory();

            _tapeDrivePresent = TapeUtils.TapeDrivePresent();
            _opticalDrivePresent = DriveInfo.GetDrives().Any(x => x.DriveType == DriveType.CDRom);
            _readOnlyFilesystem = TestForReadonlyFs();
        }

        private static bool TestForReadonlyFs()
        {
            string currentdir = Directory.GetCurrentDirectory();
            string testFile = Path.Join(currentdir, "__accesstest.tmp");
            bool canWrite = true;

            if (File.Exists(testFile))
            {
                try
                {
                    File.Delete(testFile);
                }
                catch
                {
                    canWrite = false;
                }
            }

            try
            {
                using (FileStream stream = File.Create(testFile))
                { }

                File.Delete(testFile);
            }
            catch
            {
                canWrite = false;
            }

            return !canWrite;
        }
    }
}