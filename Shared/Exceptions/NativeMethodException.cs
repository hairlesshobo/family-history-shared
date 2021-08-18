using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Archiver.Shared.Exceptions
{
    public class NativeMethodException : Exception
    {
        public NativeMethodException(string methodName)
            : base($"Native API method failed : call to {methodName} failed with error code {Marshal.GetLastWin32Error()}", new Win32Exception())
        { }
    }
}