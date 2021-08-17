using System;

namespace Archiver.Shared.Exceptions
{
    public class CsdInsufficientCapacityException : Exception
    {
        public CsdInsufficientCapacityException(string message) : base(message)
        { }
    }
}