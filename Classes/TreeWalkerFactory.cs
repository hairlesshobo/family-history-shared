using System;

namespace FoxHollow.FHM.Shared.Classes
{
    /// <summary>
    ///     Factory class used with dependency injection
    /// </summary>
    public class TreeWalkerFactory
    {
        private IServiceProvider _services;

        /// <summary>
        ///     DI Constructor
        /// </summary>
        /// <param name="services">DI service provider</param>
        public TreeWalkerFactory(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        ///     Get a new instance of the <see cref="TreeWalker" /> class
        /// </summary>
        /// <param name="rootDir">Root directory the tree walker will operate on</param>
        /// <returns>New <see cref="TreeWalker" /> instance</returns>
        public TreeWalker GetWalker(string rootDir)
            => new TreeWalker(_services, rootDir);
    }
}