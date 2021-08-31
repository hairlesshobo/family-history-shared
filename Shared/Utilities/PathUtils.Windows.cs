using System;

namespace Archiver.Shared.Utilities
{
    public static partial class PathUtils
    {
        public static class Windows
        {
            /// <summary>
            ///     Converts the provided drive name (ex: "A:") to the raw path (ex: "\\.\A:")
            /// </summary>
            /// <param name="driveName">Name of the drive</param>
            /// <returns>Raw formatted drive path</returns>
            public static string GetDrivePath(string driveName)
            {
                if (driveName.StartsWith(@"\\.\"))
                    return driveName;

                driveName = CleanDriveName(driveName);

                return @"\\.\" + driveName;
            }

            public static string CleanDriveName(string driveName)
            {
                driveName = driveName.Trim('/');
                driveName = driveName.Trim('\\');
                driveName = driveName.ToUpper();

                if (!driveName.EndsWith(":"))
                    driveName += ":";

                return driveName;
            }
        }
    }
}