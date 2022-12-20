using System;

namespace FoxHollow.FHM.Shared.Interop
{
    public class UnknownCommandException : Exception
    {
        public UnknownCommandException(string command) : base($"Uknown interop command specified: {command}")
        { }
    }
}