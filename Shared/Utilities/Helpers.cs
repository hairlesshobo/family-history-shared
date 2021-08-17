using System;

namespace Archiver.Shared.Utilities
{
    public static class HelpersNew
    {
        public static long RoundToNextMultiple(long value, int multiple)
        {
            if (value == 0)
                return 0;
                
            long nearestMultiple = (long)Math.Round((value / (double)multiple), MidpointRounding.ToPositiveInfinity) * multiple;

            return nearestMultiple;
        }
    }
}