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
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

using System.Collections.Generic;
using System.Text;
using FoxHollow.Archiver.Shared.Classes.Tape;
using FoxHollow.Archiver.Shared.Native;

using static FoxHollow.Archiver.Shared.Native.Windows;
//using Bsw.Types.Logger; 

namespace FoxHollow.Archiver.Shared.TapeDrivers
{
    /// <summary>
    /// Low level Tape operator
    /// </summary>
    public partial class NativeWindowsTapeDriver : IDisposable
    {
        #region Private variables
        private FileStream m_stream;
        private SafeFileHandle m_handleValue = null;

        private Nullable<TapeDriveInfo> m_driveInfo = null;
        private Nullable<TapeMediaInfo> m_mediaInfo = null;
        private uint _blockSize = 0;
        #endregion

        #region Public methods

        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public NativeWindowsTapeDriver(string tapeName) : this(tapeName, null, true)
        {
        }

        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public NativeWindowsTapeDriver(string tapeName, bool loadTape) : this(tapeName, null, loadTape)
        {
        }

        /// <summary>
        /// Loads tape with given name and block size. 
        /// </summary>
        public NativeWindowsTapeDriver(string tapeName, int blockSize) : this(tapeName, (uint)blockSize, true)
        {
        }

        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public NativeWindowsTapeDriver(string tapeName, Nullable<uint> blockSize, bool loadTape = true)
        {
            // Try to open the file.
            m_handleValue = Windows.CreateFile(
                tapeName,
                GENERIC_READ | GENERIC_WRITE,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                //FILE_ATTRIBUTE_ARCHIVE | FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero
                );

            if (m_handleValue.IsInvalid)
                throw new TapeOperatorWin32Exception("CreateFile", Marshal.GetLastWin32Error());

            // lets check to see if the tape has changed, if it did
            // then we have to force a load of the tape
            try
            {
                GetDriveInfo();
            }
            catch (TapeChangedException)
            {
                loadTape = true;
            }

            // Load the tape
            if (loadTape)
            {
                int result = Windows.PrepareTape(m_handleValue, TAPE_LOAD, TRUE);

                if (result != NO_ERROR)
                    throw new TapeOperatorWin32Exception("PrepareTape", Marshal.GetLastWin32Error());
            }

            if (!blockSize.HasValue)
                blockSize = this.DriveInfo.DefaultBlockSize;

            if (blockSize < this.DriveInfo.MinimumBlockSize)
                throw new TapeException($"Specified block size '{blockSize}' is smaller than the minimum size '{this.DriveInfo.MinimumBlockSize}' that this drive supports");

            if (blockSize > this.DriveInfo.MaximumBlockSize)
                throw new TapeException($"Specified block size '{blockSize}' is larger than the maximum size '{this.DriveInfo.MaximumBlockSize}' that this drive supports");

            _blockSize = (uint)blockSize;

            m_stream = new FileStream(m_handleValue, FileAccess.ReadWrite, (int)_blockSize, false);
        }

        public bool TapeLoaded()
        {
            int result = 0;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                var tmpMediaInfo = new TapeMediaInfo();

                // Allocate unmanaged memory
                int size = Marshal.SizeOf(tmpMediaInfo);
                ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(
                    tmpMediaInfo,
                    ptr,
                    false
                );

                result = Windows.GetTapeParameters(m_handleValue, MEDIA_PARAMS, ref size, ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }

            if (result == 1112)
                return false;
            else
                return true;
        }

        public void RewindTape()
            => SetTapeBlockPosition(0);

        public void EjectTape()
        {
            int result = Windows.PrepareTape(m_handleValue, TAPE_UNLOAD, FALSE);

            if (result != NO_ERROR)
                throw new TapeOperatorWin32Exception("PrepareTape", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Writes to the tape given stream starting from given postion
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="stream"></param>
        public void Write(byte[] stream)
        {
            if (stream.Length != BlockSize)
                throw new InvalidOperationException("The bytes to write must be equal to the block size");

            // Write data to the device
            m_stream.Write(stream, 0, stream.Length);
            m_stream.Flush();
        }

        /// <summary>
        /// Read one logical block from tape 
        /// starting on the given position
        /// </summary>
        /// <returns></returns>
        public bool Read(byte[] buffer, Nullable<long> startPosition)
        {
            bool endOfData = false;

            if (startPosition.HasValue)
                SetTapeBlockPosition(startPosition.Value);

            try
            {
                // we empty the buffer before we read from tape
                Array.Fill(buffer, (byte)0);

                m_stream.Read(buffer, 0, buffer.Length);
                m_stream.Flush();
            }
            catch (IOException e)
            {
                // filemark detected error code
                if (e.HResult == -2147023795)
                    endOfData = true;
            }

            return endOfData;
        }

        public bool Read(byte[] buffer)
            => this.Read(buffer, null);

        /// <summary>
        /// Closes handler of the current tape
        /// </summary>
        public void Close()
        {
            if (m_handleValue != null && !m_handleValue.IsInvalid && !m_handleValue.IsClosed)
                m_handleValue.Close();
        }

        /// <summary>
        /// Sets new tape position (current seek)
        /// </summary>
        /// <param name="logicalBlock">Block number of current file on tape to seek to</param>
        public void SetTapeFilePosition(long fileNumber)
        {
            int errorCode = 0;

            // TODO: reapit it
            if ((errorCode = Windows.SetTapePosition(m_handleValue, TAPE_SPACE_FILEMARKS, 0, (int)fileNumber, 0, FALSE)) != NO_ERROR)
                throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Sets new tape position to end of data
        /// </summary>
        public void SetTapeToEndOfData()
        {
            int errorCode = 0;

            if ((errorCode = Windows.SetTapePosition(m_handleValue, TAPE_SPACE_END_OF_DATA, 0, 0, 0, FALSE)) != NO_ERROR)
                throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Sets new tape position (current seek)
        /// </summary>
        /// <param name="logicalBlock">Block number of current file on tape to seek to</param>
        public void SetTapeBlockPosition(long logicalBlock)
        {
            int errorCode = 0;

            // TODO: reapit it
            if ((errorCode = Windows.SetTapePosition(m_handleValue, TAPE_LOGICAL_BLOCK, 0, (int)logicalBlock, 0, FALSE)) != NO_ERROR)
                throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        public void WriteFilemark()
        {
            int errorCode = 0;

            if ((errorCode = Windows.WriteTapemark(m_handleValue, TAPE_FILEMARKS, 1, FALSE)) != NO_ERROR)
                throw new TapeOperatorWin32Exception("WriteTapemark", Marshal.GetLastWin32Error());
        }

        public void SetDriveCompression()
        {
            TapeDriveInfo info = this.DriveInfo;

            TapeSetDriveParameters parameters = new TapeSetDriveParameters()
            {
                Compression = 1,
                DataPadding = info.DataPadding,
                ECC = info.ECC,
                EOTWarningZoneSize = info.EOTWarningZone,
                ReportSetMarks = info.ReportSetMarks
            };

            int errorCode = Windows.SetTapeParameters(m_handleValue, SET_TAPE_DRIVE_INFORMATION, parameters);

            if (errorCode != NO_ERROR)
                throw new TapeOperatorWin32Exception("SetTapeParameters", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Returns Current tape's postion ( seek )
        /// </summary>
        /// <returns></returns>
        public long GetTapeBlockPosition()
        {
            int partition;
            int offsetLow;
            int offsetHigh;

            if (Windows.GetTapePosition(m_handleValue, TAPE_LOGICAL_POSITION, out partition, out offsetLow, out offsetHigh) != NO_ERROR)
                throw new TapeOperatorWin32Exception("GetTapePosition", Marshal.GetLastWin32Error());

            long offset = (long)(offsetHigh * Math.Pow(2, 32) + offsetLow);

            return offset;
        }
        #endregion

        #region Public properties

        public FileStream Stream
        {
            get
            {
                return m_stream;
            }
        }

        /// <summary>
        /// Returns opened file handle
        /// </summary>
        public SafeFileHandle Handle
        {
            get
            {
                // If the handle is valid,
                // return it.
                if (!m_handleValue.IsInvalid)
                {
                    return m_handleValue;
                }
                else
                {
                    return null;
                }
            }// GET
        }

        /// <summary>
        /// Returns default block size for current device
        /// </summary>
        public uint DefaultBlockSize
        {
            get
            {
                GetDriveInfo();

                return this.m_driveInfo.Value.DefaultBlockSize;
            }
        }

        public uint BlockSize
        {
            get
            {
                return _blockSize;
            }
        }

        public TapeDriveInfo DriveInfo
        {
            get
            {
                GetDriveInfo();

                return m_driveInfo.Value;
            }
        }

        public TapeMediaInfo MediaInfo
        {
            get
            {
                GetMediaInfo();

                return m_mediaInfo.Value;
            }
        }
        #endregion

        #region Private methods

        private void GetDriveInfo()
        {
            if (!m_driveInfo.HasValue)
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    m_driveInfo = new TapeDriveInfo();

                    // Allocate unmanaged memory
                    int size = Marshal.SizeOf(m_driveInfo);
                    ptr = Marshal.AllocHGlobal(size);

                    Marshal.StructureToPtr(m_driveInfo, ptr, false);

                    int result = Windows.GetTapeParameters(m_handleValue, DRIVE_PARAMS, ref size, ptr);
                    if (result != NO_ERROR)
                    {
                        if (result == 1110)
                        {
                            m_driveInfo = null;
                            throw new TapeChangedException();
                        }
                        else
                            throw new TapeOperatorWin32Exception("GetTapeParameters", Marshal.GetLastWin32Error());
                    }

                    // Get managed media Info
                    m_driveInfo = (TapeDriveInfo)Marshal.PtrToStructure(ptr, typeof(TapeDriveInfo));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptr);
                }
            }
        }

        private void GetMediaInfo()
        {
            if (!m_mediaInfo.HasValue)
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    m_mediaInfo = new TapeMediaInfo();

                    // Allocate unmanaged memory
                    int size = Marshal.SizeOf(m_mediaInfo);
                    ptr = Marshal.AllocHGlobal(size);

                    Marshal.StructureToPtr(
                        m_mediaInfo,
                        ptr,
                        false
                    );


                    int result = 0;
                    if ((result = Windows.GetTapeParameters(m_handleValue, MEDIA_PARAMS, ref size, ptr)) != NO_ERROR)
                        throw new TapeOperatorWin32Exception("GetTapeParameters", Marshal.GetLastWin32Error());

                    // Get managed media Info
                    m_mediaInfo = (TapeMediaInfo)Marshal.PtrToStructure(ptr, typeof(TapeMediaInfo));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Close();
        }
        #endregion
    }

    /// <summary>
    /// Exception that will be thrown by tape
    /// operator when one of WIN32 APIs terminates 
    /// with error code 
    /// </summary>
    public class TapeOperatorWin32Exception : ApplicationException
    {
        public TapeOperatorWin32Exception(string methodName, int win32ErroCode) :
            base(string.Format(
                "WIN32 API method failed : {0} failed with error code {1}",
                methodName,
                win32ErroCode
            ))
        { }
    }

    public class TapeChangedException : Exception
    {
        public TapeChangedException() : base()
        {

        }
    }

    public class TapeException : Exception
    {
        public TapeException(string message) : base(message)
        { }
    }

}
