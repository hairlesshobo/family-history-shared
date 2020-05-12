using System.Collections.Generic;
using Archiver.Classes;

namespace Archiver
{
    public static class Globals
    {
        public static List<DiscSourceFile> _discSourceFiles = new List<DiscSourceFile>();
        public static List<DiscDetail> _destinationDiscs = new List<DiscDetail>();


        public static long _scannedNewlyFoundFiles = 0;
        public static long _scannedExistingFilesArchived = 0;
        public static long _scannedTotalSize = 0;
        public static long _scannedExcludedFileCount = 0;

        public static int _leftHeaderWidth = 11;
        public static bool _readOnlyFs = false;



        public static string[] _discSourcePaths;
        public static List<string> _discExcludePaths = new List<string>();
        public static List<string> _discExcludeFiles = new List<string>();
        public static long _discCapacityLimit = 0;
        public static string _discStagingDir;
        public static string  _cdbxpPath;
        public static string _ddPath;
        public static string _tapeDrive;
        public static int _tapeBlockingFactor;
        public static string _indexDiscDir;

    }
}