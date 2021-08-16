using System;

namespace Archiver.Shared.Exceptions
{
    public class TapeDriveOperationException : Exception
    {
        public TapeDriveOperationException(string methodName, Exception innerException)
            : base($"Tape drive operation {methodName} failed. See inner exception", innerException)
        { }
    }
}