using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("commission_date")]
        public Nullable<DateTime> CommissionDate { get; set; }

        [JsonPropertyName("decommission_date")]
        public Nullable<DateTime> DecommissionDate { get; set; }

        [JsonPropertyName("hints")]
        public List<CameraHint> Hints { get; set; } = new List<CameraHint>();
    }
}