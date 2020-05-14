using System;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

using System.Collections.Generic;
using System.Text;
//using Bsw.Types.Logger; 

namespace Archiver.Utilities
{
    #region Typedefenitions
    using BOOL = System.Int32;
    #endregion

    #region Types
    [StructLayout(LayoutKind.Sequential)] 
    public struct MediaInfo
    {
        public long Capacity;
        public long Remaining;

        public uint BlockSize;
        public uint PartitionCount;

        public byte IsWriteProtected;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct TapeDriveInfo
    {
        public byte ECC;
        public byte Compression;
        public byte DataPadding;
        public byte ReportSetMarks;

        public uint DefaultBlockSize;
        public uint MaximumBlockSize;
        public uint MinimumBlockSize;
        public uint PartitionCount;

        public uint FeaturesLow;
        public uint FeaturesHigh;
        public uint EATWarningZone;
    }
    #endregion

    /// <summary>
    /// Low level Tape operator
    /// </summary>
    public class TapeOperator : IDisposable
    {
        #region Private constants
        private const short FILE_ATTRIBUTE_NORMAL = 0x80;
        private const short INVALID_HANDLE_VALUE = -1;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint CREATE_NEW = 1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        private const uint NO_ERROR = 0;
        private const int TAPE_LOAD = 0;
        private const int TAPE_UNLOAD = 1;

        private const int TAPE_SETMARKS = 0;
        private const int TAPE_FILEMARKS = 1;
        private const int TAPE_SHORT_FILEMARKS = 2;
        private const int TAPE_LONG_FILEMARKS = 3;



        private const int TAPE_REWIND = 0;
        private const int TAPE_LOGICAL_POSITION = 1;
        private const int TAPE_LOGICAL_BLOCK = 2;
        private const int TAPE_SPACE_END_OF_DATA = 4;
        private const int TAPE_RELATIVE_BLOCKS = 5;
        private const int TAPE_SPACE_FILEMARKS = 6;
        private const int TAPE_SPACE_SEQUENTIAL_FMKS = 7;
        private const int TAPE_SPACE_SETMARKS = 8;
        private const int TAPE_SPACE_SEQUENTIAL_SMKS = 9;

        private const int FALSE = 0;
        private const int TRUE = 0;

        private const int MEDIA_PARAMS = 0;
        private const int DRIVE_PARAMS = 1;
        #endregion

        #region PInvoke
        // Use interop to call the CreateFile function.
        // For more information about CreateFile,
        // see the unmanaged MSDN reference library.
        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int PrepareTape(
            SafeFileHandle handle,
            int prepareType,
            BOOL isImmediate
            );


        [DllImport( "kernel32", SetLastError = true )]
        private static extern int SetTapePosition(
            SafeFileHandle handle,
            int positionType,
            int partition,
            int offsetLow,
            int offsetHigh,
            BOOL isImmediate
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int GetTapePosition(
            SafeFileHandle handle,
            int positionType,
            out int partition,
            out int offsetLow,
            out int offsetHigh
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int WriteTapemark(
            SafeFileHandle handle,
            int tapemarkType,
            int tapemarkCount,
            BOOL isImmediate
            );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int GetTapeParameters(
           SafeFileHandle handle,
           int operationType,
           ref int size,
           IntPtr mediaInfo
           );

        [DllImport( "kernel32", SetLastError = true )]
        private static extern int GetLastError();
        #endregion

        #region Private variables
        private FileStream m_stream;
        private SafeFileHandle m_handleValue = null;

        private Nullable<TapeDriveInfo> m_driveInfo = null;
        private Nullable<MediaInfo> m_mediaInfo = null;
        private uint _blockSize = 0;
        #endregion

        #region Public methods

        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public TapeOperator(string tapeName): this(tapeName, null)
        {
        }

        /// <summary>
        /// Loads tape with given name and block size. 
        /// </summary>
        public TapeOperator(string tapeName, int blockSize): this(tapeName, (uint)blockSize)
        {
        }   
        
        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public TapeOperator(string tapeName, Nullable<uint> blockSize)
        {            
            // Try to open the file.
            m_handleValue = CreateFile(
                tapeName,
                GENERIC_READ | GENERIC_WRITE,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_ARCHIVE | FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero
                );

            if (m_handleValue.IsInvalid)
                throw new TapeOperatorWin32Exception("CreateFile", Marshal.GetLastWin32Error());

            // Load the tape
            int result = PrepareTape(m_handleValue, TAPE_LOAD, TRUE);

            if ( result != NO_ERROR )
                throw new TapeOperatorWin32Exception("PrepareTape", Marshal.GetLastWin32Error());

            if (!blockSize.HasValue)
                blockSize = this.DriveInfo.DefaultBlockSize;

            if (blockSize < this.DriveInfo.MinimumBlockSize)
                throw new TapeException($"Specified block size '{blockSize}' is smaller than the minimum size '{this.DriveInfo.MinimumBlockSize}' that this drive supports");

            if (blockSize > this.DriveInfo.MaximumBlockSize)
                throw new TapeException($"Specified block size '{blockSize}' is larger than the maximum size '{this.DriveInfo.MaximumBlockSize}' that this drive supports");

            _blockSize = (uint)blockSize;

            m_stream = new FileStream(m_handleValue, FileAccess.ReadWrite, (int)_blockSize, false);
        }

        public void RewindTape()
        {
            SetTapeBlockPosition(0);
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
            m_stream.Write( stream, 0, stream.Length );
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
        {
            return this.Read(buffer, null);
        }
        
        /// <summary>
        /// Closes handler of the current tape
        /// </summary>
        public void Close()
        {
            if ( m_handleValue != null && !m_handleValue.IsInvalid && !m_handleValue.IsClosed )
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
            if ((errorCode = SetTapePosition(m_handleValue, TAPE_SPACE_FILEMARKS, 0, (int)fileNumber, 0, FALSE)) != NO_ERROR)
                throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Sets new tape position to end of data
        /// </summary>
        public void SetTapeToEndOfData()
        {
            int errorCode = 0;

            if ((errorCode = SetTapePosition(m_handleValue, TAPE_SPACE_END_OF_DATA, 0, 0, 0, FALSE)) != NO_ERROR)
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
            if ((errorCode = SetTapePosition(m_handleValue, TAPE_LOGICAL_BLOCK, 0, (int)logicalBlock, 0, FALSE)) != NO_ERROR)
                throw new TapeOperatorWin32Exception("SetTapePosition", Marshal.GetLastWin32Error());
        }

        public void WriteFilemark()
        {
            int errorCode = 0;

            if ((errorCode = WriteTapemark(m_handleValue, TAPE_FILEMARKS, 1, FALSE)) != NO_ERROR)
                throw new TapeOperatorWin32Exception("WriteTapemark", Marshal.GetLastWin32Error());
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

            if (GetTapePosition(m_handleValue, TAPE_LOGICAL_POSITION, out partition, out offsetLow, out offsetHigh) != NO_ERROR)
                throw new TapeOperatorWin32Exception("GetTapePosition", Marshal.GetLastWin32Error());

            long offset = ( long )( offsetHigh * Math.Pow( 2, 32 ) + offsetLow );

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
                if ( !m_handleValue.IsInvalid )
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

        public MediaInfo MediaInfo
        {
            get
            {
                GetMediaInfo();

                return m_mediaInfo.Value;
            }
        }
        #endregion

        #region Private methods
        
        /// <summary>
        /// Returns minum number of blocks that can contain
        /// given number of bytes
        /// </summary>
        private uint GetBlocksNumber(long bytes)
        {
            uint numberOfBlocks = ( uint )bytes / BlockSize;
            uint bytesInLastBlock = ( uint )bytes % BlockSize;

            // Calculate number of blocks
            if ( bytesInLastBlock > 0 ) numberOfBlocks++;

            return numberOfBlocks;
        }

        private void GetDriveInfo()
        {
            if ( !m_driveInfo.HasValue )
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    m_driveInfo = new TapeDriveInfo();

                    // Allocate unmanaged memory
                    int size = Marshal.SizeOf(m_driveInfo);
                    ptr = Marshal.AllocHGlobal(size);

                    Marshal.StructureToPtr(m_driveInfo, ptr, false);

                    
                    int result = 0;
                    if ((result = GetTapeParameters(m_handleValue, DRIVE_PARAMS, ref size, ptr)) != NO_ERROR)
                        throw new TapeOperatorWin32Exception("GetTapeParameters", Marshal.GetLastWin32Error());

                    // Get managed media Info
                    m_driveInfo = (TapeDriveInfo)Marshal.PtrToStructure(ptr, typeof(TapeDriveInfo));
                }
                finally
                {
                    if ( ptr != IntPtr.Zero )
                        Marshal.FreeHGlobal( ptr );
                }
            } 
        }

        private void GetMediaInfo()
        {
            if ( !m_mediaInfo.HasValue )
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    m_mediaInfo = new MediaInfo();

                    // Allocate unmanaged memory
                    int size = Marshal.SizeOf( m_mediaInfo );
                    ptr = Marshal.AllocHGlobal( size );

                    Marshal.StructureToPtr(
                        m_mediaInfo,
                        ptr,
                        false
                    );

                    
                    int result = 0;
                    if ((result = GetTapeParameters(m_handleValue, MEDIA_PARAMS, ref size, ptr)) != NO_ERROR)
                        throw new TapeOperatorWin32Exception("GetTapeParameters", Marshal.GetLastWin32Error());

                    // Get managed media Info
                    m_mediaInfo = (MediaInfo)Marshal.PtrToStructure(ptr, typeof(MediaInfo));
                }
                finally
                {
                    if ( ptr != IntPtr.Zero )
                    {
                        Marshal.FreeHGlobal( ptr );
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
        public TapeOperatorWin32Exception( string methodName, int win32ErroCode ):
            base( string.Format(
                "WIN32 API method failed : {0} failed with error code {1}",
                methodName,
                win32ErroCode
            ) ){}
    }

    public class TapeException : Exception
    {
        public TapeException(string message): base(message)
        {}
    }

}
