using System;
using YamlDotNet.Serialization;

namespace FoxHollow.FHM.Shared.Models;

public class RawVideoScene
{
    // camera: Sony HVR-Z1U
    // camera_id: sony_hvr_z1u
    // capture_person: Steve
    // location: # lat/long only if provided by mediainfo, name to be populated manually
    

    public string Camera { get; set; }

    [YamlMember(Alias = "camera_id")]
    public string CameraID { get; set; }
    public string CapturePerson { get; set; }
    public RawVideoSceneLocation Location { get; set; } = new RawVideoSceneLocation();
}