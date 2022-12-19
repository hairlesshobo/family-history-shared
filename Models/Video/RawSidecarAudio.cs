namespace FoxHollow.FHM.Shared.Models.Video
{
    public class RawSidecarAudio
    {
        /*
            format: mp2
            bitrate_mode: constant
            sampling_rate: 48k
            bitrate: 384k
            channels: 2
            bit_depth: 16 # can this be assumed if no value provided?
        */
        public string Format { get; set; }
        public BitrateMode BitrateMode { get; set; }
        public uint Bitrate { get; set; }
        public uint SamplingRate { get; set; }
        public byte Channels { get; set; } = 2;
        public byte BitDepth { get; set; } = 16;
    }
}