using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class RawSidecarDetails
    {
        public List<string> People { get; set; } = new List<string>();
    }
}