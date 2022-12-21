using System;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class CameraHint
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("section")]
        public string Section { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }
    }
}