using System;

namespace Archiver.Classes.Shared
{
    public interface ISourceFile
    {
        string Name { get; set; }
        string Extension { get; }
        string RelativePath { get; }
        string RelativeDirectory { get; set; }
        string FullPath { get; set; }
        string SourceRootPath { get; }
        long Size { get; set; }
        string Hash { get; set; }
        DateTime LastAccessTimeUtc { get; set; }
        DateTime LastWriteTimeUtc { get; set; }
        DateTime CreationTimeUtc { get; set; }
    }
}