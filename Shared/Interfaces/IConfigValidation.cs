using System;
using System.Collections.Generic;
using Archiver.Shared.Models;

namespace Archiver.Shared.Interfaces
{
    public interface IValidatableConfig
    {
        List<ValidationError> Validate(string prefix = null);
    }
}