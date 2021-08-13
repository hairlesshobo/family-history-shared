using System;

namespace Archiver.Shared.Exceptions
{
    public class TapeDriveNotFoundException : Exception
    {
        public TapeDriveNotFoundException(string drive) : base($"Unable to find the following tape drive: {drive}")
        {

        }
    }
}