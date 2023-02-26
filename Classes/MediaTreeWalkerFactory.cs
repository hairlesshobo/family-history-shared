using System;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     Factory class used with dependency injection
/// </summary>
public class MediaTreeWalkerFactory
{
    private IServiceProvider _services;

    /// <summary>
    ///     DI Constructor
    /// </summary>
    /// <param name="services">DI service provider</param>
    public MediaTreeWalkerFactory(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    ///     Get a new instance of the <see cref="MediaTreeWalker" /> class
    /// </summary>
    /// <param name="rootDir">Root directory the tree walker will operate on</param>
    /// <returns>New <see cref="MediaTreeWalker" /> instance</returns>
    public MediaTreeWalker GetWalker(string rootDir)
        => new MediaTreeWalker(_services, rootDir);
}
