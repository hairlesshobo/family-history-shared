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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared;

public static class SysInfo
{
    public static string ExecutionRoot { get; private set; }
    public static string PythonRoot { get; private set; }
    public static string ConfigRoot { get; private set; }
    public static OSType OSType { get; private set; } = OSType.Unknown;
    public static bool OSSupported { get; private set; } = true;

    /// <summary>
    ///     CPU architecture of the currently running system
    /// </summary>
    public static Architecture Architecture => System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;

    /// <summary>
    ///     User-friendly description of the currently running system
    /// </summary>
    public static string Description => System.Runtime.InteropServices.RuntimeInformation.OSDescription;

    /// <summary>
    ///     Identifier for the runtime
    /// </summary>
    public static string Identifier => System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;

    /// <summary>
    ///     Process ID of the currently running application
    /// </summary>
    public static int PID => Process.GetCurrentProcess().Id;

    static SysInfo()
    {
        SysInfo.ExecutionRoot = AppContext.BaseDirectory;
        SysInfo.ConfigRoot = Path.GetFullPath(Path.Combine(SysInfo.ExecutionRoot, "../config"));
        SysInfo.PythonRoot = Path.GetFullPath(Path.Combine(SysInfo.ExecutionRoot, "python"));

        if (OperatingSystem.IsWindows())
            SysInfo.OSType = OSType.Windows;
        else if (OperatingSystem.IsLinux())
            SysInfo.OSType = OSType.Linux;
        else if (OperatingSystem.IsFreeBSD())
            SysInfo.OSType = OSType.FreeBSD;
        else if (OperatingSystem.IsMacOS())
            SysInfo.OSType = OSType.OSX;

        // TODO: update any code that checks for supported platform to instead reference SysInfo.OSSupported
        if (SysInfo.OSType == OSType.FreeBSD || SysInfo.OSType == OSType.OSX || SysInfo.OSType == OSType.Unknown)
            SysInfo.OSSupported = false;
    }
}


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

// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Runtime.InteropServices;
// using FoxHollow.Archiver.Shared.Models;
// using FoxHollow.Archiver.Shared.Models.Config;
// using FoxHollow.Archiver.Shared.Utilities;

// namespace FoxHollow.Archiver.Shared;

// /// <summary>
// ///     Indicates the mode that the application is currently running in
// /// </summary>
// public enum SysInfoMode
// {
//     Unknown,
//     ArchiverCLI,
//     Reserved,  // reserved for ArchiverGUI 
//     TapeServer,
//     TestCLI
// }

// /// <summary>
// ///     Global class for providing useful system-related and config information
// /// </summary>
// public static class SysInfo
// {
//     private static bool _didInit = false;
//     private static ArchiverConfig _config = null;
//     private static SysInfoMode _mode = SysInfoMode.Unknown;
//     private static OSType _osType = OSType.Unknown;
//     private static bool _isOpticalDrivePresent = false;
//     private static bool _isReadonlyFilesystem = true;
//     private static bool _isTapeDrivePresent = true;
//     private static SystemDirectories _directories = null;
//     private static List<ValidationError> _configErrors;


//     /// <summary>
//     ///     Config that was loaded and parsed from appsettings.json
//     /// </summary>
//     public static ArchiverConfig Config => _config;
//     /// <summary>
//     ///     Configured directories for the application
//     /// </summary>
//     public static SystemDirectories Directories => _directories;
//     /// <summary>
//     ///     A list containing any errors that were discovered during loading of the config
//     /// </summary>
//     public static List<ValidationError> ConfigErrors => _configErrors;

//     /// <summary>
//     ///     Operating system type the application is currently running on
//     /// </summary>
//     public static OSType OSType => _osType;

//     /// <summary>
//     ///     Mode that the application is currently runnning as
//     /// </summary>
//     public static SysInfoMode Mode => _mode;

//     /// <summary>
//     ///     Flag indicating whether one or more optical drives were detected
//     /// </summary>
//     public static bool IsOpticalDrivePresent => _isOpticalDrivePresent;

//     /// <summary>
//     ///     Flag indiciating whether the application is currently running from a readonly filesystem 
//     ///     (such as a cdrom)
//     /// </summary>
//     public static bool IsReadonlyFilesystem => _isReadonlyFilesystem;

//     /// <summary>
//     ///     Flag indicating whether one or more tape drives were detected on the local system
//     /// </summary>
//     public static bool IsTapeDrivePresent => _isTapeDrivePresent;

//     /// <summary>
//     ///     Path to the configured tape drive
//     /// </summary>
//     public static string TapeDrive => _mode == SysInfoMode.ArchiverCLI ? Config.Tape.Drive : Config.TapeServer.Drive;

//     /// <summary>
//     ///     CPU architecture of the currently running system
//     /// </summary>
//     public static Architecture Architecture => System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;

//     /// <summary>
//     ///     User-friendly description of the currently running system
//     /// </summary>
//     public static string Description => System.Runtime.InteropServices.RuntimeInformation.OSDescription;

//     /// <summary>
//     ///     Identifier for the runtime
//     /// </summary>
//     public static string Identifier => System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;

//     /// <summary>
//     ///     Process ID of the currently running application
//     /// </summary>
//     public static int PID => Process.GetCurrentProcess().Id;


//     /// <summary>
//     ///     Static constructor
//     /// </summary>
//     static SysInfo()
//     {
//         _mode = GetMode();

//         if (OperatingSystem.IsWindows())
//             _osType = OSType.Windows;
//         else if (OperatingSystem.IsLinux())
//             _osType = OSType.Linux;

//         // these are commented out because we have not yet built support for them into the app. For now, they will 
//         // be identified as "Unknown" and the app will not lauch on an unknown platform.
//         // else if (OperatingSystem.IsFreeBSD())
//         //     _osType = OSType.FreeBSD;
//         // else if (OperatingSystem.IsMacOS())
//         //     _osType = OSType.OSX;
//     }

//     /// <summary>
//     ///     Global initialization which must be called as one of the very first steps
//     ///     of the application startup. This is responsible for populating the system 
//     ///     directory list, detecting local drives, and reading the app config file
//     /// </summary>
//     public static void InitPlatform()
//     {
//         if (!_didInit)
//         {
//             _config = ConfigUtils.ReadConfig(out _configErrors);

//             _directories = new SystemDirectories();
//             _directories.Bin = PathUtils.CleanPath(AppContext.BaseDirectory);
//             _directories.Index = PathUtils.ResolveRelativePath(Path.Combine(_directories.Bin, "../"));
//             _directories.JSON = PathUtils.CleanPathCombine(_directories.Index, "json");
//             _directories.ISO = PathUtils.CleanPathCombine(_directories.Index, "../iso");
//             _directories.DiscStaging = (
//                   !String.IsNullOrWhiteSpace(Config.Disc.StagingDir)
//                 ? PathUtils.ResolveRelativePath(Path.Combine(_directories.Index, Config.Disc.StagingDir))
//                 : PathUtils.CleanPath(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()))
//             );

//             _isOpticalDrivePresent = OpticalDriveUtils.GetDriveNames().Any();
//             _isReadonlyFilesystem = TestForReadonlyFs();
//             _isTapeDrivePresent = TapeUtilsNew.IsTapeDrivePresent();
//         }
//     }

//     public static void WriteSystemInfo(bool writeConfig = false, bool color = false)
//     {
//         PrintHeader(color, "System Information:");
//         PrintField(color, 11, "OS Platform", $"{SysInfo.OSType.ToString()} ({SysInfo.Architecture.ToString()})");
//         PrintField(color, 11, "Description", SysInfo.Description);
//         PrintField(color, 11, "Identifier", SysInfo.Identifier);

//         if (writeConfig && Mode == SysInfoMode.TapeServer)
//         {
//             Console.WriteLine();
//             PrintHeader(color, "Configuration:");
//             PrintField(color, 12, "Tape Drive", Config.TapeServer.Drive);
//         }
//     }

//     private static void PrintHeader(bool color, string value)
//     {
//         if (color)
//             Formatting.WriteLineC(ConsoleColor.Magenta, value);
//         else
//             Console.WriteLine(value);
//     }

//     private static void PrintField(bool color, int width, string fieldName, string value)
//     {
//         if (color == true)
//         {
//             Formatting.WriteC(ConsoleColor.Cyan, $"  {fieldName.PadLeft(width)}: ");
//             Console.WriteLine(value);
//         }
//         else
//             Console.WriteLine($"  {fieldName.PadLeft(width)}: {value}");
//     }

//     /// <summary>
//     ///     Get the mode that the application is currently running as
//     /// </summary>
//     /// <returns>Currently running mode</returns>
//     private static SysInfoMode GetMode()
//     {
//         string assemblyName = Assembly.GetEntryAssembly().GetName().Name;

//         switch (assemblyName)
//         {
//             case "Archiver":
//                 return SysInfoMode.ArchiverCLI;

//             case "TapeServer":
//                 return SysInfoMode.TapeServer;

//             case "TestCLI":
//                 return SysInfoMode.TestCLI;

//             default:
//                 return SysInfoMode.Unknown;
//         }
//     }

//     /// <summary>
//     ///     Test if the app is running on a readonly filesystem
//     /// </summary>
//     /// <returns>true if on readonly filesystem, false otherwise</returns>
//     private static bool TestForReadonlyFs()
//     {
//         string testFile = Path.Combine(Directories.Bin, "__accesstest.tmp");
//         bool canWrite = true;

//         if (File.Exists(testFile))
//         {
//             try
//             {
//                 File.Delete(testFile);
//             }
//             catch
//             {
//                 canWrite = false;
//             }
//         }

//         try
//         {
//             File.WriteAllText(testFile, String.Empty);
//             File.Delete(testFile);
//         }
//         catch
//         {
//             canWrite = false;
//         }

//         return !canWrite;
//     }

// }