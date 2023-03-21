using System;

namespace FoxHollow.FHM.Shared;

public interface IVersionedMetadata
{
    int Version { get; }
    bool Upgradeable { get; }
    
    object Upgrade();
}