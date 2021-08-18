using System;

namespace Archiver.Shared.Native
{
    public static partial class Linux
    {
        public enum OpenType
        {
            ReadOnly = 0,
            WriteOnly = 1,
            ReadWrite = 2
        }
    }
}