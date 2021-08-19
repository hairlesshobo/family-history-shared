using System;
using Archiver.Shared;
using Archiver.Shared.Utilities;
using Terminal.Gui;

namespace Archiver.ViewComponents
{
    public static class StatusBarComponent
    {
        public static void Add(Toplevel top, Action requestQuit)
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
                    // _quit = true;
                    requestQuit();
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
                new StatusItem (Key.CharMask, "Tape Drive: " + (SysInfo.IsTapeDrivePresent ? SysInfo.TapeDrive : "NOT Detected"), null),
                new StatusItem (Key.CharMask, "Optical Drives: " + (OpticalDriveUtils.GetDriveNames().Length), null),
				new StatusItem (Key.CharMask, "GUI Driver: " + Application.Driver.GetType().Name, null)
			};

            top.Add(statusBar);
        }
    }
}