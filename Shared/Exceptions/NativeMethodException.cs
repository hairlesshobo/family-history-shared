using System;

namespace Archiver.Shared.Exceptions
{
    public class NativeMethodException : System.ComponentModel.Win32Exception
    {
        public NativeMethodException(int errorCode) 
            : base(errorCode)
        { }

        public NativeMethodException(string methodName, int errorCode) 
            : base(errorCode, $"Native API method failed : call to {methodName} failed with error code {errorCode}")
        { }
    }
}