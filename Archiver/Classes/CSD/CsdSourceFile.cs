using System;
using System.IO;
using Archiver.Shared.Interfaces;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;
using Newtonsoft.Json;

namespace Archiver.Classes.CSD
{
    public class CsdSourceFile : CsdSourceFilePathDetail, ISourceFile
    {
        private bool _copied = false;

        public CsdSourceFilePathDetail OriginalFile { get; set; } = null;
        public long Size { get; set; } = -1;
        public bool? FileDeleted { get; set; } = null;
        public string Hash { get; set; }
        public bool Copied 
        { 
            get => _copied; 
            set
            {
                bool isBeingMarkedAsCopied = _copied == false && value == true;

                _copied = value;

                if (isBeingMarkedAsCopied && this.DestinationCsd != null)
                    this.DestinationCsd.MarkFileCopied(this);
            }
        }
        public DateTime ArchiveTimeUtc { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public FileAttributes Attributes { get; set; }
        [JsonIgnore]
        public CsdDetail DestinationCsd { get; set; } = null;
        [JsonIgnore]
        public string SourceRootPath
        {
            get
            {
                return this.FullPath.Substring(0, this.FullPath.Length - this.Name.Length - this.RelativeDirectory.Length - 1);
            }
        }

        public CsdSourceFile()
        {
            CsdGlobals._jsonReadSourceFiles.Add(this);
        }

        public CsdSourceFile(string sourcePath, bool scanForRenames = false)
        {
            this.FullPath = sourcePath;

            if (!File.Exists(this.FullPath))
                throw new DirectoryNotFoundException($"Source file does not exist: {sourcePath}");

            // we found a file on the filesystem, but it is already in the archive
            if (CsdGlobals._sourceFileDict.ContainsKey(this.RelativePath))
                CsdGlobals._existingFileCount += 1;

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
                    CsdGlobals._newFileCount += 1;
                    CsdGlobals._sourceFileDict.Add(this.RelativePath, this);
                }
            }
        }

        public void ReadSizeAndAttribs()
        {
            // only read attribs if we haven't already done so
            if (this.LastWriteTimeUtc.Equals(default(DateTime)))
            {
                FileInfo fileInfo = new FileInfo(this.FullPath);

                this.Size = fileInfo.Length;
                this.LastAccessTimeUtc = fileInfo.LastAccessTimeUtc;
                this.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
                this.CreationTimeUtc = fileInfo.CreationTimeUtc;
                this.Attributes = fileInfo.Attributes;

                CsdGlobals._totalSizePending += this.Size;
            }
        }

        public void AssignCsd()
        {
            if (this.Size >= 0)
            {
                this.DestinationCsd = CsdUtils.GetDestinationCsd(this.Size);
                this.DestinationCsd.AddFile(this);
            }
        }

        public CustomFileCopier ActivateCopy(string driveLetter)
        {
            string destinationRoot = $"{driveLetter}/data";

            CustomFileCopier copier = new CustomFileCopier(this, destinationRoot);
            copier.OverwriteDestination = true;
            copier.Preserve = true;

            return copier;
        }
    }
}
