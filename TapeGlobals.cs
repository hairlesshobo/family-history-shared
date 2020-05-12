using System.Collections.Generic;
using Archiver.Classes.Tape;

namespace Archiver
{
    public static class TapeGlobals
    {
        public static List<TapeSourceFile> _sourceFiles = new List<TapeSourceFile>();
        public static List<TapeDetail> _tapes = new List<TapeDetail>();


        public static long _foundFiles = 0;
        public static ulong _totalSize = 0;
        public static ulong _totalArchiveSize = 0;
        public static long _excludedFileCount = 0;


        public static string _tapeDrive;
        public static int _tapeBlockingFactor;

    }
}