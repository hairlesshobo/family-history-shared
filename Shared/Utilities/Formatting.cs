/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;

namespace FoxHollow.Archiver.Shared.Utilities
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
    }
}