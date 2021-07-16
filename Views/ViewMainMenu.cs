using System;
using System.Collections.Generic;
using Archiver.Classes.Views;
using Archiver.Utilities.Shared;
using Terminal.Gui;

namespace Archiver.Views
{
    public class ViewMainMenu
    {
        private List<int> _menuEntryIndex = new List<int>();
        HyperlinkInfo _actionLink = null;
        volatile bool _quit = false;

        private void MenuItemSelected(HyperlinkInfo link)
        {
            if (link.DropFromGui)
            {
                _actionLink = link;
                Application.Current.Running = false;
            }
            else if (link.Action != null)
                link.Action();
        }

        public bool Show()
        {
            // reset the state handling
            _menuEntryIndex.Clear();
            _quit = false;
            _actionLink = null;


            Application.Init();
            GuiGlobals.Colors.InitColorSchemes();

            Toplevel top = Application.Top;
            Window mainWindow = BuildMainWindow(top);

            BuildDiscMenu(mainWindow);
            BuildTapeMenu(mainWindow);
            BuildCsdMenu(mainWindow);
            BuildUniversalMenu(mainWindow);
            
            BuildStatusBar(top);

            Application.Run();

            HandlePostRunAction();

            return _quit;
        }

        private void AddLabel(Window win, string text, int marginAbove = 0)
        {
            int newY = GetNextLocation(marginAbove);
            _menuEntryIndex.Add(newY);

            Label newLabel = new Label($"----- {text} -----") 
            {
                X = 0,
                Y = newY
            };
            win.Add(newLabel);
        }

        private void AddHyperlink(Window win, HyperlinkInfo info, int marginAbove = 0)
        {
            int newY = GetNextLocation(marginAbove);
            _menuEntryIndex.Add(newY);

            Hyperlink newHyperlink = new Hyperlink(info)
            {
                X = 4,
                Y = newY,
                ActionHandler = MenuItemSelected
            };
            win.Add(newHyperlink);
        }

        private int GetNextLocation(int marginAbove = 0)
        {
            int count = _menuEntryIndex.Count;

            if (count == 0)
                return 1 + marginAbove;
            else
                return _menuEntryIndex[count-1] + 1 + marginAbove;
        }

        private void HandlePostRunAction()
        {
            if (_actionLink != null)
            {
                if (_actionLink.DropFromGui)
                {
                    Application.Shutdown();
                    Console.Clear();
                }

                if (_actionLink.Action != null)
                    _actionLink.Action();
            }
        }

        private Window BuildMainWindow(Toplevel top)
        {
            Window mainWindow = new Window("Archiver - Main Menu")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                CanFocus = false,
                ColorScheme = GuiGlobals.Colors.GlobalScheme
            };

            top.Add(mainWindow);

            return mainWindow;
        }

        private void BuildStatusBar(Toplevel top)
        {
            StatusBar statusBar = new StatusBar() 
            {
				Visible = true,
                ColorScheme = GuiGlobals.Colors.StatusBar
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

            top.Add(statusBar);
        }

        private void BuildDiscMenu(Window mainWindow)
        {
            AddLabel(mainWindow, "Disc Operations");

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Search Disc Archive", 
                Color = GuiGlobals.Colors.Green,
                Action = Operations.Disc.DiscSearcher.StartOperation,
                DropFromGui = true
            });
            

            //! not implemented
            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Restore entire disc(s)", 
                Color = GuiGlobals.Colors.Green,
                Action = NotImplemented,
                Disabled = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "View Archive Summary", 
                Color = GuiGlobals.Colors.Blue,
                Action = Operations.Disc.DiscSummary.StartOperation,
                DropFromGui = true
            });
            
            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Verify Discs", 
                Color = GuiGlobals.Colors.Yellow,
                Action = Operations.Disc.DiscVerification.StartOperation,
                DropFromGui = true
            });
            
            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Scan For Changes", 
                Color = GuiGlobals.Colors.Yellow,
                Action = Operations.Disc.DiscArchiver.StartScanOnly,
                DropFromGui = true
            });
            
            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Scan For Renamed/Moved Files", 
                Color = GuiGlobals.Colors.Yellow,
                Action = Operations.Disc.ScanForFileRenames.StartOperation,
                DropFromGui = true
            });
            
            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Run Archive process", 
                Color = GuiGlobals.Colors.Red,
                Action = Operations.Disc.DiscArchiver.StartOperation,
                DropFromGui = true
            });
        }

        private void BuildTapeMenu(Window mainWindow)
        {
            AddLabel(mainWindow, "Tape Operations", 1);

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Search Tape Archive",
                Action = Operations.Tape.TapeSearcher.StartOperation,
                Color = GuiGlobals.Colors.Green,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Search Tape Archive",
                Action = Operations.Tape.TapeSearcher.StartOperation,
                Color = GuiGlobals.Colors.Green,
                DropFromGui = true
            });

            //! not implemented
            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Restore entire tape (to tar file)",
                Action = Operations.Tape.RestoreTapeToTar.StartOperation,
                // Disabled = !Config.TapeDrivePresent || true, // remove once implemented
                Color = GuiGlobals.Colors.Green,
                DropFromGui = true
            });

            //! not implemented
            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Restore entire tape (to original file structure)",
                Action = NotImplemented,
                // Disabled = !Config.TapeDrivePresent || true, // remove once implemented
                Color = GuiGlobals.Colors.Green,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Read Tape Summary",
                Action = Operations.Tape.ShowTapeSummary.StartOperation,
                // Disabled = !Config.TapeDrivePresent,
                Color = GuiGlobals.Colors.Blue,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "View Archive Summary",
                Action = Operations.Tape.TapeArchiveSummary.StartOperation,
                // SelectedValue = true, // do not show the "press enter to return to main menu" message
                Color = GuiGlobals.Colors.Blue,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Verify Tape",
                Action = Operations.Tape.TapeVerification.StartOperation,
                // Disabled = Config.ReadOnlyFilesystem || !Config.TapeDrivePresent,
                Color = GuiGlobals.Colors.Yellow,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() 
            {
                Text = "Run tape archive",
                Action = Operations.Tape.TapeArchiver.StartOperation,
                // Disabled = Config.ReadOnlyFilesystem || !Config.TapeDrivePresent,
                Color = GuiGlobals.Colors.Red,
                DropFromGui = true
            });

        }

        private void BuildCsdMenu(Window mainWindow)
        {
            AddLabel(mainWindow, "Cold Storage Disk (HDD) Operations", 1);


            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Register CSD Drive",
                Action = Operations.CSD.RegisterDrive.StartOperation,
                Color = GuiGlobals.Colors.Green,
                DropFromGui = true
            });

            //! not implemented
            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Restore entire CSD Drive",
                Action = NotImplemented,
                //Disabled = true, // remove once implemented
                Color = GuiGlobals.Colors.Green,
                DropFromGui = true
            });

            //! not implemented
            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Read CSD Drive Summary",
                Action = NotImplemented,         
                // Action = ShowTapeSummary.StartOperation,
                //Disabled = true, // remove once implemented
                // SelectedValue = true, // do not show the "press enter to return to main menu" message
                Color = GuiGlobals.Colors.Blue,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "View CSD Archive Summary",
                Action = Operations.CSD.ArchiveSummary.StartOperation,
                // SelectedValue = true, // do not show the "press enter to return to main menu" message
                Color = GuiGlobals.Colors.Blue,
                DropFromGui = true
            });

            //! not implemented
            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Verify CSD Drive",
                Action = NotImplemented,
                // Action = TapeVerification.StartOperation,
                //Disabled = Config.ReadOnlyFilesystem || true, // remove once implemented
                Color = GuiGlobals.Colors.Yellow,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Clean CSD Drive - Remove files not in index",
                Action = Operations.CSD.Cleaner.StartOperation,
                // Action = TapeVerification.StartOperation,
                //Disabled = Config.ReadOnlyFilesystem, // remove once implemented
                Color = GuiGlobals.Colors.Yellow,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Run CSD Archive Process",
                Action = Operations.CSD.Archiver.StartOperation,
                //Disabled = Config.ReadOnlyFilesystem,
                Color = GuiGlobals.Colors.Red,
                DropFromGui = true
            });

        }

        private void BuildUniversalMenu(Window mainWindow)
        {
            AddLabel(mainWindow, "Universal Operations", 1);

            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Copy Tools to Local Disk",
                Action = NotImplemented,
                //Disabled = !Config.ReadOnlyFilesystem,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Create Index ISO",
                Action = Helpers.CreateIndexIso,
                //Disabled = Config.ReadOnlyFilesystem,
                DropFromGui = true
            });

            AddHyperlink(mainWindow, new HyperlinkInfo() {
                Text = "Exit",
                Action = () => 
                {
                    _quit = true;
                    Application.RequestStop();
                }
            });
        }

        public static void NotImplemented()
            => MessageBox.ErrorQuery(40, 10, "Error", "This operation has not yet been implemented.", "Ok");
    }
}