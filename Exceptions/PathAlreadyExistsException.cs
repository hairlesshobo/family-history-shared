using System;

namespace FoxHollow.FHM.Shared.Exceptions
{
    public class PathAlreadyExistsException : Exception
    {
        public PathAlreadyExistsException(string path) : base($"Path already exists: {path}")
        { }
    }
}