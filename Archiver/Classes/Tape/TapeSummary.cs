using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;

namespace Archiver.Classes.Tape
{
    public class TapeSummary
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int BlockingFactor { get; set; }
        public DateTime WriteDTM { get; set; }
        public long ExcludedFileCount { get; set; } = 0;
        public long DataSizeBytes 
        { 
            get
            {
                return FlattenFiles().Sum(x => x.Size);
            }
        }
        public int FileCount 
        { 
            get
            {
                return FlattenFiles().Count();
            }
        }
        public int DirectoryCount 
        {
            get
            {
                return this.FlattenDirectories().Count();
            }
        }
        public long TotalArchiveBytes 
        { 
            get
            {
                long size = 0;

                foreach (TapeSourceFile file in FlattenFiles())
                {
                    // header
                    size += 512;

                    // data, rounded to next 512 bytes.. only if the file is greater than 0 bytes
                    if (file.Size > 0)
                        size += HelpersNew.RoundToNextMultiple(file.Size, 512);
                }

                // account for directory entries
                size += 512 * FlattenDirectories().Count();

                // end of archive marker
                size += 1024;

                // round to next block size
                size = HelpersNew.RoundToNextMultiple(size, (512 * Config.TapeBlockingFactor));

                return size;
            }
        }
        public List<TapeSourceDirectory> Directories { get; set; }
        public List<TapeSourceFile> Files { get; set; }
        //public TapeStats Stats { get; set; }

        public TapeSummary()
        {
            this.BlockingFactor = Config.TapeBlockingFactor;
            this.Directories = new List<TapeSourceDirectory>();
            this.Files = new List<TapeSourceFile>();
        }

        public IEnumerable<TapeSourceDirectory> FlattenDirectories()
        {
            return this.Directories
                       .Union(this.FlattenDirectories(this.Directories))
                       .OrderBy(x => x.RelativePath);
        }

        private IEnumerable<TapeSourceDirectory> FlattenDirectories(IEnumerable<TapeSourceDirectory> directory)
        {
            IEnumerable<TapeSourceDirectory> dirEnum = directory.SelectMany(x => x.Directories);

            if (dirEnum.Any(x => x.Directories.Count() > 0))
            {
                dirEnum = dirEnum.Union(FlattenDirectories(dirEnum));
            }

            return dirEnum;
        }

        public IEnumerable<TapeSourceFile> FlattenFiles()
        {
            return this.FlattenDirectories(this.Directories)
                       .SelectMany(x => x.Files)
                       .Union(this.Directories.SelectMany(x => x.Files));
        }
    }
}