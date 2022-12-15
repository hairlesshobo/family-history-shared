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
using FoxHollow.FHM.Shared.Native;
using Microsoft.Win32.SafeHandles;
using static FoxHollow.FHM.Shared.Native.Windows;

namespace FoxHollow.FHM.Shared.Utilities
{
    public static partial class TapeUtilsNew
    {
        private static bool WindowsIsTapeDrivePresent()
        {
            // TODO: Why is the native tape driver class not being used here?
            // TODO: This should scan for any tape drive, not just the one specified in config
            //       validatoin of specified tape drive should be done during app init
            SafeFileHandle handle = Windows.CreateFile(
                // SysInfo.TapeDrive,
                @"\\.\Tape0",
                GENERIC_READ | GENERIC_WRITE,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero
                );

            if (handle != null && handle.IsInvalid)
                return false;

            if (!handle.IsClosed)
                handle.Close();

            return true;
        }
    }
}