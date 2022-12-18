/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Interfaces;

namespace FoxHollow.FHM.Shared.Utilities
{
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
}