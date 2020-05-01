using System;
using System.Collections.Generic;
using System.Linq;
using DiscArchiver.Archiver.Classes;

namespace DiscArchiver.Archiver
{
    public static class Globals
    {
        public static List<SourceFile> _sourceFiles = new List<SourceFile>();
        public static List<DestinationDisc> _destinationDiscs = new List<DestinationDisc>();


        public static long _newlyFoundFiles = 0;
        public static long _existingFilesArchived = 0;
        public static long _totalSize = 0;
        public static string _indexDiscDir;
        public static long _excludedFileCount = 0;



        public static string[] _sourcePaths;
        public static List<string> _excludePaths = new List<string>();
        public static List<string> _excludeFiles = new List<string>();
        public static long _discCapacityLimit = 0;
        public static string _stagingDir;
        public static string  _cdbxpPath;

    }
}