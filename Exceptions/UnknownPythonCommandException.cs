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