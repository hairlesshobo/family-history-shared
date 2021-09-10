﻿/**
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
using System.IO;
using System.Linq;
using Archiver.Shared.Interfaces;
using Archiver.Utilities.Shared;
using Newtonsoft.Json;

namespace Archiver.Classes.Disc
{
    public class DiscSourceFile : DiscSourceFilePathDetail, ISourceFile
    {
        public DiscSourceFilePathDetail OriginalFile { get; set; } = null;
        public long Size { get; set; } = -1;
        public bool Archived { get; set; }
        public string Hash { get; set; }
        public bool Copied { get; set; }
        public DateTime ArchiveTimeUtc { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public FileAttributes Attributes { get; set; }
        [JsonIgnore]
        public DiscDetail DestinationDisc { get; set; } = null;
        [JsonIgnore]
        public string SourceRootPath
        {
            get
            {
                return this.FullPath.Substring(0, this.FullPath.Length - this.Name.Length - this.RelativeDirectory.Length - 1);
            }
        }

        public DiscSourceFile()
        {
            DiscGlobals._discSourceFiles.Add(this);
        }

        public DiscSourceFile(string sourcePath, bool scanForRenames = false)
        {
            this.FullPath = sourcePath;

            if (!File.Exists(this.FullPath))
                throw new DirectoryNotFoundException($"Source file does not exist: {sourcePath}");

            // we found a file on the filesystem, but it is already in the archive
            if (DiscGlobals._discSourceFiles.Any(x => x.Archived == true && x.RelativePath == this.RelativePath))
            {
                DiscGlobals._existingFilesArchived += 1;
            }

            // we only add the file to the index if it hasn't yet been archived
            else
            {
                bool isNewFile = true;
                
                // scan for renames, only if we are told to do so
                if (scanForRenames == true)
                {
                    // we scan for renames by checking for files with the same extension and size
                    this.ReadSizeAndAttribs();
                }

                if (isNewFile)
                {
                    DiscGlobals._newlyFoundFiles += 1;
                    DiscGlobals._discSourceFiles.Add(this);
                }
            }
        }

        public void ReadSizeAndAttribs()
        {
            FileInfo fileInfo = new FileInfo(this.FullPath);

            this.Size = fileInfo.Length;
            this.LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            this.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            this.CreationTimeUtc = fileInfo.CreationTimeUtc;
            this.Attributes = fileInfo.Attributes;

            DiscGlobals._totalSize += this.Size;
        }

        public void AssignDisc()
        {
            if (this.Size >= 0)
            {
                this.DestinationDisc = Helpers.GetDestinationDisc(this.Size);
                this.DestinationDisc.Files.Add(this);
            }
        }

        public CustomFileCopier ActivateCopy()
        {
            string destinationDir = $"{this.DestinationDisc.RootStagingPath}/data";

            CustomFileCopier copier = new CustomFileCopier(this, destinationDir);
            copier.OverwriteDestination = true;
            copier.Preserve = true;

            return copier;
        }
    }
}
