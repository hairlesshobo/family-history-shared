using System;

namespace Archiver.Shared.Exceptions
{
    public class TapeDriveNativeException : System.ComponentModel.Win32Exception
    {
        public TapeDriveNativeException(int errorCode) 
            : base(errorCode)
        { }

        public TapeDriveNativeException(string methodName, int errorCode) 
            : base(errorCode, $"Native API method failed : call to {methodName} failed with error code {errorCode}")
        { }
    }
}