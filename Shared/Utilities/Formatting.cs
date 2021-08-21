using System;

namespace Archiver.Shared.Utilities
{
    public static class Formatting
    {
        public enum LineType
        {
            Thin = '\u2500',
            Thick = '\u2501',
            ThinTripleDash = '\u2504',
            ThickTripleDash ='\u2505',
            Double = '\u2550',
            ThinDoubleDash = '\u254C',
            ThickDoubleDash ='\u254D',
        }

        [Obsolete]
        public static void DrawHorizontalLine(LineType lineType = LineType.Thin, int width = 0)
        {
            if (width <= 0)
                width = Console.WindowWidth;

            for (int i = 0; i < width; i++)
                Console.Write((char)lineType);
        }

        [Obsolete]
        public static void WriteLineSplit(string leftText, string rightText)
        {
            Console.CursorLeft = 0;

            Console.Write(leftText);

            Console.CursorLeft = Console.WindowWidth - rightText.Length;
            Console.Write(rightText);
            
            Console.CursorLeft = 0;
            // I don't need to drop down a line because writing all the way to the end of the line,
            // it seems to automatically drop down a line for me
            //Console.CursorTop += 1;
        }

        public static void WriteC(ConsoleColor color, char inputChar)
            => WriteC(color, new string(new char[] { inputChar }));

        public static void WriteC(ConsoleColor color, string inputString)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(inputString);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteLineC(ConsoleColor color, char inputChar)
            => WriteC(color, new string(new char[] { inputChar }));

        public static void WriteLineC(ConsoleColor color, string inputString)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(inputString);
            Console.ForegroundColor = originalColor;
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
        
        private static class BoxChars
        {
            public const char ThinTopLeft = '\u250C';
            public const char ThinBottomLeft = '\u2514';
            public const char ThinTopRight = '\u2510';
            public const char ThinBottomRight = '\u2518';
            public const char ThinVertical = '\u2502';

            public const char ThinHorizontal = '\u2500';
            public const char ThickHorizontal = '\u2501';
            public const char ThinDashHorizontal = '\u2504';
            public const char ThickDashHorizontal = '\u2505';
        }
    }

    
}