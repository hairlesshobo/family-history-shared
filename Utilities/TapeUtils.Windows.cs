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
using FoxHollow.FHM.Shared.Native;
using Microsoft.Win32.SafeHandles;
using static FoxHollow.FHM.Shared.Native.Windows;

namespace FoxHollow.FHM.Shared.Utilities;

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