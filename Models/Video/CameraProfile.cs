using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class CameraProfile
    {
        /*     
            "id": "sony_hvr_z1u",
            "commission_date": null,
            "decommission_date": null,
            "name": "Sony HVR-Z1U",
            "capture_person": "Steve Cross",
            "hints": []
        */

        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("commission_date")]
        public Nullable<DateTime> CommissionDate { get; set; }

        [JsonPropertyName("decommission_date")]
        public Nullable<DateTime> DecommissionDate { get; set; }

        [JsonPropertyName("capture_person")]
        public string CapturePerson { get; set; }

        [JsonPropertyName("hints")]
        public List<CameraHint> Hints { get; set; } = new List<CameraHint>();
    }
}