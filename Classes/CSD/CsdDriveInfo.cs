using System;

namespace Archiver.Classes.CSD
{
    public class CsdDriveInfo
    {
        public string Brand { get; set; }
        public string Line { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string FormFactor { get; set; }
        public bool? SolidState { get; set; }
        public DateTime? ManufactureDate { get; set; }
    }
}