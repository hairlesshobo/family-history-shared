using System;
using System.Collections.Generic;
using System.Linq;

namespace Archiver.Classes.Tape
{
    public class TapeSummary
    {
        public string Name { get; set; }
        public int BlockingFactor { get; set; }
        public DateTime LastWriteDTM { get; set; }
        public long DataSizeBytes { get; set; }
        public int FileCount 
        { 
            get
            {
                return this.FlattenDirectories(this.Directories).SelectMany(x => x.Files).Count();
            }
        }
        public int DirectoryCount 
        {
            get
            {
                return this.FlattenDirectories(this.Directories).Count();
            }
        }
        public long TotalArchiveBytes { get; set; }
        public List<TapeSourceDirectory> Directories { get; set; }
        public List<TapeSourceFile> Files { get; set; }
        public TapeStats Stats { get; set; }

        public TapeSummary()
        {
            this.BlockingFactor = Config.TapeBlockingFactor;
            this.Directories = new List<TapeSourceDirectory>();
            this.Files = new List<TapeSourceFile>();
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
    }
}