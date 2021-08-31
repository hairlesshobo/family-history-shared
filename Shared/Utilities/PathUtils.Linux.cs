using System;

namespace Archiver.Shared.Utilities
{
    public static partial class PathUtils
    {
        public static class Linux
        {
            public static string GetDrivePath(string driveName)
                => (driveName.IndexOf('/') >= 0 ? driveName : $"/dev/{driveName}");
        }
    }
}