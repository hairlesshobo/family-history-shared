using System;
using System.Runtime.InteropServices;
using Archiver.Shared;
using Archiver.Shared.Interfaces;
using Archiver.TapeServer.Classes.Config;

namespace Archiver.TapeServer
{
    public class TapeOperator_Linux : IDisposable
    {
        static readonly int OPEN_READ_ONLY = 0;
        static readonly int OPEN_WRITE_ONLY = 1;
        static readonly int OPEN_READ_WRITE = 2;


        [DllImport("libc.so.6", EntryPoint = "open", SetLastError = true)]
        private static extern int Open(string fileName, int mode);
        
        [DllImport("libc.so.6", EntryPoint = "close", SetLastError = true)]
        private static extern int Close(int handle);



        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        private static extern int Read(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        private static extern int Write(int handle, byte[] data, int length);

        

        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        private extern static int Ioctl(int fd, int request, int data);

        

        private string _devicePath;
        private int _tapeHandle;



        public string DevicePath { get; set; }

        #region empty constructor overloads
        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public TapeOperator_Linux(string devicePath): this(devicePath, null, true) { }

        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public TapeOperator_Linux(string devicePath, bool loadTape): this(devicePath, null, loadTape) { }

        /// <summary>
        /// Loads tape with given name and block size. 
        /// </summary>
        public TapeOperator_Linux(string devicePath, int blockSize): this(devicePath, (uint)blockSize, true) { }   
        #endregion empty constructor overloads
        
        /// <summary>
        /// Loads tape with given name. 
        /// </summary>
        public TapeOperator_Linux(string devicePath, Nullable<uint> blockSize, bool loadTape = true)
        {
            _devicePath = devicePath;

            _tapeHandle = Open(_devicePath, OPEN_READ_ONLY);
        }

        /// <summary>
        /// Read one logical block from tape 
        /// starting on the given position
        /// </summary>
        /// <returns></returns>
        public bool Read(byte[] buffer, Nullable<long> startPosition)
        {
            bool endOfData = false;

            // if (startPosition.HasValue)
            //     SetTapeBlockPosition(startPosition.Value);
            
            // try
            // {
            //     // we empty the buffer before we read from tape
            //     Array.Fill(buffer, (byte)0);

            //     m_stream.Read(buffer, 0, buffer.Length);
            //     m_stream.Flush();
            // }
            // catch (IOException e)
            // {
            //     // filemark detected error code
            //     if (e.HResult == -2147023795)
            //         endOfData = true;   
            // }

            // byte[] buffer = new byte[1024];

            int response = Read(_tapeHandle, buffer, buffer.Length);

            int errorCode = -1;

            // a negative return value means an error occurred
            if (response < 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(errorCode);
            }

            Close(_tapeHandle);

            return endOfData;
        }

        public bool Read(byte[] buffer)
        {
            return this.Read(buffer, null);
        }

        /// <summary>
        /// Closes handle of the current tape
        /// </summary>
        public void Close()
        {
            // TODO: Check for errors on close
            if (_tapeHandle >= 0)
                Close(_tapeHandle);
        }
        
        public void Dispose()
            => this.Close();
    }
}
