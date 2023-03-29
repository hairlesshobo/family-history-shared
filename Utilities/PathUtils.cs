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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared.Utilities;

// TODO: move to a separate library
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


    /// <summary>
    ///     Calculate the relative depth between two directories
    /// </summary>
    /// <param name="root"></param>
    /// <param name="testDir"></param>
    /// <returns>Numerical depth difference between the root and test directory</returns>
    public static int GetRelativeDepth(string root, string testDir)
    {
        // TODO: Convert this to traverse upwards until it finds the matching dir
        root = PathUtils.CleanPath(root);
        testDir = PathUtils.CleanPath(testDir);

        int depth = 0;

        if (testDir.StartsWith(root))
        {
            int substr = root.Length;

            if (substr > testDir.Length)
                return -1;

            var testPart = testDir.Substring(substr);

            depth = testPart.Count((tChar) => tChar == '/');
        }

        return depth;
    }

    /// <summary>
    ///     Lookup the directory at the depth relative to the specified root directory.
    /// 
    ///     An example of this:
    ///         root = /home/user/source
    ///         test = /home/user/source/some/sub/path/far/below/the/root
    ///         depth = 2
    /// 
    ///     The result would be /home/user/source/some/sub
    /// </summary>
    /// <param name="root">The root directory to use as depth "0"</param>
    /// <param name="test">A path that is a below the root directory to use as the test directory</param>
    /// <param name="desiredDepth">depth location desired to be found</param>
    /// <returns>Full path to the directory at the specified depth</returns>
    public static DirectoryInfo FindDirectoryAtRelativeDepth(string root, string test, int desiredDepth)
    {
        DirectoryInfo rootDirInfo = new DirectoryInfo(root);
        DirectoryInfo testDirInfo = new DirectoryInfo(test);

        DirectoryInfo desiredDirInfo = testDirInfo;

        int currentDepth = PathUtils.GetRelativeDepth(rootDirInfo.FullName, testDirInfo.FullName);

        for (int i = currentDepth; i > desiredDepth; i--)
        {
            if (desiredDirInfo == null)
                throw new DirectoryNotFoundException($"Unable to find directory at relative depth {desiredDepth}");

            desiredDirInfo = desiredDirInfo.Parent;
        }

        return desiredDirInfo;
    }


    /// <summary>
    ///     Combine, clean and resolve provided path
    /// </summary>
    /// <param name="paths">Parts of the path to combine</param>
    /// <returns>Clean, combined, and resolved path</returns>
    public static string CleanPathCombine(params string[] paths)
        => PathUtils.ResolveRelativePath(CombinePaths(paths));

    public static string CombinePaths(params string[] parts)
    {
        List<string> newParts = new List<string>();

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];

            if (String.IsNullOrWhiteSpace(part))
                continue;

            if (i > 0 && part.StartsWith('/'))
                part = part.TrimStart('/');

            part = part.TrimEnd('/');

            newParts.Add(part);
        }

        return String.Join("/", newParts);
    }


    /// <summary>
    ///     Clean that path (convert it to linux style paths with / )
    /// </summary>
    /// <param name="inPath">Path to clean</param>
    /// <returns>Clean path</returns>
    public static string CleanPath(string inPath)
    {
        string replacedPath = inPath.Replace(@"\", "/");
        
        if (!String.Equals(replacedPath, "/"))
            replacedPath = replacedPath.TrimEnd('/');

        return replacedPath;
    }

    /// <summary>
    ///     Dirty the path (convert it to windows-style \)
    /// </summary>
    /// <param name="inPath">Path to make dirty</param>
    /// <returns>Dirty path</returns>
    public static string DirtyPath(string inPath)
        => inPath.Replace("/", @"\");

    /// <summary>
    ///     Get the relative portion of a file path (in this context, that means to
    ///     get the file path without the leading drive letter or unc path)
    /// 
    /// Examples:
    ///     /mnt/drive/path     ==>   /mnt/drive/path
    ///     D:/path/to/dir      ==>   /path/to/dir
    ///     //server/path/dir   ==>   /path/dir
    /// </summary>
    /// <param name="inPath">Path to process</param>
    /// <returns></returns>
    public static string GetRelativePath(string inPath)
    {
        // TODO: add handling for paths with protocol

        // if the path is a UNC path, we strip the server 
        if (inPath.StartsWith("//"))
        {
            inPath = inPath.TrimStart('/');
            return inPath.Substring(inPath.IndexOf('/'));
        }

        // strip the drive letter
        return inPath.Split(':')[1];
    }

    /// <summary>
    ///     Resolve the provided relative path into an absolute path.
    ///     Note: Preceeding drive letter or protocol is supported.
    /// 
    /// Examples:
    ///     D:/path/to/../directory          ==>   D:/path/directory
    ///     \\server\some/folder/./another   ==>   //server/some/folder/another
    /// </summary>
    /// <param name="inPath">Path that is to be resolved</param>
    /// <returns>Resolved path</returns>
    public static string ResolveRelativePath(string inPath)
    {
        // Start by performing basic cleaning on the path (convert \ to  / and trim trailing /)
        inPath = CleanPath(inPath);

        // if path starts with ./ or ../, prepend the path our executable is in
        if (inPath.StartsWith("./") || inPath.StartsWith("../"))
            inPath = $"{SysInfo.ExecutionRoot}/{inPath}";

        // a bare filename is provided with no leading ./, ../, or / is provided..
        // we prepend current directory.. but only if there is also no
        // ':' present, which would indicate a protocol was provided
        if (!inPath.StartsWith('/') && inPath.IndexOf(':') < 0)
            inPath = $"{SysInfo.ExecutionRoot}/{inPath}";

        // if there is a protocol or drive provided, store it for later
        string protocolOrDrive = String.Empty;

        if (inPath.IndexOf(':') >= 0)
            protocolOrDrive = inPath.Substring(0, inPath.IndexOf(':') + 1);

        // strip out the drive letter or protocol, if provided
        string pathWithoutProtocol = inPath.Substring(protocolOrDrive.Length);

        // if our path stars with a double // (such as a unc path or a protocol path)
        // we need to save that for later when we reconstruct the path
        bool doublePrefix = pathWithoutProtocol.StartsWith("//");

        // clean up any double slashes present (in case /a/path//to/something was provided, for example)
        pathWithoutProtocol = pathWithoutProtocol.Replace("//", "/");

        // split the path into parts
        List<string> pathParts = pathWithoutProtocol.TrimStart('/').Split('/').ToList();

        // we need to use a while, because if we change the object while looping with
        // a foreach, an exception will be thrown by the framework
        int index = 0;
        while (true)
        {
            if (index >= pathParts.Count)
                break;

            // get the current part
            string part = pathParts[index];

            // if the current part is ".." (parent directory indicator), we need to remove it
            // and the directory part thast occurs before the ".."
            if (part == ".." && index > 0)
            {
                pathParts.RemoveAt(index);
                pathParts.RemoveAt(index - 1);

                // since we removed two enties, we need to back up one before we continue processing
                index--;
            }

            // if the current part is "." we can just remove it because it is not needed
            else if (part == ".")
                pathParts.RemoveAt(index);

            // nothing to do, so we move on to the next part to check
            else
                index++;
        }

        // reconstruct the path and return it
        return protocolOrDrive + (doublePrefix ? "//" : "/") + String.Join("/", pathParts);
    }

    /// <summary>
    ///     Get the file name portion of the provided path
    /// </summary>
    /// <param name="fullPath">full path</param>
    /// <returns>file name only</returns>
    public static string GetFileName(string fullPath)
    {
        fullPath = CleanPath(fullPath);

        return fullPath.Substring(fullPath.LastIndexOf('/') + 1);
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

    /// <summary>
    ///     Return the path, relative to the root directory. 
    /// 
    ///     Note: The path will be cleaned and the return relative path will
    ///     not begin with a './' or '/'
    /// 
    ///     Example: 
    ///         root = /home/user/source
    ///         fullPath = /home/user/source/some/sub/path
    /// 
    ///         return: some/sub/path
    /// </summary>
    /// <param name="root">Path to use as the root path</param>
    /// <param name="fullPath">Full path to resolve to root relative</param>
    /// <returns>Root relative path without preceeding './' or '/'</returns>
    public static string GetRootRelativePath(string root, string fullPath)
    {
        root = PathUtils.CleanPath(root);
        fullPath = PathUtils.CleanPath(fullPath);

        if (!fullPath.StartsWith(root))
            throw new ArgumentException($"Provided path must begin with the provided root path", nameof(fullPath));

        var rootRelativePath = fullPath.Substring(root.Length).TrimStart('/');

        return rootRelativePath;
    }

    /// <summary>
    ///     Get the extension portion of the path
    /// </summary>
    /// <param name="path">File name or full path</param>
    /// <returns>Extension of the path, with preceeding '.'</returns>
    public static string GetExtension(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));

        // Clean the path to be used for testing
        var testPath = CleanPath(path);

        if (testPath.Contains('/'))
        {
            var testPathParts = testPath.Split('/');
            testPath = testPathParts[testPathParts.Length - 1];
        }

        // no extension found
        if (!testPath.Contains('.'))
            return "";

        string[] parts = testPath.Split('.');

        string lastPart = parts[parts.Length - 1];

        return "." + lastPart;
    }

    /// <summary>
    ///     Strip the protocol portion of a path (smb://, file://, etc.)
    /// </summary>
    /// <param name="path">The path to strip the protocol from</param>
    /// <returns>A path without a preceeding protocol</returns>
    public static string StripProtocol(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));

        if (!path.Contains("://"))
            return path;

        var parts = path.Split("://");
        var newPath = parts[1];

        // working with a windows style path
        if (newPath.Length >= 2 && newPath[1] == ':')
            return newPath;

        if (!newPath.StartsWith('/'))
            newPath = '/' + newPath;

        return newPath;
    }
}