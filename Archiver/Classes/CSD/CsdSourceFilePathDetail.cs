using System;
using Archiver.Shared.Utilities;

namespace Archiver.Classes.CSD
{
    public class CsdSourceFilePathDetail
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
                _FullPath = PathUtils.CleanPath(value);

                this.Name = PathUtils.GetFileName(_FullPath);
                this.RelativePath = PathUtils.GetRelativePath(value);
                this.RelativeDirectory = this.RelativePath.Substring(0, this.RelativePath.LastIndexOf('/'));
            }
        }
        public string RelativePath { get; set; }
        public string RelativeDirectory { get; set; }

        public CsdSourceFilePathDetail CloneCsdPaths()
        {
            return new CsdSourceFilePathDetail()
            {
                Name = this.Name,
                FullPath = this.FullPath,
                RelativePath = this.RelativePath,
                RelativeDirectory = this.RelativeDirectory
            };
        }
    }
}
