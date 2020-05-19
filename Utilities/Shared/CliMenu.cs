using System;
using System.Collections.Generic;
using System.Linq;

namespace Archiver.Utilities.Shared
{
    public delegate void CliMenuCanceled();

    public class CliMenuEntry : CliMenuEntry<string>
    {
        public CliMenuEntry():base()
        {}
    }

    public class CliMenuEntry<TKey>
    {
        public string Name { get; set; }
        public Action Action { get; set; }
        public bool Disabled { get; set; } = false;
        public bool Header { get; set; } = false;
        public TKey SelectedValue { get; set; }
        public ConsoleColor ForegroundColor { get; set; } = Console.ForegroundColor;
        public ConsoleColor BackgroundColor { get; set; } = Console.BackgroundColor;
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

    public class CliMenu<TKey>
    {
        #region Public Properties
        public event CliMenuCanceled OnCancel;
        public string MenuLabel { get; set; }
        public ConsoleColor HeaderColor { get; set; } = ConsoleColor.Magenta;
        public ConsoleColor KeyColor { get; set; } = ConsoleColor.DarkYellow;
        public ConsoleColor CursorBackgroundColor { get; set; } = ConsoleColor.DarkGray;
        public ConsoleColor CursorForegroundColor { get; set; } = ConsoleColor.DarkGreen;
        public ConsoleColor CursorArrowColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor DisabledForegroundColor { get; set; } = ConsoleColor.DarkGray;
        public Boolean MultiSelect { 
            get
            {
                return _multiSelect;
            }
            private set
            {
                _multiSelect = value;
            } 
        }
        public List<TKey> SelectedEntries {
            get {
                return _entries.Where(x => x.Selected).Select(x => x.SelectedValue).ToList();
            }
        }
        #endregion Public Properties

        #region Private Fields
        private List<CliMenuEntry<TKey>> _entries;
        private ConsoleColor _foregroundColor;
        private bool _multiSelect;
        private int _cursorIndex = -1;
        private bool _canceled = false;

        private int _startLine;
        #endregion Private Fields

        #region Constructors
        public CliMenu (List<CliMenuEntry<TKey>> entries, bool multiSelect)
        {
            Initalize(entries, multiSelect);
        }

        public CliMenu (List<CliMenuEntry<TKey>> Entries)
        {
            Initalize(Entries, false);
        }
        
        public void Initalize (List<CliMenuEntry<TKey>> Entries, bool multiSelect)
        {
            this.MultiSelect = multiSelect;
            _entries = Entries;
            _cursorIndex = _entries.IndexOf(_entries.First(x => !x.Disabled && !x.Header));

            if (_entries != null)
            {
                if (_multiSelect == false && (!_entries.Any(x => x.Selected && !x.Disabled && !x.Header)))
                    _entries.First(x => !x.Disabled && !x.Header).Selected = true;

                // if this isn't a multiselect menu, make sure there is only one selected by default
                if (this.MultiSelect == false && _entries.Count(x => x.Selected == true) > 1)
                {
                    _entries.ForEach(x => x.Selected = false);
                    _entries.First().Selected = true;
                }
            }

            this.OnCancel += delegate {};
        }
        #endregion Constructors

        #region Public Methods
        public List<TKey> Show()
        {
            return Show(false);
        }

        public List<TKey> Show(bool ClearScreen)
        {
            Console.CursorVisible = false;
            _foregroundColor = Console.ForegroundColor;

            if (ClearScreen)
                Console.Clear();

            Console.WriteLine();

            if (this.MenuLabel != null)
            {
                Console.ForegroundColor = this.HeaderColor;
                Console.WriteLine(this.MenuLabel);
                Console.WriteLine();
                Console.ForegroundColor = _foregroundColor;
            }

            _startLine = Console.CursorTop;

            foreach (CliMenuEntry<TKey> entry in _entries)
            {
                WriteMenuEntry(entry);
            }

            Console.SetCursorPosition(0, _startLine + _entries.Count() + 1);
            
            if (_multiSelect == true)
            {
                Console.Write("Press ");
                Console.ForegroundColor = this.KeyColor;
                Console.Write("<space>");
                Console.ForegroundColor = _foregroundColor;
                Console.Write(" to select entry, ");
                Console.ForegroundColor = this.KeyColor;
                Console.Write("<Shift>-A");
                Console.ForegroundColor = _foregroundColor;
                Console.Write(" to select all, ");
                Console.ForegroundColor = this.KeyColor;
                Console.Write("<Shift>-D");
                Console.ForegroundColor = _foregroundColor;
                Console.Write(" to deselect all");
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.Write("Press ");
            Console.ForegroundColor = this.KeyColor;
            Console.Write("<enter>");
            Console.ForegroundColor = _foregroundColor;
            Console.Write(" when finished, ");
            Console.ForegroundColor = this.KeyColor;
            Console.Write("<esc>");
            Console.ForegroundColor = _foregroundColor;
            Console.Write(" or ");
            Console.ForegroundColor = this.KeyColor;
            Console.Write("q");
            Console.ForegroundColor = _foregroundColor;
            Console.Write(" to cancel");

            while (1 == 1)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                CliMenuEntry<TKey> selectedEntry = _entries[_cursorIndex];
                CliMenuEntry<TKey> previousEntry = selectedEntry;

                if (_multiSelect == false && (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow))
                    selectedEntry.Selected = false;

                if (_multiSelect == true)
                {
                    if (key.Key == ConsoleKey.Spacebar)
                    {
                        selectedEntry.Selected = !selectedEntry.Selected;
                        WriteMenuEntry(selectedEntry);
                    }
                    else if (key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if (key.Key == ConsoleKey.A)
                        {
                            foreach (CliMenuEntry<TKey> entry in _entries)
                            {
                                entry.Selected = true;
                                WriteMenuEntry(entry);
                            }
                        }
                        else if (key.Key == ConsoleKey.D)
                        {
                            foreach (CliMenuEntry<TKey> entry in _entries)
                            {
                                entry.Selected = false;
                                WriteMenuEntry(entry);
                            }
                        }
                    }
                }

                if (key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Q)
                {
                    _canceled = true;
                    break;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                    MoveCursor(selectedEntry, true);
                else if (key.Key == ConsoleKey.UpArrow)
                    MoveCursor(selectedEntry, false);

                if (_multiSelect == false && (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow))
                {
                    WriteMenuEntry(previousEntry);
                    selectedEntry = _entries[_cursorIndex];
                    selectedEntry.Selected = true;
                    WriteMenuEntry(selectedEntry);
                }
                else
                {
                    WriteMenuEntry(previousEntry);
                    WriteMenuEntry(_entries[_cursorIndex]);
                }

                if (key.Key == ConsoleKey.Enter)
                    break;
            }

            Console.CursorVisible = true;
            Console.SetCursorPosition(0, _startLine+_entries.Count()+1);

            if (_canceled == false)
            {
                if (_multiSelect == false)
                {
                    CliMenuEntry<TKey> finalEntry = _entries.First(x => x.Selected);

                    if (ClearScreen)
                        Console.Clear();

                    if (finalEntry != null && finalEntry.Action != null)
                        finalEntry.Action();

                    return new List<TKey>() {
                        finalEntry.SelectedValue
                    };
                }
                else
                    return this.SelectedEntries;
            }
            else
            {
                this.OnCancel();
                return null;
            }
        }
        #endregion Public Methods

        #region Private Methods
        private void MoveCursor(CliMenuEntry<TKey> entry, bool down)
        {
            if (down == true)
            {
                if (_cursorIndex < (_entries.Count()-1))
                    _cursorIndex++;
                else
                    _cursorIndex = 0;

                if (_entries[_cursorIndex].Disabled || _entries[_cursorIndex].Header)
                    MoveCursor(_entries[_cursorIndex], down);
            }
            else
            {
                if (_cursorIndex > 0)
                    _cursorIndex--;
                else
                    _cursorIndex = _entries.Count()-1;

                if (_entries[_cursorIndex].Disabled || _entries[_cursorIndex].Header)
                    MoveCursor(_entries[_cursorIndex], down);
            }
        }

        private void WriteMenuEntry(CliMenuEntry<TKey> Entry)
        {
            int entryIndex = _entries.IndexOf(Entry);

            Console.CursorLeft = 0;
            Console.CursorTop = _startLine + entryIndex;
            

            if (Entry.Header)
                Console.ForegroundColor = ConsoleColor.Cyan;

            else
            {
                Console.Write("    ");

                if (entryIndex == _cursorIndex)
                {
                    Console.BackgroundColor = this.CursorBackgroundColor;
                    Console.ForegroundColor = this.CursorArrowColor;
                    Console.Write("> ");
                    Console.ForegroundColor = this.CursorForegroundColor;
                }
                else if (Entry.Disabled)
                {
                    Console.ForegroundColor = this.DisabledForegroundColor;
                    Console.Write("  ");
                }
                else
                    Console.Write("  ");
            }

            if (_multiSelect == true)
            {
                Console.Write("[");
            
                if (Entry.Selected == true)
                    Console.Write("X");
                else
                    Console.Write(" ");
            
                Console.Write("] ");
            }


            if (Entry.Header && Entry.Name != null)
                Console.Write($"----- {Entry.Name} -----");
            else
            {
                if (!Entry.Disabled)
                    Console.ForegroundColor = Entry.ForegroundColor;

                Console.Write(Entry.Name);
            }

            Console.CursorLeft = 0;

            Console.ResetColor();
        }
        #endregion Private Methods
    }
}