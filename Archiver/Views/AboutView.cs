// using System;
// using System.Data;
// using Archiver.Shared;
// using Archiver.ViewComponents;
// using Terminal.Gui;

// namespace Archiver.Views
// {
//     public static class AboutView
//     {
//         public static void Show()
//         {
//             Toplevel top = new Toplevel();
//             top.ColorScheme = Colors.Base;

//             // Button doneButton = new Button("Done", true);
//             // doneButton.HotKey = Key.Q | Key.CtrlMask;
//             // doneButton.Clicked += () =>
//             // {
//             //     Application.RequestStop();
//             // };

            
//             // dialog.AddButton(doneButton);

//             Window aboutWindow = new Window("Archiver: About")
//             {
//                 Width = Dim.Fill(),
//                 Height = 10
//                 // Height = Dim.Fill(1)
//             };

//             string aboutText =
//                 "    The ~~archiver~~ application is cross platform tool designed to handle all data " +
//                 "archival and backup needs using a variety of media types as backup media. \n" +
//                 "\n" +
//                 "Supported storage types: \n" +
//                 "    DVD or Blu-ray discs (Ideally, M-Disc media)\n" +
//                 "    Offline hard drives (solid state or platter)\n" +
//                 "    Tape drives (only LTO4 is tested, but should work with others)\n";


//             Label aboutLabel = new Label(0, 1, aboutText);

//             aboutLabel.LayoutStyle = LayoutStyle.Computed;
//             aboutLabel.TextAlignment = TextAlignment.Left;
//             aboutLabel.X = 0;
//             aboutLabel.Y = 0;
//             aboutLabel.Width = Dim.Fill(2);
//             aboutLabel.AutoSize = true;
//             //label.Height = Dim.Fill (1);

//             string sysInfoText = 
//             $"OS Platform: {SysInfo.OSType.ToString()} ({SysInfo.Architecture.ToString()})\n" +
//             $"Description: {SysInfo.Description}\n" +
//             $" Identifier: {SysInfo.Identifier}";


//             Label sysInfoLabel = new Label(0, 0, sysInfoText);
//             sysInfoLabel.LayoutStyle = LayoutStyle.Computed;
//             sysInfoLabel.TextAlignment = TextAlignment.Left;
//             sysInfoLabel.X = 0;
//             sysInfoLabel.Y = 0;
//             sysInfoLabel.AutoSize = true;


//             Window sysInfoWindow = new Window("System Information")
//             {
//                 // LayoutStyle = LayoutStyle.Absolute,
//                 Width = Dim.Fill(),
//                 Height = 5,
//                 X = 0,
//                 Y = Pos.Bottom(aboutWindow),
//                 Text = "meow!!"
//             };

            
//             aboutWindow.Add(aboutLabel);
//             sysInfoWindow.Add(sysInfoLabel);
//             // window.Add(line);
//             // window.Add(sysInfoLabel);

//             top.Add(aboutWindow);
//             top.Add(sysInfoWindow);
//             StatusBarComponent.Add(top, () => Application.RequestStop(), StatusBarOptions.HideAbout);

//             Application.Run(top);
//         }
//     }
// }