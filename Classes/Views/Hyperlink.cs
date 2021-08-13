//
// Button.cs: Button control
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Adapted by Steve Cross from the "Button" view source code

using System;
using NStack;
using Terminal.Gui;
using TAttribute = Terminal.Gui.Attribute;

namespace Archiver.Classes.Views {
	/// <summary>
	///   Hyperlink is a <see cref="View"/> that provides an item that invokes an <see cref="Action"/> when activated by the user.
	/// </summary>
	/// <remarks>
	/// <para>
	///   Provides a hyperlink showing text invokes an <see cref="Action"/> when clicked on with a mouse
	///   or when the user presses SPACE, ENTER, or hotkey. The hotkey is the first letter or digit following the first underscore ('_') 
	///   in the button text. 
	/// </para>
	/// <para>
	///   Use <see cref="View.HotKeySpecifier"/> to change the hotkey specifier from the default of ('_'). 
	/// </para>
	/// <para>
	///   If no hotkey specifier is found, the first uppercase letter encountered will be used as the hotkey.
	/// </para>
	/// </remarks>
	public class Hyperlink : View {
        Action<HyperlinkInfo> _actionHandler = null;
        HyperlinkInfo _info;
        Rune _rightArrow;
        ustring _text;
        bool _isHighlighted;

        /// <summary>
		///   The text displayed by this <see cref="Hyperlink"/>.
		/// </summary>
		public new ustring Text {
			get {
				return _text;
			}

			set {
				_text = value;
				Update ();
			}
		}

        public Action<HyperlinkInfo> ActionHandler
        {
            get {
                if (_actionHandler == null)
                    return (info) => info.Action();
                else
                    return _actionHandler;
            }
            set {
                _actionHandler = value;
            }
        }

        public HyperlinkInfo HyperlinkInfo => _info;

		/// <summary>
		///   Initializes a new instance of <see cref="Hyperlink"/> using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		/// <remarks>
		///   The width of the <see cref="Hyperlink"/> is computed based on the
		///   text length. The height will always be 1.
		/// </remarks>
		public Hyperlink () : this (text: string.Empty) { }

        public Hyperlink(HyperlinkInfo info)
        {
            _info = info;
            Init(info.ToString());
        }

		/// <summary>
		///   Initializes a new instance of <see cref="Hyperlink"/> using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		/// <remarks>
		///   The width of the <see cref="Hyperlink"/> is computed based on the
		///   text length. The height will always be 1.
		/// </remarks>
		/// <param name="text">The button's text</param>
		public Hyperlink (ustring text) : base (text)
		{
			Init (text);
		}

		void Init (ustring text)
		{
            HotKeySpecifier = '\xffff';
			// HotKeySpecifier = new Rune ('_');

			_rightArrow = new Rune ('>');

			CanFocus = true;
			this._text = text ?? string.Empty;

            if (_info != null)
            {
                if (_info.Action != null)
                    Clicked += () => this.ActionHandler(_info);

                if (_info.Disabled)
                    this.CanFocus = false;
            }

			Update();

            this.Enter += (args) => {
                _isHighlighted = true;
                
                Update();
            };

            this.Leave += (args) => {
                _isHighlighted = false;

                Update();
            };
		}

		internal void Update ()
		{
            if (_isHighlighted)
                base.Text = ustring.Make(_rightArrow) + " " + _text;
            else
                base.Text = "  " + _text;

			int w = base.Text.RuneCount - (base.Text.Contains (HotKeySpecifier) ? 1 : 0);
			
            GetCurrentWidth (out int cWidth);

			bool canSetWidth = SetWidth (w, out int rWidth);
			
            if (canSetWidth && (cWidth < rWidth || AutoSize)) 
            {
				Width = rWidth;
				w = rWidth;
            } 
            else if (!canSetWidth || !AutoSize)
				w = cWidth;

			Height = 1;
			Frame = new Rect (Frame.Location, new Size (w, 1));

            if (_info != null && _info.Color != null)
                base.ColorScheme = _info.Color;

			SetNeedsDisplay ();
		}

		bool CheckKey (KeyEvent key)
		{
			if (key.Key == (Key.AltMask | HotKey))
            {
				SetFocus ();
				Clicked?.Invoke ();
				return true;
			}
			return false;
		}

		///<inheritdoc/>
		public override bool ProcessHotKey (KeyEvent kb)
		{
			if (kb.IsAlt)
				return CheckKey (kb);

			return false;
		}

		///<inheritdoc/>
		public override bool ProcessColdKey (KeyEvent kb)
		{
			return CheckKey (kb);
		}

		///<inheritdoc/>
		public override bool ProcessKey (KeyEvent kb)
		{
			var c = kb.KeyValue;
			if (c == '\n' || c == ' ' || kb.Key == HotKey) 
            {
				Clicked?.Invoke ();
				return true;
			}

			return base.ProcessKey (kb);
		}


		/// <summary>
		///   Clicked <see cref="Action"/>, raised when the user clicks the primary mouse button within the Bounds of this <see cref="View"/>
		///   or if the user presses the action key while this view is focused. (TODO: IsDefault)
		/// </summary>
		/// <remarks>
		///   Client code can hook up to this event, it is
		///   raised when the button is activated either with
		///   the mouse or the keyboard.
		/// </remarks>
		public event Action Clicked;

		///<inheritdoc/>
		public override bool MouseEvent (MouseEvent me)
		{
			if (me.Flags == MouseFlags.Button1Clicked || me.Flags == MouseFlags.Button1DoubleClicked ||
				me.Flags == MouseFlags.Button1TripleClicked) 
            {
				if (CanFocus) 
                {
					if (!HasFocus) 
                    {
						SetFocus ();
						SetNeedsDisplay ();
					}

					Clicked?.Invoke ();
				}

				return true;
			}

			return false;
		}

		///<inheritdoc/>
		public override void PositionCursor ()
		{
			if (HotKey == Key.Unknown) 
            {
				for (int i = 0; i < base.Text.RuneCount; i++) 
                {
					if (base.Text [i] == _text [0]) 
                    {
						Move (i, 0);
						return;
					}
				}
			}

			base.PositionCursor ();
		}

		///<inheritdoc/>
		public override bool OnEnter (View view)
		{
			Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);

			return base.OnEnter(view);
		}

        // TextFormatter textFormatter = new TextFormatter(); 
        		///<inheritdoc/>
		public override void Redraw (Rect bounds)
		{
			//if (Frame.Y != Driver.Rows - 1) {
			//	Frame = new Rect (Frame.X, Driver.Rows - 1, Frame.Width, Frame.Height);
			//	Y = Driver.Rows - 1;
			//	SetNeedsDisplay ();
			//}

			Move (0, 0);
			// Driver.SetAttribute (ColorScheme.Normal);
			// for (int i = 0; i < Frame.Width; i++)
			// 	Driver.AddRune (' ');

			Move (1, 0);
			TAttribute normalScheme = ColorScheme.Normal;
            TAttribute hotScheme = ColorScheme.HotNormal;

            if (_isHighlighted)
            {
                normalScheme = ColorScheme.Focus;
                hotScheme = ColorScheme.HotFocus;
            }

            // disabled
            if (!this.CanFocus)
            {
                normalScheme = ColorScheme.Disabled;
                Driver.SetAttribute (normalScheme);
                Driver.AddStr(base.Text);
            }
            else
            {
                DrawHotString(base.Text, hotScheme, normalScheme);
            }

			// Driver.SetAttribute (normalScheme);
            
            // Driver.AddStr(base.Text);
			// for (int i = 0; i < Items.Length; i++) {
			// 	var title = Items [i].Title.ToString ();
			// 	for (int n = 0; n < Items [i].Title.RuneCount; n++) {
			// 		if (title [n] == '~') {
			// 			scheme = ToggleScheme (scheme);
			// 			continue;
			// 		}
			// 		Driver.AddRune (title [n]);
			// 	}
			// 	if (i + 1 < Items.Length) {
			// 		Driver.AddRune (' ');
			// 		Driver.AddRune (Driver.VLine);
			// 		Driver.AddRune (' ');
			// 	}
			// }
		}
    }

    public class HyperlinkInfo
    {
        public string Text { get; set; } = String.Empty;
        public string Description { get; set; }
        public Action Action { get; set; }
        public ColorScheme Color { get; set; }
        public bool Disabled { get; set; } = false;
        public bool DropFromGui { get; set; } = false;
        public bool Header { get; set; } = false;
		public bool PauseAfterOperation { get; set; } = true;

        public override string ToString() => Text;
    }
}
