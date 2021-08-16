using System;

namespace Archiver.Shared.Exceptions
{
    public class TapeDriveNotOpenException : Exception
    {
        public TapeDriveNotOpenException(string methodName) : base($"Unable to perform {methodName} operation, the tape drive is not open")
        { }

        public TapeDriveNotOpenException() : base($"Unable to perform requested operation, the tape drive is not open")
        { }
    }
}