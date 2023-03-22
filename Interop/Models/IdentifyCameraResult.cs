using System;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Interop.Models;

/// <summary>
///     Result of camera identification from Python interop
/// </summary>
public class IdentifyCameraResult
{
    // 'file_path': media_file_path,
    // 'mediainfo_path': mediainfo_file_path,
    // 'identified_cam_name': identified_cam_name,
    // 'confidence': confidence,
    // 'confidence_pass': confidence_pass,
    // 'scores': dict(scores)

    /// <summary>
    ///     Path to the video file that was analyzed
    /// </summary>
    /// <value></value>
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; }

    /// <summary>
    ///     Path to the mediainfo metadata file that was used for analysis
    /// </summary>
    /// <value></value>
    [JsonPropertyName("mediainfo_path")]
    public string MediainfoPath { get; set; }

    /// <summary>
    ///     Name of the camera that was identified
    /// </summary>
    /// <value></value>
    [JsonPropertyName("identified_cam_name")]
    public string IdentifiedCamName { get; set; }

    /// <summary>
    ///     How confident is the result of the camera identification
    /// </summary>
    /// <value></value>
    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }

    /// <summary>
    ///     Did the analysis achieve the required confidence necessary 
    ///     for the result to be considered accurated?
    /// </summary>
    /// <value></value>
    [JsonPropertyName("confidence_pass")]
    public bool ConfidencePass { get; set; }
}