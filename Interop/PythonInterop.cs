using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Interop
{
    public class PythonInterop<TProgress, TResult> : IDisposable
    {
        private readonly string[] _knownCommands = new string[] { "identify-camera" };
        private Process _process;


        public string Command { get; private set; }
        public string[] Arguments { get; private set; }
        public TResult Result { get; private set; }

        public delegate void ProgressChangedDelegate(TProgress processObj);

        public event ProgressChangedDelegate OnProgressChanged;

        public PythonInterop(string command, params string[] args)
        {
            if (!_knownCommands.Contains(command))
                throw new UnknownCommandException(command);

            this.Command = command;
            this.Arguments = args;

            this.OnProgressChanged += delegate { };
        }

        public async Task<TResult> RunAsync(CancellationToken ctk)
        {
            if (_process != null)
                throw new InvalidOperationException("Interop process already started");

            _process = new Process();
        
            _process.StartInfo.FileName = "python3";
            _process.StartInfo.ArgumentList.Add(Path.Combine(SysInfo.PythonRoot, "fhm.py"));
            _process.StartInfo.ArgumentList.Add(this.Command);

            foreach (string arg in this.Arguments)
                _process.StartInfo.ArgumentList.Add(arg);
            
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.Start();

            Task stderrTask = Task.Run(() => Task.Delay(1000));
            Task stdoutTask = Task.Run(this.MonitorStdout);

            await Task.WhenAll(stderrTask, stdoutTask);

            await _process.WaitForExitAsync();

            return this.Result;
        }

        private void MonitorStdout()
        {
            while (!_process.StandardOutput.EndOfStream)
            {
                string line = _process.StandardOutput.ReadLine();

                if (!String.IsNullOrWhiteSpace(line))
                {
                    var node = JsonSerializer.Deserialize<JsonNode>(line);

                    if (String.Equals(node["type"]?.GetValue<string>(), "result"))
                        this.Result = JsonSerializer.Deserialize<TResult>(node["payload"].ToJsonString(), Static.DefaultJso);
                }
            }
        }

        public void Dispose()
        {
            if (_process != null)
                _process.Dispose();
        }
    }
}