using System;

namespace Archiver.Classes.Shared
{
    public delegate void MD5_CompleteDelegate(string hash);
    public delegate void MD5_ProgressChangedDelegate(Md5Progress progress);

    public class Md5Progress 
    {
        public long TotalCopiedBytes { get; set; } = 0;
        public long BytesCopiedSinceLastupdate { get; set; } = 0;
        public long TotalBytes { get; set; } = 0;
        public double PercentCopied { get; set; } = 0.0;
        public double InstantTransferRate { get; set; } = 0.0;
        public double AverageTransferRate { get; set; } = 0.0;
        public TimeSpan ElapsedTime { get; set; }
        public bool Complete { get; set; } = false;
        public string FileName { get; set; } = String.Empty;
    }
}