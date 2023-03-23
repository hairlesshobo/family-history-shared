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
using System.Threading;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     An action that is optionally meant to be verifiable 
///     by the end user prior to execution
/// </summary>
public class VerifiableAction
{
    private Action<VerifiableAction, CancellationToken> _action;
    private readonly Guid _id;


    /// <summary>
    ///     Unique UUID of the verifiable action
    /// </summary>
    public Guid ID => _id;

    /// <summary>
    ///     Object on which the action will operate, may be null
    /// </summary>
    public object Target { get; private set; }

    /// <summary>
    ///     User-friendly description of the action
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    ///     Flag indicating if the action has aleady been executed
    /// </summary>
    public bool Executed { get; private set; } = false;

    /// <summary>
    ///     Flag indicating if the action has completed
    /// </summary>
    public bool Completed { get; private set; } = false;

    /// <summary>
    ///     If the action has been executed, this includes the DTM when the action
    ///     was started. Null if not yet started.
    /// </summary>
    public Nullable<DateTime> ExecutionStartDtm { get; private set; }

    /// <summary>
    ///     Once the action has completed, this is the number of miliseconds that
    ///     it took for the action to execute.
    /// </summary>
    /// <value></value>
    public long DurationMs { get; private set; } = -1;

    /// <summary>
    ///     Create a new verifiable action
    /// </summary>
    /// <param name="target">Object on which the action will operate. May be null.</param>
    /// <param name="description">User-friendly description of the action. May be null.</param>
    /// <param name="action">Action to take when Execute() is called</param>
    public VerifiableAction(object target, string description, Action<VerifiableAction, CancellationToken> action) : this()
    {
        this.Target = target;
        this.Description = description ?? String.Empty;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }


    private VerifiableAction()
    {
        _id = Guid.NewGuid();
    }

    /// <summary>
    ///     Execute the action
    /// </summary>
    /// <param name="ctk">Cancellation token used to abort execution</param>
    /// <returns>Task</returns>
    public async Task ExecuteAsync(CancellationToken ctk = default)
    {
        if (ctk == default)
            ctk = CancellationToken.None;

        if (this.Executed)
            return;

        this.Executed = true;
        this.ExecutionStartDtm = DateTime.UtcNow;

        Stopwatch sw = Stopwatch.StartNew();
        await Task.Run(() => _action(this, ctk), ctk);
        sw.Stop();

        this.Completed = true;
        this.DurationMs = sw.ElapsedMilliseconds;
    }

    /// <summary>
    ///     Execute the action synchronously
    /// </summary>
    /// <param name="ctk">Cancellation token used to abort execution</param>
    public void Execute(CancellationToken ctk = default)
        => this.ExecuteAsync(ctk).RunSynchronously();
}
