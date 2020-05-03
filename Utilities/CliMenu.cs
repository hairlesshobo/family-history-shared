using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscArchiver.Utilities
{
    public class CliMenuEntry
    {
        public string Name { get; set; }
        public Action Action { get; set; }
        public bool Selected { 
            get
            {
                return _selected;
            }
            internal set
            {
                _selected = value;
            }
        }

        private bool _selected;
    }

    public class CliMenu
    {
        private List<CliMenuEntry> _entries;
        public string MenuLabel { get; set; }
        public ConsoleColor _foregroundColor;

        private int _startLine;
        
        public CliMenu (List<CliMenuEntry> Entries)
        {
            _entries = Entries;

            if (_entries != null)
            {
                if (!(_entries.Any(x => x.Selected)))
                    _entries.First().Selected = true;

                // make sure there is only one selected by default
                if (_entries.Count(x => x.Selected == true) > 1)
                {
                    _entries.ForEach(x => x.Selected = false);
                    _entries.First().Selected = true;
                }
            }
        }

        public void Show()
        {
            Show(false);
        }

        public void Show(bool ClearScreen)
        {
            Console.CursorVisible = false;
            _foregroundColor = Console.ForegroundColor;

            if (ClearScreen)
                Console.Clear();

            Console.WriteLine();

            if (this.MenuLabel != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(this.MenuLabel);
                Console.WriteLine();
                Console.ForegroundColor = _foregroundColor;
            }

            _startLine = Console.CursorTop;

            foreach (CliMenuEntry entry in _entries)
            {
                WriteMenuEntry(entry);
            }

            while (1 == 1)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                CliMenuEntry selectedEntry = _entries.First(x => x.Selected);

                int currentSelection = _entries.IndexOf(selectedEntry);

                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow)
                {
                    selectedEntry.Selected = false;
                    WriteMenuEntry(selectedEntry);
                }

                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (currentSelection < (_entries.Count()-1))
                        currentSelection++;
                    else
                        currentSelection = 0;
                }

                if (key.Key == ConsoleKey.UpArrow)
                {
                    selectedEntry.Selected = false;
                    WriteMenuEntry(selectedEntry);

                    if (currentSelection > 0)
                        currentSelection--;
                    else
                        currentSelection = _entries.Count()-1;
                        
                }

                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow)
                {
                    selectedEntry = _entries[currentSelection];
                    selectedEntry.Selected = true;
                    WriteMenuEntry(selectedEntry);
                }

                if (key.Key == ConsoleKey.Enter)
                    break;
            }

            Console.CursorVisible = true;
            Console.SetCursorPosition(0, _startLine+_entries.Count()+1);

            CliMenuEntry finalEntry = _entries.First(x => x.Selected);

            if (ClearScreen)
                Console.Clear();

            if (finalEntry != null && finalEntry.Action != null)
                finalEntry.Action();
        }

        private void WriteMenuEntry(CliMenuEntry Entry)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = _startLine + _entries.IndexOf(Entry);
            Console.Write("    ");

            if (Entry.Selected)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> ");
                Console.ForegroundColor = _foregroundColor;
            }
            else
                Console.Write("  ");

            Console.Write(Entry.Name);
            Console.CursorLeft = 0;

            Console.ResetColor();
        }
    }
}