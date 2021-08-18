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
        private static bool LinuxIsTapeDrivePresent()
            => File.Exists(SysInfo.TapeDrive);
    }
}