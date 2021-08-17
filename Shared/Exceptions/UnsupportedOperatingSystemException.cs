using System;

namespace Archiver.Shared.Exceptions
{
    public class UnsupportedOperatingSystemException : Exception
    {
        public UnsupportedOperatingSystemException() : base("The current operating system is unsupported")
        { }
    }
}