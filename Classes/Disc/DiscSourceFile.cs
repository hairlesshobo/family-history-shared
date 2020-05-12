using System;
using System.IO;
using System.Linq;
using Archiver.Utilities;
using Newtonsoft.Json;

namespace Archiver.Classes
{
public class DiscSourceFile
    {
        private string _FullPath;
        public string Name { get; set; }
        public string Extension { 
            get
            {
                if (this.Name.Contains(".") && this.Name.LastIndexOf('.') != this.Name.Length-1)
                    return this.Name.Substring(this.Name.LastIndexOf('.')+1).ToLower();
                else
                    return String.Empty;
            }
        }
        public string FullPath { 
            get
            {
                return _FullPath;
            } 
            set
            {
                _FullPath = Helpers.CleanPath(value);

                this.Name = Helpers.GetFileName(_FullPath);
                this.RelativePath = Helpers.GetRelativePath(value);
                this.RelativeDirectory = this.RelativePath.Substring(0, this.RelativePath.LastIndexOf('/'));
            }
        }
        public string RelativePath { get; set; }
        public string RelativeDirectory { get; set; }
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

        public DiscSourceFile()
        {
            Globals._discSourceFiles.Add(this);
        }

        public DiscSourceFile(string sourcePath)
        {
            this.FullPath = sourcePath;

            if (!File.Exists(this.FullPath))
                throw new DirectoryNotFoundException($"Source file does not exist: {sourcePath}");

            // we found a file on the filesystem, but it is already in the archive
            if (Globals._discSourceFiles.Any(x => x.Archived == true && x.RelativePath == this.RelativePath))
            {
                Globals._scannedExistingFilesArchived += 1;
            }

            // we only add the file to the index if it hasn't yet been archived
            else
            {
                Globals._scannedNewlyFoundFiles += 1;
                Globals._discSourceFiles.Add(this);
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

            Globals._scannedTotalSize += this.Size;
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
            string destinationDir = $"{this.DestinationDisc.RootStagingPath}/data" + this.RelativeDirectory;

            CustomFileCopier copier = new CustomFileCopier(this.FullPath, destinationDir);
            copier.OverwriteDestination = true;
            copier.Preserve = true;

            return copier;
        }
    }
}
