using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.Native;
using Microsoft.Win32.SafeHandles;

namespace Archiver.Shared.Utilities
{
    public static partial class OpticalDriveUtils
    {
        private static class Windows
        {
            public static List<OpticalDrive> GetDrives()
            {
                return DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.CDRom).Select(x => 
                {
                    string driveName = CleanDriveName(x.Name);

                    return new OpticalDrive()
                    {
                        Name = driveName,
                        MountPoint = driveName,
                        FullPath = GetOpticalDrivePath(driveName),
                        IsReady = x.IsReady,
                        VolumeLabel = (x.IsReady ? x.VolumeLabel : null),
                        VolumeFormat = (x.IsReady ? x.DriveFormat : null),
                        IsDiscLoaded = x.IsReady
                    };
                }).ToList();
            }

            public static string GetDriveLabel(string driveName)
            {
                DriveInfo drive = DriveInfo.GetDrives().FirstOrDefault(x => CleanDriveName(x.Name) == CleanDriveName(driveName));

                if (drive == null)
                    return null;

                return drive.VolumeLabel;
            }

            public static FileStream GetDriveRawStream(string driveName)
            {
                string drivePath = GetOpticalDrivePath(driveName);
                string md5Hash = String.Empty;

                SafeFileHandle handle = Native.Windows.CreateFile(
                    drivePath,
                    Native.Windows.GENERIC_READ,
                    Native.Windows.FILE_SHARE_READ | Native.Windows.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    Native.Windows.OPEN_EXISTING,
                    0,
                    IntPtr.Zero
                );

                if (handle.IsInvalid)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                    // throw new NativeMethodException("CreateFile", Marshal.GetLastWin32Error());
                    // throw new TapeOperatorWin32Exception("CreateFile", Marshal.GetLastWin32Error());

                // byte[] buffer = new byte[256 * 1024]; // 256KB buffer


                FileStream stream = new FileStream(handle, FileAccess.Read);
                
                return stream;
            }
            
            public static string GenerateDiscMD5(string driveName)
            {
                string drivePath = GetOpticalDrivePath(driveName);
                string md5Hash = String.Empty;

                SafeFileHandle handle = Native.Windows.CreateFile(
                    drivePath,
                    Native.Windows.GENERIC_READ,
                    Native.Windows.FILE_SHARE_READ | Native.Windows.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    Native.Windows.OPEN_EXISTING,
                    0,
                    IntPtr.Zero
                );

                if (handle.IsInvalid)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                    // throw new NativeMethodException("CreateFile", Marshal.GetLastWin32Error());
                    // throw new TapeOperatorWin32Exception("CreateFile", Marshal.GetLastWin32Error());

                byte[] buffer = new byte[256 * 1024]; // 256KB buffer


                using (FileStream stream = new FileStream(handle, FileAccess.Read, (int)buffer.Length, false))
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    // Stopwatch sw = Stopwatch.StartNew();
                    int currentBlockSize = 0;

                    // long lastSample = sw.ElapsedMilliseconds;
                    long totalBytesRead = 0;
                    int md5Offset = 0;
                    // double currentPercent = 0.0;

                    // m_stream.Read(buffer, 0, buffer.Length);
                    // m_stream.Flush();

                    while ((currentBlockSize = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytesRead += currentBlockSize;

                        // currentPercent = ((double)totalBytesRead / (double)fileLength) * 100.0;
                        md5Offset += md5.TransformBlock(buffer, 0, currentBlockSize, buffer, 0);
                        
                        // if (sw.ElapsedMilliseconds - lastSample > _sampleDurationMs || currentBlockSize < buffer.Length)
                        // {
                        //     OnProgressChanged(currentPercent);
                        //     lastSample = sw.ElapsedMilliseconds;
                        // }
                    }

                    // OnProgressChanged(currentPercent);
                    // lastSample = sw.ElapsedMilliseconds;

                    // sw.Stop();
                    md5.TransformFinalBlock(new byte[] { }, 0, 0);

                    // this.MD5_Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                    md5Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();

                    // OnComplete(this.MD5_Hash);
                }

                return md5Hash;
            }

            private static string CleanDriveName(string driveName)
            {
                driveName = driveName.Trim('/');
                driveName = driveName.Trim('\\');
                driveName = driveName.ToUpper();

                if (!driveName.EndsWith(":"))
                    driveName += ":";

                return driveName;
            }

            /// <summary>
            ///     Converts the provided drive name (ex: "A:") to the raw path (ex: "\\.\A:")
            /// </summary>
            /// <param name="driveName">Name of the drive</param>
            /// <returns>Raw formatted drive path</returns>
            private static string GetOpticalDrivePath(string driveName)
            {
                if (driveName.StartsWith(@"\\.\"))
                    return driveName;
                    
                driveName = CleanDriveName(driveName);

                return @"\\.\" + driveName;
            }

            public static string[] GetOpticalDriveNames()
                => DriveInfo.GetDrives().Select(x => CleanDriveName(x.Name)).ToArray();
                
                
            public static void EjectDrive(string drive)
            {
                throw new NotImplementedException();
            }
        }
    }
}
