using System;
using System.Collections.Generic;
using Archiver.Shared;
using Archiver.Shared.Utilities;
using Archiver.Views;
using Terminal.Gui;

namespace Archiver.ViewComponents
{
    [Flags]
    public enum StatusBarOptions
    {
        None = 0,
        HideAbout = 1,
        HideHelp = 2,
        HideTapeInfo = 4,
        HideDiscInfo = 8,
        HideQuit = 16,
        HideTerminalDriver = 32,
        ShowAbort = 64
    }
    public static class StatusBarComponent
    {
        public static void Add(Toplevel top, Action requestQuit)
            => Add(top, requestQuit, StatusBarOptions.None);

        public static void Add(Toplevel top, Action quitAction, StatusBarOptions options)
            => Add(top, quitAction, options, false);

        public static void Add(Toplevel top, Action quitAction, bool isTopLevelWindow)
            => Add(top, quitAction, StatusBarOptions.None, isTopLevelWindow);

        public static void Add(Toplevel top, Action quitAction, StatusBarOptions options, bool isTopLevelWindow)
        {
            if (quitAction == null)
                quitAction = () => Application.RequestStop();

            List<StatusItem> statusItems = new List<StatusItem>();

            if (!options.HasFlag(StatusBarOptions.HideQuit))
                statusItems.Add(new StatusItem(Key.Q | Key.CtrlMask, "~Ctrl-Q~ " + (isTopLevelWindow ? "Exit Application" : "Close"), quitAction));

            if (!options.HasFlag(StatusBarOptions.HideAbout))
                statusItems.Add(new StatusItem(Key.F2, "~F2~ About", AboutView.Show));


            if (!options.HasFlag(StatusBarOptions.HideTapeInfo))
                statusItems.Add(new StatusItem (Key.CharMask, "Tape Drive: " + (SysInfo.IsTapeDrivePresent ? SysInfo.TapeDrive : "NOT Detected"), null));

            if (!options.HasFlag(StatusBarOptions.HideDiscInfo))
                statusItems.Add(new StatusItem (Key.CharMask, "Optical Drives: " + (OpticalDriveUtils.GetDriveNames().Length), null));

			if (!options.HasFlag(StatusBarOptions.HideTerminalDriver))
                statusItems.Add(new StatusItem (Key.CharMask, "GUI Driver: " + Application.Driver.GetType().Name, null));

            StatusBar statusBar = new StatusBar() 
            {
				Visible = true,
                // ColorScheme = GuiGlobals.Colors.StatusBar,
                Items = statusItems.ToArray()
			};

            top.Add(statusBar);
        }
    }
}



// {
//     // _quit = true;
//     // requestQuit();
//     Application.RequestStop();
    
//     // if (_runningScenario is null){
// 	// 	// This causes GetScenarioToRun to return null
// 	// 	_runningScenario = null;
// 		// Application.RequestStop();
// 	// } else {
// 	// 	_runningScenario.RequestStop();
// 	// }
// }));


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