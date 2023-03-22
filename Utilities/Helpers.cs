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

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Utilities;

public static class HelpersNew
{
    public static long RoundToNextMultiple(long value, int multiple)
    {
        if (value == 0)
            return 0;

        long nearestMultiple = (long)Math.Round((value / (double)multiple), MidpointRounding.ToPositiveInfinity) * multiple;

        return nearestMultiple;
    }


    public static async Task<TResult> ReadJsonFileAsync<TResult>(string filePath, bool required = true)
    {

        // if it doesn't exists and is required, let File.OpenRead throw an exception below
        if (!File.Exists(filePath) && !required)
            return default(TResult);

        using (FileStream stream = File.OpenRead(filePath))
        {
            var config = await JsonSerializer.DeserializeAsync<TResult>(stream, Static.DefaultJso);
            return config;
        }
    }
}