using System;

namespace Archiver.Shared.Models
{
    public class TapeDriveDetails
    {
        string DriveType { get; }
        public bool TapeLoaded { get; set; } = false;
    }
}