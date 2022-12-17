using System;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class CameraHint
    {
        /* {
            "key": "Format",
            "section": "general",
            "type": "mediainfo",
            "value": "BDAV",
            "weight": "low"
        }, */

        
        public string Key { get; set; }
        public string Section { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Weight { get; set; }
    }
}