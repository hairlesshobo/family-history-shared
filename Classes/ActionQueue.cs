/*
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     An ActionQueue that is meant to contain multiple planned units of work, 
///     known as verifiable actions.
/// </summary>
public class ActionQueue
{
    private readonly List<VerifiableAction> _list;

    /// <summary>
    ///     If true, all actions in the queue have already been executed
    /// </summary>
    public bool Executed => _list.All(x => x.Executed);

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionQueue()
    {
        _list = new List<VerifiableAction>();
    }

    /// <summary>
    ///     Add a new VerifiableAction to the queue
    /// </summary>
    /// <param name="action">Action deinition to add to the queue</param>
    /// <returns>This ActionQueue instance</returns>
    public ActionQueue Add(VerifiableAction action)
    {
        _list.Add(action);
        return this;
    }

    /// <summary>
    ///     Create a new VerifiableAction and add it to the queue
    /// </summary>
    /// <param name="target">Target on which the action operates</param>
    /// <param name="description">Human readable description of the action</param>
    /// <param name="action">lambda action to be performed</param>
    /// <returns>This ActionQueue instance</returns>
    public ActionQueue Add(object target, string description, Action<VerifiableAction, CancellationToken> action)
    {
        _list.Add(new VerifiableAction(target, description, action));

        return this;
    }

    /// <summary>
    ///     Iterate through every action in the queue that has not yet been executed
    /// </summary>
    /// <returns>Enumerable containing actoins to execute</returns>
    public IEnumerable<VerifiableAction> Iterate()
    {
        foreach (var action in _list.Where(x => x.Executed == false))
            yield return action;
    }
    
    /// <summary>
    ///     Execute all actions in the queue
    /// </summary>
    /// <param name="ctk">Cancellation token used to abort execution</param>
    /// <returns>A task to be awaited during execution</returns>
    public async Task ExecuteAll(CancellationToken ctk = default)
    {
        foreach (var action in this.Iterate())
        {
            await action.ExecuteAsync(ctk);

            if (ctk.IsCancellationRequested)
                break;
        }
    }
}