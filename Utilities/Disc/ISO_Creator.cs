using System;
using System.Diagnostics;
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
            string procArgs = $" --burn-data -folder[\\]:\"{Helpers.DirtyPath(_sourceDirectory)}\" -name:\"{_discName}\" -udf:2.5 -iso:\"{Helpers.DirtyPath(_destinationPath)}\"";

            Process process = new Process();
            process.StartInfo.FileName = Globals._cdbxpPath;
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