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
