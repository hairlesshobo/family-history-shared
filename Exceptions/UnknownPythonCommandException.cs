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

namespace FoxHollow.FHM.Shared.Exceptions;

/// <summary>
///     Exception that is thrown when an unknown command is passed to the Python Interop class
/// </summary>
public class UnknownPythonCommandException : Exception
{
    /// <summary>
    ///     Constructor that allows to specify the name of the invalid command
    /// </summary>
    /// <param name="command">Name of the command</param>
    public UnknownPythonCommandException(string command) : base($"Uknown interop command specified: {command}")
    { }
}