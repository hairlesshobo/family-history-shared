using System;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarGeneral
{
    //   file_name: Tape 1-2022_11_24-16_45_01.m2t
    //   container_format: MPEG-TS / HDV 1080i
    //   size: 395517220 # read from filesystem
    //   capture_dtm: 2022-11-24T16:45:01 # UTC
    //   duration: 00:02:01.48 # hh:mm:ss.ss
    public string OriginalFileName { get; set; }
    public DateTime FileModifyDtm { get; set; }
    public long Size { get; set; }
    public Nullable<DateTime> CaptureDtm { get; set; }
}