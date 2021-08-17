using System;

namespace Archiver.Shared.Utilities
{
    public static class PathUtils
    {
        public static string CleanPath(string inPath)
            => inPath.Replace(@"\", "/").TrimEnd('/');

        public static string DirtyPath(string inPath)
            => inPath.Replace("/", @"\");

        public static string GetRelativePath(string inPath)
        {
            if (inPath.StartsWith("//"))
            {
                inPath = inPath.TrimStart('/');
                return inPath.Substring(inPath.IndexOf('/'));
            }
                
            return inPath.Split(':')[1];
        }

        public static string GetFileName(string FullPath)
        {
            FullPath = CleanPath(FullPath);

            return FullPath.Substring(FullPath.LastIndexOf('/')+1);

            // string[] nameParts = FullPath.Split('/');
            
            // return nameParts[nameParts.Length-1];
        }
    }
}