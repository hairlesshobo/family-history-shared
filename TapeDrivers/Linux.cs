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
using System.Runtime.InteropServices;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Native;
using static FoxHollow.FHM.Shared.Native.Linux;

namespace FoxHollow.FHM.Shared.TapeDrivers
{
    public partial class NativeLinuxTapeDriver : IDisposable
    {
        private string _devicePath = null;
        private Nullable<int> _tapeHandle = null;
        private Nullable<OpenType> _access = null;
        private uint _blockSize = 0;

        public string DevicePath => _devicePath;
        public uint BlockSize => _blockSize;
        public bool IsOpen => _tapeHandle.HasValue;

        #region empty constructor overloads
        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public NativeLinuxTapeDriver(string devicePath): this(devicePath, null, true) { }

        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public NativeLinuxTapeDriver(string devicePath, bool loadTape): this(devicePath, null, loadTape) { }

        /// <summary>
        /// Loads tape with given name and block size. 
        /// </summary>
        public NativeLinuxTapeDriver(string devicePath, int blockSize): this(devicePath, (uint)blockSize, true) { }   
        #endregion empty constructor overloads
        
        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public NativeLinuxTapeDriver(string devicePath, Nullable<uint> blockSize, bool autoOpen = false, bool loadTape = true)
        {
            _tapeHandle = null;
            _devicePath = devicePath;

            if (autoOpen)
                Open();
        }

        /// <summary>
        ///     Opens a new handle to the current tape drive
        /// </summary>
        public void Open(OpenType openType = OpenType.ReadOnly)
        {
            if (!this.IsOpen)
            {
                _tapeHandle = Linux.Open(_devicePath, openType);
                _access = openType;

                if (_tapeHandle < 0)
                    throw new NativeMethodException("Open");
            }
        }

        /// <summary>
        ///     Closes handle of the current tape drive
        /// </summary>
        public void Close()
        {
            if (this.IsOpen)
            {
                int result = Linux.Close(_tapeHandle.Value);

                if (result < 0)
                    throw new NativeMethodException("Close");

                _tapeHandle = null;
            }
        }

        /// <summary>
        /// Writes to the tape given stream starting from given postion
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="stream"></param>
        public void Write(byte[] stream)
        {
            throw new NotImplementedException();
            // if (stream.Length != BlockSize)
            //     throw new InvalidOperationException("The bytes to write must be equal to the block size");

            // // Write data to the device
            // m_stream.Write( stream, 0, stream.Length );
            // m_stream.Flush();
        }

        /// <summary>
        ///     Read one logical block from tape 
        /// </summary>
        /// <returns>boolean value indicating whether the end of the current file has been reached</returns>
        public bool Read(byte[] buffer)
        {
            if (!this.IsOpen)
                throw new TapeDriveNotOpenException("Read");
                
            bool endOfData = false;

            try
            {
                // we empty the buffer before we read from tape
                Array.Fill(buffer, (byte)0);

                int result = Linux.Read(_tapeHandle.Value, buffer, buffer.Length);

                if (result == 0)
                    endOfData = true;

                // a negative return value means an error occurred
                if (result < 0)
                    throw new NativeMethodException("Read");

            }
            catch (Exception e)
            {
                throw new TapeDriveOperationException("Read", e);
            }

            return endOfData;
        }

        /// <Summary>
        ///     Eject the tape that is currently in the drive
        /// <Summary>
        public void Eject()
            => PerformTapeOp("Eject", TapeOpType.MTOFFL);

        /// <Summary>
        ///     Rewind the tape that is currently in the drive
        /// <Summary>
        public void Rewind()
            => PerformTapeOp("Rewind", TapeOpType.MTREW);

        /// <summary>
        ///     Sets new tape position (current seek)
        /// </summary>
        /// <param name="fileNumber">File number on tape to seek to</param>
        public void SetTapeFilePosition(int fileNumber)
        {
            // TODO: Test
            Rewind();

            if (fileNumber > 0)
                PerformTapeOp("SetTapeFilePosition", TapeOpType.MTFSF, fileNumber);

            // int errorCode = 0;

            // // TODO: reapit it
            // if ((errorCode = SetTapePosition(m_handleValue, TAPE_SPACE_FILEMARKS, 0, (int)fileNumber, 0, FALSE)) != NO_ERROR)
            //     throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Sets new tape position to end of data
        /// </summary>
        public void SetTapeToEndOfData()
        {
            throw new NotImplementedException();
            // int errorCode = 0;

            // if ((errorCode = SetTapePosition(m_handleValue, TAPE_SPACE_END_OF_DATA, 0, 0, 0, FALSE)) != NO_ERROR)
            //     throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Sets new tape position (current seek)
        /// </summary>
        /// <param name="logicalBlock">Block number of current file on tape to seek to</param>
        public void SetTapeBlockPosition(long logicalBlock)
        {
            throw new NotImplementedException();
            // int errorCode = 0;

            // // TODO: reapit it
            // if ((errorCode = SetTapePosition(m_handleValue, TAPE_LOGICAL_BLOCK, 0, (int)logicalBlock, 0, FALSE)) != NO_ERROR)
            //     throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        /// <Summary>
        ///     Write a new filemark to the current position on the tape.
        ///     NOTE: on Linux, this operation closes the tape drive handle. Be sure to call Open()
        ///     again prior to subsequent Write() calls.
        /// </Summary>
        public void WriteFilemark()
        {
            // TODO: Test
            // supposedly, in linux, the driver automatically writes a filemark upon close when 
            // the last command was a write. In theory, that just means all we need to do here is
            // close the tape handle
            if (this.IsOpen)
                Close();

            // int errorCode = 0;

            // if ((errorCode = WriteTapemark(m_handleValue, TAPE_FILEMARKS, 1, FALSE)) != NO_ERROR)
            //     throw new TapeOperatorWin32Exception("WriteTapemark", Marshal.GetLastWin32Error());
        }

        public void SetDriveCompression(bool enabled = true)
        {
            throw new NotImplementedException();
            // TapeDriveInfo info = this.DriveInfo;

            // TapeSetDriveParameters parameters = new TapeSetDriveParameters()
            // {
            //     Compression = 1,
            //     DataPadding = info.DataPadding,
            //     ECC = info.ECC,
            //     EOTWarningZoneSize = info.EOTWarningZone,
            //     ReportSetMarks = info.ReportSetMarks
            // };

            // int errorCode = SetTapeParameters(m_handleValue, SET_TAPE_DRIVE_INFORMATION, parameters);

            // if (errorCode != NO_ERROR)
            //     throw new TapeOperatorWin32Exception("SetTapeParameters", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Returns Current tape's postion ( seek )
        /// </summary>
        /// <returns></returns>
        public long GetTapeBlockPosition()
        {
            throw new NotImplementedException();
            // int partition;
            // int offsetLow;
            // int offsetHigh;

            // if (GetTapePosition(m_handleValue, TAPE_LOGICAL_POSITION, out partition, out offsetLow, out offsetHigh) != NO_ERROR)
            //     throw new TapeOperatorWin32Exception("GetTapePosition", Marshal.GetLastWin32Error());

            // long offset = ( long )( offsetHigh * Math.Pow( 2, 32 ) + offsetLow );

            // return offset;
        }


        
        public void Dispose()
            => this.Close();

        private void PerformTapeOp(string callingMethodName, TapeOpType opType, int repeat_count = 1)
        {
            if (!this.IsOpen)
                throw new TapeDriveNotOpenException(callingMethodName);

            IntPtr ptr = IntPtr.Zero;

            try
            {
                MagneticTapeOperation op = new MagneticTapeOperation();
                op.Count = repeat_count;
                op.Operation = (short)opType;

                int size = Marshal.SizeOf(op);
                ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(op, ptr, false);
                
                int result = Linux.Ioctl(_tapeHandle.Value, MTIOCTOP, ptr);

                if (result < 0)
                    throw new NativeMethodException(callingMethodName);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
