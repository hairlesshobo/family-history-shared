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

using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes;
using Archiver.Classes.Disc;

namespace Archiver
{
    public static class DiscGlobals
    {
        public static List<DiscSourceFile> _newFileEntries => _discSourceFiles.Where(x => x.Copied == false).ToList();
        public static List<DiscSourceFile> _discSourceFiles = new List<DiscSourceFile>();
        public static List<DiscDetail> _destinationDiscs = new List<DiscDetail>();


        public static long _newlyFoundFiles = 0;
        public static long _renamedFiles = 0;
        public static long _existingFilesArchived = 0;
        public static long _totalSize = 0;
        public static long _excludedFileCount = 0;

        public static void Reset()
        {
            _discSourceFiles.Clear();
            _destinationDiscs.Clear();
            _newlyFoundFiles = 0;
            _renamedFiles = 0;
            _existingFilesArchived = 0;
            _totalSize = 0;
            _excludedFileCount = 0;
        }

    }
}