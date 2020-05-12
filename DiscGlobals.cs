using System.Collections.Generic;
using Archiver.Classes;
using Archiver.Classes.Disc;

namespace Archiver
{
    public static class DiscGlobals
    {
        public static List<DiscSourceFile> _discSourceFiles = new List<DiscSourceFile>();
        public static List<DiscDetail> _destinationDiscs = new List<DiscDetail>();


        public static long _newlyFoundFiles = 0;
        public static long _existingFilesArchived = 0;
        public static long _totalSize = 0;
        public static long _excludedFileCount = 0;


        public static string[] _discSourcePaths;
        public static List<string> _discExcludePaths = new List<string>();
        public static List<string> _discExcludeFiles = new List<string>();
        public static long _discCapacityLimit = 0;
        public static string _discStagingDir;

    }
}