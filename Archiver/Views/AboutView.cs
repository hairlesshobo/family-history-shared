using System;
using Terminal.Gui;

namespace Archiver.Views
{
    public static class AboutView
    {
        public static void Show()
        {
            Button doneButton = new Button("Done", true);
            doneButton.HotKey = Key.Q | Key.CtrlMask;
            doneButton.Clicked += () => 
            {
                Application.RequestStop();
            };

            Dialog dialog = new Dialog("Archiver: About", 0, 0, doneButton) {
                Width = Dim.Percent(50),
                Height = Dim.Percent(50)
            };

            string aboutText = 
                    "    The ~~archiver~~ application is cross platform tool designed to handle all data " +
                "archival and backup needs using a variety of media types as backup media. \n" +
                "\n" +
                "Supported storage types: \n" +
                "    DVD or Blu-ray discs (Ideally, M-Disc media)\n" +
                "    Offline hard drives (solid state or platter)\n" +
                "    Tape drives (only LTO4 is tested, but should work with others)\n";


            Label label  = new Label(0, 1, aboutText);

            label.LayoutStyle = LayoutStyle.Computed;
            label.TextAlignment = TextAlignment.Left;
            label.X = 0;
            label.Y = 0;
            label.Width = Dim.Fill (2);
            label.AutoSize = true;
            //label.Height = Dim.Fill (1);

            dialog.Add (label);

            

            LineView line 
            Application.Run(dialog);
        }
    }
}