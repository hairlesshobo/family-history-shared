using System;

namespace DiscArchiver.Utilities
{
    public static class StatusHelpers
    {
        public static void WriteStatusLineWithPct(string left, string right, double percent, bool complete)
        {
            WriteStatusLineWithPct(left, right, percent, complete, Console.ForegroundColor);
        }

        public static void WriteStatusLineWithPct(string left, string right, double percent, bool complete, ConsoleColor color)
        {
            string line = right;

            if (!line.EndsWith(" "))
                line += " ";

            int leftWidth = line.Length + 1;

            if (left != null)
                leftWidth += Globals._leftHeaderWidth + 2;

            line += GeneratePercentBar(Console.WindowWidth, leftWidth, 0, percent, complete);

            WriteStatusLine(left, line, color);
        }

        public static void WriteStatusLine(string left, string right)
        {
            WriteStatusLine(left, right, Console.ForegroundColor);
        }

        public static void WriteStatusLine(string left, string right, ConsoleColor rightColor)
        {
            int padRight = Console.WindowWidth - Globals._leftHeaderWidth - 2 - 1;

            if (left != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"{left.PadLeft(Globals._leftHeaderWidth)}: ");
                Console.ResetColor();
            }

            Console.ForegroundColor = rightColor;
            Console.Write($"{right.PadRight(padRight)}");
            Console.ResetColor();
        }

        public static string GeneratePercentBar (int AvailableSpace, int LeftLength, int RightLength, double CurrentPercent, bool Complete)
        {
            string percentLeft = "[";
            string percentRight = "] " + Math.Round(CurrentPercent, 0).ToString().PadLeft(3) + "%";

            int totalSegments = AvailableSpace - LeftLength - RightLength - percentRight.Length - percentLeft.Length - 1;
            int completeSegments = (int)Math.Floor(totalSegments * (CurrentPercent/100.0));

            string progressBar = "";

            for (int i = 0; i < completeSegments; i++)
                progressBar += "=";

            if (Complete == false)
                progressBar += ">";

            string line = percentLeft + progressBar.PadRight(totalSegments) + percentRight;

            return line;
        }
    }
}