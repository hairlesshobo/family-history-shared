using System;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class RawSidecarVideo
    {
        /*
            video_width: 1440
            video_height: 1080
            format: mpeg2
            format_name: HDV 1080i
            bitrate_mode: constant
            frame_rate: 29.970
            frame_count: 3628
            scan_type: interlaced
        */
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
        public string Format { get; set; }
        public string FormatName { get; set; }
        public BitrateMode BitrateMode { get; set; }
        public double FrameRate { get; set; }
        public long FrameCount { get; set; }
        public ScanType ScanType { get; set; }
    }
}