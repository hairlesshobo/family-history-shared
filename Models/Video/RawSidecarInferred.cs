using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class RawSidecarInferred
    {
        // [YamlMember(Description = "People automatically detected as being present in this video")]
        public List<string> People { get; set; } = new List<string>();
    }
}