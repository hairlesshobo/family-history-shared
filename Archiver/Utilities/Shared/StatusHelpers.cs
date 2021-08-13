using System;

namespace Archiver.Utilities.Shared
{
    public static class StatusHelpers
    {
        public static void WriteStatusLineWithPct(string left, string right, double percent, bool complete)
            => WriteStatusLineWithPct(left, right, percent, complete, Console.ForegroundColor);

        public static void WriteStatusLineWithPct(string left, string right, double percent, bool complete, bool increasing)
            => WriteStatusLineWithPct(left, right, percent, complete, Console.ForegroundColor, increasing);

        public static void WriteStatusLineWithPct(string left, string right, double percent, bool complete, ConsoleColor color)
            => WriteStatusLineWithPct(left, right, percent, complete, color, true);

        public static void WriteStatusLineWithPct(string left, string right, double percent, bool complete, ConsoleColor color, bool increasing)
        {
            string line = right;

            if (!line.EndsWith(" "))
                line += " ";

            int leftWidth = line.Length + 1;

            if (left != null)
                leftWidth += Globals._leftHeaderWidth + 2;

            line += GeneratePercentBar(Console.WindowWidth, leftWidth, 0, percent, complete, increasing);

            WriteStatusLine(left, line, color);
        }

        public static void ClearLine(int lineNum)
        {
            Console.SetCursorPosition(0, lineNum);

            ClearLine();
        }

        public static void ClearLine()
        {
            Console.CursorLeft = 0;
            Console.Write("".PadRight(Console.BufferWidth-2));
            Console.CursorLeft = 0;
        }

        public static void WriteStatusLine(string left, string right)
        {
            WriteStatusLine(left, right, Console.ForegroundColor);
        }

        public static void WriteStatusLine(string left, string right, ConsoleColor rightColor)
        {
            int padRight = Console.WindowWidth - Globals._leftHeaderWidth - 2 - 1;

            if (left != null)
                Formatting.WriteC(ConsoleColor.DarkCyan, $"{left.PadLeft(Globals._leftHeaderWidth)}: ");

            Formatting.WriteC(rightColor, $"{right.PadRight(padRight)}");
        }

        public static string GeneratePercentBar (int AvailableSpace, int LeftLength, int RightLength, double CurrentPercent, bool Complete, bool Increasing)
        {
            return GeneratePercentBar(AvailableSpace, LeftLength, RightLength, CurrentPercent, Complete, Increasing, false);
        }

        public static string GeneratePercentBar (int AvailableSpace, int LeftLength, int RightLength, double CurrentPercent, bool Complete)
        {
            return GeneratePercentBar(AvailableSpace, LeftLength, RightLength, CurrentPercent, Complete, true, false);
        }
        
        public static string GeneratePercentBar (int AvailableSpace, double CurrentPercent, bool Complete, bool Increasing, bool BarOnly)
        {
            return GeneratePercentBar(AvailableSpace, 0, 0, CurrentPercent, Complete, Increasing, BarOnly);
        }

        public static string GeneratePercentBar (int AvailableSpace, double CurrentPercent, bool Complete, bool Increasing)
        {
            return GeneratePercentBar(AvailableSpace, 0, 0, CurrentPercent, Complete, Increasing, false);
        }


        public static string GeneratePercentBar (int AvailableSpace, int LeftLength, int RightLength, double CurrentPercent, bool Complete, bool Increasing, bool BarOnly)
        {
            string percentLeft = "[";
            string percentRight = "]";
            
            if (BarOnly == false)
                percentRight += " " + Math.Round(CurrentPercent, 0).ToString().PadLeft(3) + "%";

            int totalSegments = AvailableSpace - LeftLength - RightLength - percentRight.Length - percentLeft.Length - 1;
            int completeSegments = (int)Math.Floor(totalSegments * (CurrentPercent/100.0));

            string progressBar = "";

            for (int i = 0; i < completeSegments; i++)
                progressBar += "=";

            if (Complete == false && completeSegments > 0)
                progressBar += (Increasing == true ? ">" : "<");

            string line = percentLeft + progressBar.PadRight(totalSegments) + percentRight;

            return line;
        }

        public static string FileCountPosition (long currentFile, long totalFiles, int width = 0)
        {
            string totalFilesStr = totalFiles.ToString();

            if (width == 0)
                width = totalFilesStr.Length;

            return $"[{currentFile.ToString().PadLeft(width)} / {totalFiles.ToString().PadLeft(width)}]";
        }
    }
}