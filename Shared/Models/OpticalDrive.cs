using System;

namespace Archiver.Shared.Models
{
    public class OpticalDrive
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string MountPoint { get; set; }
        public string VolumeLabel { get; set; }

        // TODO: Figure out what format is and decide if it is needed
        public string VolumeFormat { get; set; }
        public bool IsReady { get; set; }
        public bool IsDiscLoaded { get; set; }

        // TODO: Detect drive capabilities
        public OpticalDriveCapabilities Capabilities => new OpticalDriveCapabilities();
    }
}