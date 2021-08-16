using System;
using System.Runtime.InteropServices;

namespace Archiver.TapeServer.TapeDrivers
{
    public partial class NativeLinuxTapeDriver
    {
        // TODO: make this private
        [StructLayout(LayoutKind.Sequential)] 
        public struct MagneticTapeOperation
        {
            /// <Summary>
            /// Operation to be performed
            /// </Summary>
            public short Operation;	/* operations defined below */

            /// <Summary>
            /// How many times to perform this operation
            /// </Summary>
	        public int	Count;	        /* how many of them */
        }
    }
}