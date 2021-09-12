/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Archiver.Classes;
// using Archiver.Classes.CSD;

// namespace Archiver
// {
//     public static class CsdGlobals
//     {
//         public static List<CsdSourceFile> _jsonReadSourceFiles = new List<CsdSourceFile>();
//         public static List<CsdSourceFile> _newFileEntries => _sourceFileDict.Select(x => x.Value).Where(x => x.Copied == false).ToList();
        
//         public static List<CsdSourceFile> _deletedFiles = new List<CsdSourceFile>();
//         public static List<CsdDetail> _destinationCsds = new List<CsdDetail>();
//         public static Dictionary<string, CsdSourceFile> _sourceFileDict = new Dictionary<string, CsdSourceFile>(StringComparer.OrdinalIgnoreCase);


//         public static long _newFileCount = 0;
//         public static long _deletedFileCount = 0;
//         public static long _modifiedFileCount = 0;
//         public static long _renamedFileCount = 0;
//         public static long _existingFileCount = 0;
//         public static long _excludedFileCount = 0;
//         public static long _totalSizePending = 0;

//         public static void Reset()
//         {
//             _sourceFileDict.Clear();
//             _jsonReadSourceFiles.Clear();
//             _deletedFiles.Clear();
//             _destinationCsds.Clear();
//             _newFileCount = 0;
//             _renamedFileCount = 0;
//             _existingFileCount = 0;
//             _totalSizePending = 0;
//             _excludedFileCount = 0;
//         }

//     }
// }