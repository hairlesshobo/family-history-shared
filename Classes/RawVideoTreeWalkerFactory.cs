using System;

namespace FoxHollow.FHM.Shared.Classes
{
    /// <summary>
    ///     Factory class used with dependency injection
    /// </summary>
    public class RawVideoTreeWalkerFactory
    {
        private IServiceProvider _services;

        /// <summary>
        ///     DI Constructor
        /// </summary>
        /// <param name="services">DI service provider</param>
        public RawVideoTreeWalkerFactory(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        ///     Get a new instance of the <see cref="RawVideoTreeWalker" /> class
        /// </summary>
        /// <param name="rootDir">Root directory the tree walker will operate on</param>
        /// <returns>New <see cref="RawVideoTreeWalker" /> instance</returns>
        public RawVideoTreeWalker GetWalker(string rootDir)
            => new RawVideoTreeWalker(_services, rootDir);
    }
}