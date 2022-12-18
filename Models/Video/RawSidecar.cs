using System.Text.Json.Nodes;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class RawSidecar
    {
        public RawSidecarGeneral General { get; set; } = new RawSidecarGeneral();
        public RawSidecarVideo Video { get; set; } = new RawSidecarVideo();
        public RawSidecarAudio Audio { get; set; } = new RawSidecarAudio();
        public RawSidecarHash Hash { get; set; } = new RawSidecarHash();
        public RawSidecarInferred Inferred { get; set; } = new RawSidecarInferred();
        public JsonNode MediaInfo { get; set; }
    }
}