using System;
using System.Runtime.InteropServices;

namespace Archiver.TapeServer.TapeDrivers
{
    public partial class NativeLinuxTapeDriver : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)] 
        public struct MagneticTapeOperation
        {
            public short Operation;	/* operations defined below */
	        public int	Count;	        /* how many of them */
        }


        private string _devicePath;
        private int _tapeHandle;



        public string DevicePath { get; set; }

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
        public NativeLinuxTapeDriver(string devicePath, Nullable<uint> blockSize, bool loadTape = true)
        {
            _devicePath = devicePath;

            _tapeHandle = Open(_devicePath, OPEN_READ_ONLY);

	    Console.WriteLine($"Tape Handle: {_tapeHandle}");

            if (_tapeHandle < 0)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
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

        private void PerformTapeOp(TapeOpType opType)
        {
            IntPtr ptr = IntPtr.Zero;

            try
            {
                MagneticTapeOperation op = new MagneticTapeOperation();
                op.Count = 1;
                op.Operation = (short)opType;

                int size = Marshal.SizeOf(op);
                ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(op, ptr, false);
                
                int result = Ioctl(_tapeHandle, MTIOCTOP, ptr);

                if (result < 0)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public void Eject()
        {
            PerformTapeOp(TapeOpType.MTOFFL);
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
