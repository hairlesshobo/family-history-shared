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
using System.Collections.Generic;
using System.IO;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;

namespace Archiver.Shared.Utilities
{
    public static partial class PathUtils
    {
        public static string GetDrivePath(string driveName)
        {
            if (SysInfo.OSType == OSType.Windows)
                return Windows.GetDrivePath(driveName);
            else if (SysInfo.OSType == OSType.Linux)
                return Linux.GetDrivePath(driveName);

            throw new UnsupportedOperatingSystemException();
        }
        
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
    }
}