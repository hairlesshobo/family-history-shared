using System;
using System.Text.RegularExpressions;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

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
    }
}