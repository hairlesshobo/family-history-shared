using System;
using System.IO;
using System.Linq;
using Archiver.Utilities.Shared;
using Newtonsoft.Json;

namespace Archiver.Classes.Tape
{
public class TapeSourceFile
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
        public long Size { get; set; } = 0;
        public string Hash { get; set; }
        public bool Copied { get; set; }
        public DateTime ArchiveTimeUtc { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public FileAttributes Attributes { get; set; }
        [JsonIgnore]
        public TapeDetail Tape { get; set; } = null;

        public TapeSourceFile()
        {
        }

        public TapeSourceFile(string sourcePath)
        {
            this.FullPath = sourcePath;

            if (!File.Exists(this.FullPath))
                throw new DirectoryNotFoundException($"Source file does not exist: {sourcePath}");
        }

        public void ReadSizeAndAttribs(TapeDetail tape)
        {
            FileInfo fileInfo = new FileInfo(this.FullPath);

            this.Size = fileInfo.Length;
            this.LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
            this.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            this.CreationTimeUtc = fileInfo.CreationTimeUtc;
            this.Attributes = fileInfo.Attributes;

            // tape.Stats.TotalSize += this.Size;
            
            // add the data size with padding
            // tape.Stats.TotalArchiveSize += Helpers.RoundToNextMultiple(this.Size, 512);

            // add the header size
            // tape.Stats.TotalArchiveSize += 512;
        }

        // public CustomFileCopier ActivateCopy()
        // {
        //     string destinationDir = $"{this.DestinationDisc.RootStagingPath}/data" + this.RelativeDirectory;

        //     CustomFileCopier copier = new CustomFileCopier(this.FullPath, destinationDir);
        //     copier.OverwriteDestination = true;
        //     copier.Preserve = true;

        //     return copier;
        // }
    }
}
