//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

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