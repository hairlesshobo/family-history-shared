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
using Archiver.Shared.Classes.CSD;
using Archiver.Utilities.CSD;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.CSD
{
    public static class ShowCsdSummary
    {
        public static void StartOperation()
        {
            // string drive = CsdUtils.SelectDrive();

            // if (drive != null)
            // {
            //     // TODO: read summary from index, not drive
            //     CsdSummary summary = CsdUtils.ReadSummaryFromCsd(drive);

            //     using (Pager pager = new Pager())
            //     {
            //         pager.Start();
                    
            //         pager.AutoScroll = false;
            //         pager.ShowLineNumbers = true;

            //         foreach (string line in text.Split("\n"))
            //         {
            //             pager.AppendLine(line);
            //         }

            //         pager.WaitForExit();
            //     }
            // }
        }
    }
}