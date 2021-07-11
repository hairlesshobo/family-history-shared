using System.Collections.Generic;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;

namespace Archiver
{
    public static class CsdGlobals
    {
        public static List<CsdSourceFile> _newFileEntries => _sourceFiles.Where(x => x.Copied == false).ToList();
        
        public static List<CsdSourceFile> _sourceFiles = new List<CsdSourceFile>();
        public static List<CsdSourceFile> _deletedFiles = new List<CsdSourceFile>();
        public static List<CsdDetail> _destinationCsds = new List<CsdDetail>();


        public static long _newFileCount = 0;
        public static long _deletedFileCount = 0;
        public static long _renamedFileCount = 0;
        public static long _existingFileCount = 0;
        public static long _excludedFileCount = 0;
        public static long _totalSize = 0;


        public static string[] _csdSourcePaths;
        public static List<string> _csdExcludePaths = new List<string>();
        public static List<string> _csdExcludeFiles = new List<string>();

        public static void Reset()
        {
            _sourceFiles.Clear();
            _deletedFiles.Clear();
            _destinationCsds.Clear();
            _newFileCount = 0;
            _renamedFileCount = 0;
            _existingFileCount = 0;
            _totalSize = 0;
            _excludedFileCount = 0;
        }

    }
}