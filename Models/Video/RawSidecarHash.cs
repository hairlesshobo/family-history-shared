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

using YamlDotNet.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video;

public class RawSidecarHash
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
}