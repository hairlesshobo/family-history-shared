using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Classes;

public class ActionQueue
{
    private readonly List<VerifiableAction> _list;

    public ActionQueue()
    {
        _list = new List<VerifiableAction>();
    }

    public ActionQueue Add(VerifiableAction action)
    {
        _list.Add(action);
        return this;
    }

    public ActionQueue Add(object target, string description, Action<VerifiableAction, CancellationToken> action)
    {
        _list.Add(new VerifiableAction(target, description, action));
        return this;
    }

    public IEnumerable<VerifiableAction> Iterate()
    {
        foreach (var action in _list)
            yield return action;
    }

    public async Task ExecuteAll(CancellationToken ctk = default)
    {
        foreach (var action in this.Iterate())
            await action.ExecuteAsync(ctk);
    }
}