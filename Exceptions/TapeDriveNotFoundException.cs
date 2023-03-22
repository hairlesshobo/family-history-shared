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

namespace FoxHollow.FHM.Shared.Exceptions;

/// <summary>
///     Exception that occurs when an attempt is made to access a non-existant
///     tape drive
/// </summary>
public class TapeDriveNotFoundException : Exception
{
    /// <summary>
    ///     Constructor that accepts a tape drive
    /// </summary>
    /// <param name="drive">Tape drive that does not exist</param>
    public TapeDriveNotFoundException(string drive) : base($"Unable to find the following tape drive: {drive}")
    {

    }
}