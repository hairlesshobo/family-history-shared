using System;
using Archiver.Classes;
using Archiver.Classes.Disc;

namespace Archiver.Utilities
{
    public static class Formatting
    {
        public static string GetDiscName(DiscDetail disc)
        {
            return $"Disc {disc.DiscNumber.ToString("0000")}";
        }
        
        public static string FormatElapsedTime(TimeSpan elapsed)
        {
            if (elapsed == default(TimeSpan))
                return "--:--:--";
            else
                return elapsed.ToString(@"hh\:mm\:ss");
        }

        public static string GetFriendlyTransferRate(double Rate)
        {
            return GetFriendlySize(Rate, true);
        }

        public static string GetFriendlySize(long Size)
        {
            return GetFriendlySize((double)Size);
        }

        private static string GetFriendlySize(double Size, bool IsRate = false)
        {
            string suffix = "B";

            if (Size > 1024)
            {
                suffix = "KiB";
                Size /= 1024.0;
            }

            if (Size > 1024)
            {
                suffix = "MiB";
                Size /= 1024.0;
            }

            if (Size > 1024)
            {
                suffix = "GiB";
                Size /= 1024.0;
            }

            if (Size > 1024)
            {
                suffix = "TiB";
                Size /= 1024.0;
            }

            string currentSizeFriendly = $"{Math.Round((double)Size, 1).ToString("0.0")} {suffix}";

            if (IsRate == true)
                currentSizeFriendly += "/s";

            return currentSizeFriendly;
        }
    }
}