// using System;
// using System.Diagnostics;
// using Terminal.Gui;
// // using Mono.Terminal;

// namespace Archiver.Views
// {
//     public class SelectOutputPath
//     {
//         static bool Quit ()
//         {
//             var n = MessageBox.Query(50, 7, "Quit Demo", "Are you sure you want to quit this demo?", "Yes", "No");
//             return n == 0;
//         }

//         public SelectOutputPath()
//         {
//             TreeView tv = new TreeView();

//             Application.Init();

//             var top = Application.Top;
//             var tframe = top.Frame;

            

//             var win = new Window ("Hello"){
//                 X = 0,
//                 Y = 1,
//                 Width = Dim.Fill(),
//                 Height = Dim.Fill(1)
//             };					
//             var menu = new MenuBar (new MenuBarItem[] {
//                 new MenuBarItem("_File", new MenuItem[] {
//                     new MenuItem("_New", "Creates new file", null),
//                     new MenuItem("_Open", "", null),
//                     new MenuItem("_Close", "", null),
//                     new MenuItem("_Quit", "", () => { if (Quit ()) top.Running = false; }, null, null, Key.Q | Key.CtrlMask)
//                 }),
//                 new MenuBarItem("_Edit", new MenuItem[] {
//                     new MenuItem("_Copy", "", null),
//                     new MenuItem("C_ut", "", null),
//                     new MenuItem("_Paste", "", null)
//                 })
//             });

//             var statusBar = new StatusBar() {
// 				Visible = true,
// 			};
// 			statusBar.Items = new StatusItem[] {
// 				// _capslock,
// 				// _numlock,
// 				// _scrolllock,
// 				new StatusItem(Key.Q | Key.CtrlMask, "~CTRL-Q~ Quit", () => {
//                     // MessageBox.Query(10, 10, "meow", "meow!!!", "OK");
// 					// if (_runningScenario is null){
// 					// 	// This causes GetScenarioToRun to return null
// 					// 	_runningScenario = null;
// 						Application.RequestStop();
// 					// } else {
// 					// 	_runningScenario.RequestStop();
// 					// }
// 				}),
// 				new StatusItem(Key.F10, "~F10~ Hide/Show Status Bar", () => {
// 					statusBar.Visible = !statusBar.Visible;
//                     win.Height = Dim.Fill(statusBar.Visible ? 1 : 0);
// 					// _leftPane.Height = Dim.Fill(_statusBar.Visible ? 1 : 0);
// 					// _rightPane.Height = Dim.Fill(_statusBar.Visible ? 1 : 0);
//                     // top.LayoutSubviews();
// 					win.LayoutSubviews();
// 					win.SetChildNeedsDisplay();
//                     // top.SetChildNeedsDisplay();
// 				}),
// 				new StatusItem(Key.CharMask, Application.Driver.GetType().Name, null),
// 			};

//             win.Add(tv);

//             // ShowEntries (win);
//             // int count = 0;
//             // ml = new Label (new Rect (3, 17, 47, 1), "Mouse: ");
//             // Application.RootMouseEvent += delegate (MouseEvent me) {
//             // 	ml.Text = $"Mouse: ({me.X},{me.Y}) - {me.Flags} {count++}";
//             // };

//             // win.Add (ml);

//             top.Add(win);
//             top.Add(menu);
//             top.Add(statusBar);
//             Application.Run();
//         }
//     }
// }