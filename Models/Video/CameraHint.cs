using System;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class CameraHint
    {
        public string Key { get; set; }
        public string Section { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Weight { get; set; }
    }
}