using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Archiver.Utilities.Shared
{
    public class Pager : IDisposable
    {
        #region Constants
        private const string _statusLineLeft = "Navigate: <up>/<down> or <pageUp>/<pageDown>  Save To File: <ctrl>+s  Exit: q or <esc>";
        #endregion Constants
        
        #region Public Properties
        public bool AutoScroll { get; set; } = false;
        public bool ShowLineNumbers { get; set; } = false;
        public bool DeferDraw 
        { 
            get
            {
                return _deferDraw;
            }
        }
        #endregion Public Properties

        #region Private Fields
        private object _consoleLock = new object();
        
        private List<string> _lines;
        private int _startLine = 0;
        private int _bottomLine = 0;
        private int _windowHeight = -1;
        private int _windowWidth = -1;
        private int _topLineIndexPointer = 0;

        private int _maxWidth = -1;
        private int _maxLines = -1;
        private int _totalLines = 0;
        private int _lineNumberWidth = 0;
        private volatile bool _drawn = false;

        private Thread _thread;
        private volatile bool _abort = false;
        private bool _deferDraw = true;
        #endregion Private Fields

        #region Constructor
        public Pager()
        {
            Initialize();
        }

        public Pager(bool deferDraw)
        {
            _deferDraw = deferDraw;
        }

        public Pager(int StartLine)
        {
            _startLine = Console.WindowHeight - StartLine;

            Initialize();
        }

        private void Initialize()
        {
            _lines = new List<string>();
            _windowWidth = Console.WindowWidth - 1;
            _windowHeight = Console.WindowHeight - _startLine;
            _bottomLine = Console.WindowHeight - 1;
            _maxWidth = _windowWidth;
            _maxLines = _windowHeight - 1;

            _thread = new Thread(Run);
            _thread.Start();
        }
        #endregion Constructor

        public void AppendLine()
        {
            AppendLine(string.Empty);
        }

        public void AppendLine(string Line)
        {
            _lines.Add(Line);
            _totalLines++;

            if (this.AutoScroll == true)
                ScrollToBottom();
            else
            {
                if (DeferDraw == true)
                {
                    if (_totalLines >= _maxLines)
                    {
                        if (_drawn == false)
                            Redraw();
                        else
                            WriteStatusBar();
                    }
                }
                else
                {
                    if (_totalLines - _topLineIndexPointer <= _maxLines)
                        Redraw();
                    else
                        WriteStatusBar();
                }
            }
        }

        public void ScrollToTop()
        {
            _topLineIndexPointer = 0;

            Redraw();
        }

        public void ScrollToBottom()
        {
            _topLineIndexPointer = _totalLines - _maxLines;

            Redraw();
        }

        public void UpLine()
        {
            _topLineIndexPointer--;

            if (_topLineIndexPointer < 0)
                _topLineIndexPointer = 0;

            Redraw();
        }

        public void UpPage()
        {
            _topLineIndexPointer -= _maxLines;

            if (_topLineIndexPointer < 0)
                _topLineIndexPointer = 0;

            Redraw();
        }


        public void DownLine()
        {
            _topLineIndexPointer++;

            if (_topLineIndexPointer + _maxLines >= _totalLines)
                _topLineIndexPointer = _totalLines - (_maxLines > _totalLines ? _totalLines : _maxLines);

            Redraw();
        }

        public void DownPage()
        {
            _topLineIndexPointer += _maxLines;

            if (_topLineIndexPointer + _maxLines >= _totalLines)
                _topLineIndexPointer = _totalLines - (_maxLines > _totalLines ? _totalLines : _maxLines);

            Redraw();
        }

        public void SaveFile()
        {
            lock (_consoleLock)
            {
                Console.SetCursorPosition(0, _bottomLine);
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.CursorVisible = true;
                Console.Write(String.Empty.PadRight(_windowWidth));

                Console.SetCursorPosition(0, _bottomLine);
                Console.Write("Enter file name (blank to abort): ");
                string fileName = Console.ReadLine();
                Console.ResetColor();
                Console.CursorVisible = false;

                Redraw();

                fileName = fileName.Trim();

                if (fileName != String.Empty)
                {
                    if (File.Exists(fileName))
                    {
                        Console.SetCursorPosition(0, _bottomLine);
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(String.Empty.PadRight(_windowWidth));

                        Console.SetCursorPosition(0, _bottomLine);
                        Console.Write("ERROR: File already exists, try again.");
                        Console.ResetColor();

                        Thread.Sleep(2000);
                        Redraw();
                    }
                    else
                    {
                        using (StreamWriter stream = File.CreateText(fileName))
                        {
                            foreach (string line in _lines)
                                stream.WriteLine(line);
                        }

                        Console.SetCursorPosition(0, _bottomLine);
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(String.Empty.PadRight(_windowWidth));

                        Console.SetCursorPosition(0, _bottomLine);
                        Console.Write($"Success! Text saved to {fileName}");
                        Console.ResetColor();

                        Thread.Sleep(2000);
                        Redraw();    
                    }
                }
            }
        }

        public void WaitForExit()
        {
            if (!_drawn)
                Redraw();

            _thread.Join();
        }

        public void Dispose()
        {
            if (!_abort)
                _abort = true;

            WaitForExit();
        }


        #region Private Methods
        private void Run()
        {
            Setup();
            MainLoop();
            Cleanup();
        }

        private void Setup()
        {
            lock (_consoleLock)
            {
                Console.CursorVisible = false;

                if (!_deferDraw)
                    WriteStatusBar();

                Console.CancelKeyPress += (sender, e) => {
                    _abort = true;
                };
            }
        }

        private void MainLoop()
        {
            while (!_abort)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                    _abort = true;
                else if (key.Key == ConsoleKey.DownArrow)
                    DownLine();
                else if (key.Key == ConsoleKey.UpArrow)
                    UpLine();
                else if (key.Key == ConsoleKey.PageDown)
                    DownPage();
                else if (key.Key == ConsoleKey.PageUp)
                    UpPage();
                else if (key.Key == ConsoleKey.Home)
                    ScrollToTop();
                else if (key.Key == ConsoleKey.End)
                    ScrollToBottom();
                else if (key.Key == ConsoleKey.S && key.Modifiers == ConsoleModifiers.Control)
                    SaveFile();
            }
        }

        private void Cleanup()
        {
            lock (_consoleLock)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = true;
                Console.ResetColor();
            }

            _lines.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Redraw()
        {
            if (!_abort)
            {
                _drawn = true;

                WriteStatusBar();

                if (ShowLineNumbers)
                {
                    _lineNumberWidth = (_topLineIndexPointer + _maxLines).ToString().Length;
                    _maxWidth = _windowWidth - _lineNumberWidth - 1; // we subtract one more for a little padding
                }
                else
                    _lineNumberWidth = 0;

                IEnumerable<string> linesToShow = _lines.Skip(_topLineIndexPointer).Take(_maxLines);
                int i = 0;

                foreach (string line in linesToShow)
                {
                    lock (_consoleLock)
                    {
                        Console.SetCursorPosition(0, _startLine + i);

                        if (ShowLineNumbers == true)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.Write((_topLineIndexPointer + i + 1).ToString().PadLeft(_lineNumberWidth));
                            Console.ResetColor();
                            Console.Write(" ");
                        }

                        int lineWidth = (line.Length > _maxWidth ? _maxWidth : line.Length);

                        Console.Write(line.Substring(0, lineWidth).PadRight(_maxWidth));
                    }

                    i++;
                }
            }
        }

        private void WriteStatusBar()
        {
            string line = _statusLineLeft;

            int startLine = _topLineIndexPointer+1;
            int endLine = _topLineIndexPointer+_maxLines;

            if (endLine > _totalLines)
                endLine = _totalLines;

            double linePct = Math.Round(((double)(endLine) / (double)_totalLines) * 100.0, 0);
            string lineIndex = $"Line: {startLine}-{endLine} / {_totalLines}   {linePct.ToString("##0").PadLeft(3)}%";

            int leftPad = _windowWidth - lineIndex.Length;

            lock (_consoleLock)
            {
                Console.SetCursorPosition(0, _bottomLine);
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write(line.PadRight(leftPad) + lineIndex);
                Console.ResetColor();
            }
        }
        #endregion Private Methods
    }
}