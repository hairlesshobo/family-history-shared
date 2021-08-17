using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.CSD;

namespace Archiver
{
    public static class CsdGlobals
    {
        public static List<CsdSourceFile> _jsonReadSourceFiles = new List<CsdSourceFile>();
        public static List<CsdSourceFile> _newFileEntries => _sourceFileDict.Select(x => x.Value).Where(x => x.Copied == false).ToList();
        
        public static List<CsdSourceFile> _deletedFiles = new List<CsdSourceFile>();
        public static List<CsdDetail> _destinationCsds = new List<CsdDetail>();
        public static Dictionary<string, CsdSourceFile> _sourceFileDict = new Dictionary<string, CsdSourceFile>(StringComparer.OrdinalIgnoreCase);


        public static long _newFileCount = 0;
        public static long _deletedFileCount = 0;
        public static long _modifiedFileCount = 0;
        public static long _renamedFileCount = 0;
        public static long _existingFileCount = 0;
        public static long _excludedFileCount = 0;
        public static long _totalSizePending = 0;


        public static string[] _csdSourcePaths;
        public static List<string> _csdExcludePaths = new List<string>();
        public static List<string> _csdExcludeFiles = new List<string>();

        public static void Reset()
        {
            _sourceFileDict.Clear();
            _jsonReadSourceFiles.Clear();
            _deletedFiles.Clear();
            _destinationCsds.Clear();
            _newFileCount = 0;
            _renamedFileCount = 0;
            _existingFileCount = 0;
            _totalSizePending = 0;
            _excludedFileCount = 0;
        }

    }
}