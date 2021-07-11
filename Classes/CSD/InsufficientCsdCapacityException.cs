using System;

namespace Archiver.Classes.CSD
{
    public class InsufficientCsdCapacityException : Exception
    {
        public InsufficientCsdCapacityException(string message) : base(message)
        { }
    }
}