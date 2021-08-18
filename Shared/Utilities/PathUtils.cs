using System;
using System.Collections.Generic;
using System.IO;

namespace Archiver.Shared.Utilities
{
    public static class PathUtils
    {
        public static string CleanPath(string inPath)
            => inPath.Replace(@"\", "/").TrimEnd('/');

        public static string CleanPathCombine(params string[] paths)
            => PathUtils.ResolveRelativePath(Path.Combine(paths));

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

        public static string ResolveRelativePath(string inPath)
        {
            if (!(File.Exists(inPath) || Directory.Exists(inPath)))
                return null;

            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(inPath);

            if (attr.HasFlag(FileAttributes.Directory))
                return PathUtils.CleanPath(new DirectoryInfo(inPath).FullName);
            else
                return PathUtils.CleanPath(new FileInfo(inPath).FullName);

        }

        public static string GetFileName(string FullPath)
        {
            FullPath = CleanPath(FullPath);

            return FullPath.Substring(FullPath.LastIndexOf('/')+1);
        }

        public static string[] CleanExcludePaths(string[] excludePaths)
        {
            List<string> validatedPaths = new List<string>();
            
            foreach (string excPath in excludePaths)
            {
                string cleanExcPath = CleanPath(excPath);

                if (File.Exists(cleanExcPath) || Directory.Exists(cleanExcPath))
                    validatedPaths.Add(cleanExcPath);
            }

            return validatedPaths.ToArray();
        }


        public static string LinuxGetDrivePath(string driveName)
            => (driveName.IndexOf('/') >= 0 ? driveName : $"/dev/{driveName}");
    }
}