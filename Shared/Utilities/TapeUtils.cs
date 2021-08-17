using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.TapeDrivers;

namespace Archiver.Shared.Utilities
{
    public static class TapeUtilsNew
    {
        public static string CleanTapeDrivePath(string path)
        {
            // Clean up windows path name
            if (SysInfo.OSType == OSType.Windows)
                path = path.Replace('/', '\\');

            // provide mapping from tapeX to OS specific tape path or from "auto" to first tape of the current platform
            if (path.ToLower() == "auto" || Regex.Match(path, @"tape\d+", RegexOptions.IgnoreCase).Success)
            {
                string tapeNum = "0";
                
                if (path.ToLower() != "auto")
                    tapeNum = path.Substring(4);

                if (SysInfo.OSType == OSType.Windows)
                    path = @$"\\.\Tape{tapeNum}";
                else if (SysInfo.OSType == OSType.Linux)
                    path = $"/dev/nst{tapeNum}";
                else
                    throw new UnsupportedOperatingSystemException();
            }

            return path;
        }

        public static byte[] GetStringPaddedBytes(string Input, uint BlockSize)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(Input);
            int lengthNeeded = (int)HelpersNew.RoundToNextMultiple(rawData.Length, (int)BlockSize);
            Array.Resize(ref rawData, lengthNeeded);

            return rawData;
        }

        // TODO: Find a way to split this that works for archiver and tape server
        public static bool IsTapeDrivePresent()
        {
            if (SysInfo.Config.Tape.Driver.ToLower() != "auto-remote")
            {
                if (SysInfo.OSType == OSType.Windows)
                    return WindowsIsTapeDrivePresent();
                else if (SysInfo.OSType == OSType.Linux)
                    return LinuxIsTapeDrivePresent();
            }

            // TODO: finish implementing tape drive detection
            return false;
        }

        #region Linux Utilities
        private static bool LinuxIsTapeDrivePresent()
            => File.Exists(SysInfo.TapeDrive);
            
        #endregion

        #region Windows Utilities
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
        #endregion

    }
}