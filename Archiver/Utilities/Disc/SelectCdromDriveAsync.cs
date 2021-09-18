using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Archiver.Shared.Models;
using Archiver.Shared.Utilities;
using TerminalUI;
using TerminalUI.Elements;
using TerminalUI.Types;

namespace Archiver.Utilities.Disc
{
    public static partial class DiscTasks
    {
        public static async Task<OpticalDrive> SelectCdromDriveAsync()
        {
            List<OpticalDrive> drives = OpticalDriveUtils.GetDrives();

            if (drives.Count == 0)
                throw new DriveNotFoundException("No optical drives were detected on this system!");
        
            if (drives.Count == 1)
                return drives[0];

            Terminal.Clear();
            Menu menu = new Menu();
            menu.EnableCancel = true;

            List<MenuEntry> entries = new List<MenuEntry>();

            foreach (OpticalDrive drive in drives)
            {
                MenuEntry newEntry = new MenuEntry();
                newEntry.Name = drive.Name + " (Disc Loaded: ";
                newEntry.SelectedValue = drive;

                if (drive.IsReady)
                    newEntry.Name += $"YES | Volume name: {drive.VolumeLabel} | Format: {drive.VolumeFormat})";
                else
                    newEntry.Name += "NO)";

                entries.Add(newEntry);
            }

            menu.SetMenuItems(entries);

            List<OpticalDrive> selectedDrives = await menu.ShowAsync<OpticalDrive>(); //was true

            if (selectedDrives == null)
                return null;
            
            return selectedDrives[0];
        }
    }
}