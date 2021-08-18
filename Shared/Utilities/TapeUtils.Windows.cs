using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.TapeDrivers;

namespace Archiver.Shared.Utilities
{
    public static partial class TapeUtilsNew
    {
        private static bool WindowsIsTapeDrivePresent()
        {
            try
            {
                using (NativeWindowsTapeDriver tape = new NativeWindowsTapeDriver(SysInfo.TapeDrive, false))
                {}
            }
            catch (TapeOperatorWin32Exception exception)
            {
                if (exception.HResult == -2146232832)
                    return false;
            }

            return true;
        }
    }
}