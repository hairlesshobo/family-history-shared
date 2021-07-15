using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Archiver.Operations.Disc;
using Archiver.Operations.Tape;
using Terminal.Gui;

namespace Archiver.Views
{
    public class MainMenuItem
    {
        public string Category { get; set; }
        public string Label { get; set; } = String.Empty;
        public string Description { get; set; }
        public Action Action { get; set; }
        public Color ForegroundColor { get; set; }
        public bool DropFromGui { get; set; } = false;
        public bool Header { get; set; } = false;

        public override string ToString() => Label;
    }

    public class ViewMainMenu
    {
        MainMenuListSource _menuSource;
        // MainMenuListSource _tapeMenuSource;
        MainMenuItem _actionMenuItem = null;
        volatile bool _quit = false;

        public ViewMainMenu()
        {
            _menuSource = new MainMenuListSource(new List<MainMenuItem>()
            {
                new MainMenuItem() {
                    Label = "----- Disc Operations -----",
                    Header = true
                },
                new MainMenuItem() { 
                    Label = "Search Disc Archive", 
                    ForegroundColor = Color.Green,
                    Action = DiscSearcher.StartOperation,
                    DropFromGui = true
                },
                
                new MainMenuItem() { 
                    Label = "Restore entire disc(s)", 
                    ForegroundColor = Color.Green,
                    Action = NotImplemented
                },
                
                new MainMenuItem() { 
                    Label = "View Archive Summary", 
                    ForegroundColor = Color.Blue,
                    Action = DiscSummary.StartOperation,
                    DropFromGui = true
                },
                
                new MainMenuItem() { 
                    Label = "Verify Discs", 
                    ForegroundColor = Color.BrightYellow,
                    Action = DiscVerification.StartOperation,
                    DropFromGui = true
                },
                
                new MainMenuItem() { 
                    Label = "Scan For Changes", 
                    ForegroundColor = Color.BrightYellow,
                    Action = DiscArchiver.StartScanOnly,
                    DropFromGui = true
                },
                
                new MainMenuItem() { 
                    Label = "Scan For Renamed/Moved Files", 
                    ForegroundColor = Color.BrightYellow,
                    Action = ScanForFileRenames.StartOperation,
                    DropFromGui = true
                },
                
                new MainMenuItem() { 
                    Label = "Run Archive process", 
                    ForegroundColor = Color.Red,
                    Action = DiscArchiver.StartOperation,
                    DropFromGui = true
                },


                new MainMenuItem() {
                    Header = true
                },
                new MainMenuItem() {
                    Label = "----- Tape Operations -----",
                    Header = true
                },
                new MainMenuItem() {
                    Label = "Search Tape Archive",
                    Action = TapeSearcher.StartOperation,
                    ForegroundColor = Color.Green,
                    // SelectedValue = true, // do not show the "press enter to return to main menu" message
                },
                //! not implemented
                new MainMenuItem() {
                    Label = "Restore entire tape (to tar file)",
                    Action = RestoreTapeToTar.StartOperation,
                    // Disabled = !Config.TapeDrivePresent || true, // remove once implemented
                    ForegroundColor = Color.Green
                },
                //! not implemented
                new MainMenuItem() {
                    Label = "Restore entire tape (to original file structure)",
                    Action = NotImplemented,
                    // Disabled = !Config.TapeDrivePresent || true, // remove once implemented
                    ForegroundColor = Color.Green
                },
                new MainMenuItem() {
                    Label = "Read Tape Summary",
                    Action = ShowTapeSummary.StartOperation,
                    // Disabled = !Config.TapeDrivePresent,
                    // SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = Color.Blue
                },
                new MainMenuItem() {
                    Label = "View Archive Summary",
                    Action = TapeArchiveSummary.StartOperation,
                    // SelectedValue = true, // do not show the "press enter to return to main menu" message
                    ForegroundColor = Color.Blue
                },
                new MainMenuItem() {
                    Label = "Verify Tape",
                    Action = TapeVerification.StartOperation,
                    // Disabled = Config.ReadOnlyFilesystem || !Config.TapeDrivePresent,
                    ForegroundColor = Color.BrightYellow
                },
                new MainMenuItem() {
                    Label = "Run tape archive",
                    Action = TapeArchiver.StartOperation,
                    // Disabled = Config.ReadOnlyFilesystem || !Config.TapeDrivePresent,
                    ForegroundColor = Color.Red
                }
            });

        }

        private void MenuItemSelected(ListViewItemEventArgs item)
        {
            MainMenuItem menuItem = (MainMenuItem)item.Value;

            if (menuItem.DropFromGui)
            {
                _actionMenuItem = menuItem;
                Application.Current.Running = false;
            }
            else
                menuItem.Action();
        }

        public bool Show()
        {
            // reset the state handling
            _quit = false;
            _actionMenuItem = null;

            
            Application.Init();
            Toplevel top = Application.Top;

            Window mainWindow = new Window("Archiver - Main Menu")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                CanFocus = false
            };					


            // Label label_discMenu = new Label("----- Disc Operations -----") {
            //     X = 0,
            //     Y = 1
            // };
            // mainWindow.Add(label_discMenu);

            // Label button_test = new Label("_Not Implemented")
            // {
            //     X = 4,
            //     Y = Pos.Bottom(label_discMenu),
            //     CanFocus = true
            // };
            // button_test.Clicked += NotImplemented;

            // mainWindow.Add(button_test);


            ListView lv_discMenu = new ListView(_menuSource)
            {
                X = 4,
				Y = 1,
				Width = Dim.Fill(0),
				Height = _menuSource.Count,
				AllowsMarking = false,
				CanFocus = true,
            };
            lv_discMenu.OpenSelectedItem += MenuItemSelected;
            mainWindow.Add(lv_discMenu);


            // Label label_tapeMenu = new Label("----- Tape Operations -----") {
            //     X = 0,
            //     Y = Pos.Bottom(lv_discMenu) + 1
            // };
            // mainWindow.Add(label_tapeMenu);


            // ListView lv_tapeMenu = new ListView(_tapeMenuSource)
            // {
            //     X = 4,
			// 	Y = Pos.Bottom(label_tapeMenu),
			// 	Width = Dim.Fill(0),
			// 	Height = _tapeMenuSource.Count,
			// 	AllowsMarking = false,
			// 	CanFocus = true
            // };
            // lv_tapeMenu.OpenSelectedItem += MenuItemSelected;
            // mainWindow.Add(lv_tapeMenu);


            StatusBar statusBar = new StatusBar() 
            {
				Visible = true,
                ColorScheme = new ColorScheme() { Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Blue)}
			};

			statusBar.Items = new StatusItem[] 
            {
				new StatusItem(Key.Q | Key.CtrlMask, "~Ctrl-Q~ Quit", () => 
                {
                    _quit = true;
                    Application.RequestStop();
					
                    // if (_runningScenario is null){
					// 	// This causes GetScenarioToRun to return null
					// 	_runningScenario = null;
						// Application.RequestStop();
					// } else {
					// 	_runningScenario.RequestStop();
					// }
				}),
				// new StatusItem(Key.F10, "~F10~ Hide/Show Status Bar", () => 
                // {
				// 	statusBar.Visible = !statusBar.Visible;
                //     win.Height = Dim.Fill(statusBar.Visible ? 1 : 0);
				// 	// _leftPane.Height = Dim.Fill(_statusBar.Visible ? 1 : 0);
				// 	// _rightPane.Height = Dim.Fill(_statusBar.Visible ? 1 : 0);
                //     // top.LayoutSubviews();
				// 	win.LayoutSubviews();
				// 	win.SetChildNeedsDisplay();
                //     // top.SetChildNeedsDisplay();
				// }),
				new StatusItem (Key.CharMask, Application.Driver.GetType().Name, null),
			};

            

            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Black);

            top.Add(mainWindow);
            top.Add(statusBar);

            Application.Run();

            if (_actionMenuItem != null)
            {
                if (_actionMenuItem.DropFromGui)
                {
                    Application.Shutdown();
                    Console.Clear();
                }

                if (_actionMenuItem.Action != null)
                    _actionMenuItem.Action();
            }

            return _quit;
        }

        public static void NotImplemented()
            => MessageBox.ErrorQuery(40, 10, "Error", "This operation has not yet been implemented.", "Ok");

        public class MainMenuListSource : IListDataSource
        {
            int _prevSelected = -1;

            List<MainMenuItem> _items;

            public int Count => _items.Count;

            public int Length => 20;

            public MainMenuListSource(List<MainMenuItem> menuItems) 
            {
                _items = menuItems;
            }

            public bool IsMarked(int item) => false;

            public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
            {
                MainMenuItem menuItem = _items[item];

                if (menuItem.Header)
                    menuItem.ForegroundColor = Color.Cyan;
                else
                {
                    driver.SetAttribute(Application.Driver.MakeAttribute(Color.Black, Color.Black));
                    driver.AddStr("    ");
                }

                Color backgroundColor = selected && !menuItem.Header ? Color.DarkGray : Color.Black;

                driver.SetAttribute(Application.Driver.MakeAttribute(menuItem.ForegroundColor, backgroundColor));
                StringBuilder builder = new StringBuilder();

                if (menuItem.Header)
                    driver.AddStr(menuItem.Label);
                else
                {
                    builder.Append(selected ? ">" : " ");
                    builder.Append(" ");
                    builder.Append(menuItem.Label);
                    
                    driver.AddStr(builder.ToString());
                }
                
                if (selected)
                {
                    bool isMovingDown = item > _prevSelected;
                    _prevSelected = item;

                    if (menuItem.Header == true)
                    {
                        selected = false;

                        // TODO: Add safety check here to make sure there is a down
                        if (isMovingDown)
                            container.MoveDown();
                        else
                        {
                            if (item == 0 && menuItem.Header)
                                container.MoveDown();
                            else
                                container.MoveUp();
                        }
                    }
                }
            }

            public void SetMark(int item, bool value) { }

            public IList ToList() => _items;
        }
    }
}