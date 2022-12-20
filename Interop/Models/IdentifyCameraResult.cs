using System;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Interop.Models
{
    public class IdentifyCameraResult
    {
        // 'file_path': media_file_path,
        // 'mediainfo_path': mediainfo_file_path,
        // 'identified_cam_name': identified_cam_name,
        // 'confidence': confidence,
        // 'confidence_pass': confidence_pass,
        // 'scores': dict(scores)

        [JsonPropertyName("file_path")]
        public string FilePath { get; set; }

        [JsonPropertyName("mediainfo_path")]
        public string MediainfoPath { get; set; }

        [JsonPropertyName("identified_cam_name")]
        public string IdentifiedCamName { get; set; }

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("confidence_pass")]
        public bool ConfidencePass { get; set; }
        // public 
    }
}