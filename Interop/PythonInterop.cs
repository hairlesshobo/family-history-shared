using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.Shared.Interop
{
    public abstract class PythonInterop<TProgress, TResult> : IDisposable
    {
        private readonly string[] _knownCommands = new string[] { "identify-camera" };
        private Process _process;
        private IServiceProvider _services;
        protected ILogger _logger;


        public string Command { get; private set; }
        public string[] Arguments { get; private set; }
        public TResult Result { get; private set; }

        public delegate void ProgressChangedDelegate(TProgress processObj);

        public event ProgressChangedDelegate OnProgressChanged;

        public PythonInterop(IServiceProvider services, string command, params string[] args)
        {
            if (!_knownCommands.Contains(command))
                throw new UnknownCommandException(command);

            _services = services ?? throw new ArgumentNullException(nameof(services));

            this.Command = command;
            this.Arguments = args;

            this.OnProgressChanged += delegate { };
        }

        public abstract Task<TResult> RunAsync(CancellationToken ctk);

        internal async Task<TResult> RunInternalAsync(CancellationToken ctk)
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

            Task stderrTask = Task.Run(this.MonitorStderr);
            Task stdoutTask = Task.Run(this.MonitorStdout);

            await Task.WhenAll(stderrTask, stdoutTask);

            await _process.WaitForExitAsync();

            return this.Result;
        }

        private void MonitorStderr()
        {
            while (!_process.StandardError.EndOfStream)
            {
                string line = _process.StandardError.ReadLine();
                
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("DEBUG:"))
                    _logger.LogDebug(line.Substring(line.IndexOf(':', 6) + 1));
                else if (line.StartsWith("INFO:"))
                    _logger.LogInformation(line.Substring(line.IndexOf(':', 5) + 1));
                else if (line.StartsWith("WARNING:"))
                    _logger.LogWarning(line.Substring(line.IndexOf(':', 8) + 1));
                else if (line.StartsWith("ERROR:"))
                    _logger.LogError(line.Substring(line.IndexOf(':', 6) + 1));
                else if (line.StartsWith("CRITICAL:"))
                    _logger.LogCritical(line.Substring(line.IndexOf(':', 9) + 1));
                else
                    throw new Exception($"Unknown log level: {line.Substring(0, (line.Length < 10 ? line.Length : 10))}");
            }
        }

        private void MonitorStdout()
        {
            while (!_process.StandardOutput.EndOfStream)
            {
                string line = _process.StandardOutput.ReadLine();

                if (!String.IsNullOrWhiteSpace(line))
                {
                    var node = JsonSerializer.Deserialize<JsonNode>(line);

                    string msgType = node["type"]?.GetValue<string>();
                    string payloadJson = node["payload"]?.ToJsonString();

                    if (String.IsNullOrWhiteSpace(msgType) || String.IsNullOrWhiteSpace(payloadJson))
                        continue;

                    if (String.Equals(msgType, "progress"))
                        this.OnProgressChanged(JsonSerializer.Deserialize<TProgress>(payloadJson, Static.DefaultJso));

                    if (String.Equals(msgType, "result"))
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