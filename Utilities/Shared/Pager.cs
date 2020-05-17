using System;
using System.Collections.Generic;

namespace Archiver.Utilities.Shared
{
    public class Pager
    {
        public bool AutoScroll { get; set; } = false;
        public bool ShowLineNumbers { get; set; } = true;
        
        private List<string> _lines;
        private int _startLine = 0;
        private int _bottomLine = 0;
        private int _maxLines = -1;
        private int _maxWidth = -1;
        private int _topLineIndexPointer = 0;

        public Pager()
        {
            Initialize();
        }

        public Pager(int StartLine)
        {
            _startLine = Console.WindowHeight - StartLine;

            Initialize();
        }

        private void Initialize()
        {
            _lines = new List<string>();
            _maxWidth = Console.WindowWidth - 1;
            _maxLines = Console.WindowHeight - _startLine;
            _bottomLine = Console.WindowHeight - 1;

            Console.CursorVisible = false;

            Console.SetCursorPosition(0, _bottomLine);
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Write("".PadRight(_maxWidth));
            Console.ResetColor();
        }

        public void AppendLine(string Line, bool ScrollToLine = false)
        {
            _lines.Add(Line);

            if (_startLine + _topLineIndexPointer < _bottomLine)
            {
                Console.SetCursorPosition(0, _startLine + _topLineIndexPointer);
                _topLineIndexPointer++;

                Console.WriteLine(Line);
            }
        }

        public void ScrollToTop()
        {

        }

        public void ScrollToBottom()
        {

        }
    }
}