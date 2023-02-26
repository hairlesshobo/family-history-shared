using YamlDotNet.Serialization;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarHash
{
    /*
        md5: ee89f300725e6b2e0c0080f41bbb2ab2
        sha1: f5101da5ec1281ab583c5d82c704a3f9878e7b56
        fingerprint: # generate "quick" fingerprint
    */
    [YamlMember(Alias = "md5")]
    public string MD5 { get; set; }

    [YamlMember(Alias = "sha1")]
    public string SHA1 { get; set; }
    
    // public string Fingerprint { get; set; }

    [YamlMember(Alias = "image_md5")]
    public string ImageMD5 { get; set; }
}
