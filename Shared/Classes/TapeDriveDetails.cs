using System;

namespace Archiver.Shared.Classes
{
    public class TapeDriveDetails
    {
        string DriveType { get; }
        public bool TapeLoaded { get; set; } = false;
    }
}