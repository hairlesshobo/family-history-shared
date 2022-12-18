using System;
using System.Collections.Generic;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class CameraProfile
    {
        /*     
            "ID": "sony_hvr_z1u",
            "CommissionDate": null,
            "DecommissionDate": null,
            "Name": "Sony HVR-Z1U",
            "Hints": []
        */
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime CommissionDate { get; set; }
        public DateTime DecommissionDate { get; set; }
        public List<CameraHint> Hints { get; set; } = new List<CameraHint>();
    }
}