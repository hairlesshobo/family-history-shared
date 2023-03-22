//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Interop;

/// <summary>
///     Generic class that describes an interop Python execution
/// </summary>
/// <typeparam name="TProgress">Type of progress message</typeparam>
/// <typeparam name="TResult">Type of result message</typeparam>
public abstract class PythonInterop<TProgress, TResult> : IDisposable
{
    /// <summary>
    ///     List of valid python commands
    /// </summary>
    private readonly string[] _knownCommands = new string[] { "identify-camera" };
    private Process _process;
    private IServiceProvider _services;
    protected ILogger _logger;


    /// <summary>
    ///     Command being executed
    /// </summary>
    public string Command { get; private set; }

    /// <summary>
    ///     Arguments to pass to Python command
    /// </summary>
    public string[] Arguments { get; private set; }

    /// <summary>
    ///     Deserialized result of Python command
    /// </summary>
    public TResult Result { get; private set; }

    /// <summary>
    ///     Delegate type that is executed whenever a progress change message is received from
    ///     the interop process
    /// </summary>
    /// <param name="processObj">Object describing the progress of the operatoin</param>
    public delegate void ProgressChangedDelegate(TProgress processObj);

    /// <summary>
    ///     Event tat is triggered when a process change message is received from the
    ///     interop process
    /// </summary>
    public event ProgressChangedDelegate OnProgressChanged;

    /// <summary>
    ///     Create a new interop instance
    /// </summary>
    /// <param name="services">DI services container</param>
    /// <param name="command">Command to execute</param>
    /// <param name="args">Arguments to pass to command</param>
    public PythonInterop(IServiceProvider services, string command, params string[] args)
    {
        if (!_knownCommands.Contains(command))
            throw new UnknownPythonCommandException(command);

        _services = services ?? throw new ArgumentNullException(nameof(services));

        this.Command = command;
        this.Arguments = args;

        this.OnProgressChanged += delegate { };
    }

    /// <summary>
    ///     Execute the interop commmand asynchronously
    /// </summary>
    /// <param name="ctk">Token used to cancel execution</param>
    /// <returns>Deserialized result object</returns>
    public abstract Task<TResult> RunAsync(CancellationToken ctk);

    /// <summary>
    ///     Start the interop process
    /// </summary>
    /// <param name="ctk">Token used to cancel execution</param>
    /// <returns>Deserialized result of execution</returns>
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

    /// <summary>
    ///     Clean up any handles or streams left open when execution completes
    /// </summary>
    public void Dispose()
    {
        if (_process != null)
            _process.Dispose();
    }
}