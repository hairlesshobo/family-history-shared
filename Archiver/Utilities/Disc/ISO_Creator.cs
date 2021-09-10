/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Diagnostics;
using Archiver.Shared;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;

namespace Archiver.Utilities.Disc
{
    public delegate void ISO_ProgressChangedDelegate(int currentPercent);
    public delegate void ISO_CompleteDelegate();

    public class ISO_Creator
    {
        public event ISO_CompleteDelegate OnComplete;
        public event ISO_ProgressChangedDelegate OnProgressChanged;

        private string _discName;
        private string _sourceDirectory;
        private string _destinationPath;
        private const int _sampleDurationMs = 100;

        public ISO_Creator(string discName, string sourceDirectory, string destinationPath)
        {
            _discName = discName;
            _sourceDirectory = sourceDirectory;
            _destinationPath = destinationPath;

            this.OnComplete += delegate { };
            this.OnProgressChanged += delegate { };
        }

        public void CreateISO()
        {
            string procArgs = $" --burn-data -folder[\\]:\"{PathUtils.DirtyPath(_sourceDirectory)}\" -name:\"{_discName}\" -udf:2.5 -iso:\"{PathUtils.DirtyPath(_destinationPath)}\"";

            Process process = new Process();
            process.StartInfo.FileName = SysInfo.Config.CdbxpPath;
            process.StartInfo.Arguments = procArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            while (process.HasExited == false)
            {
                string output = process.StandardOutput.ReadLine();

                if (output != null && output.EndsWith("%"))
                {
                    output = output.TrimEnd('%');

                    int currentPercent = Int32.Parse(output);

                    this.OnProgressChanged(currentPercent);
                }
            }

            process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            this.OnComplete();
        }
    }
}