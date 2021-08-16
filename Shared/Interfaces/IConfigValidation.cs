using System;
using System.Collections.Generic;

namespace Archiver.Shared.Interfaces
{
    public interface IValidatableConfig
    {
        List<string> Validate(string prefix = null);
    }
}