using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes;

namespace Archiver
{
    public static class Globals
    {
        public static List<SourceFile> _sourceFiles = new List<SourceFile>();
        public static List<DiscDetail> _destinationDiscs = new List<DiscDetail>();


        public static long _newlyFoundFiles = 0;
        public static long _existingFilesArchived = 0;
        public static long _totalSize = 0;
        public static string _indexDiscDir;
        public static long _excludedFileCount = 0;
        public static int _leftHeaderWidth = 11;
        public static bool _readOnlyFs = false;



        public static string[] _sourcePaths;
        public static List<string> _excludePaths = new List<string>();
        public static List<string> _excludeFiles = new List<string>();
        public static long _discCapacityLimit = 0;
        public static string _discStagingDir;
        public static string  _cdbxpPath;
        public static string _ddPath;
        public static string _tapeDrive;
        public static int _tapeBlockingFactor;

    }
}