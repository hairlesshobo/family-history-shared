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
using System.Collections.Generic;

namespace FoxHollow.FHM.Shared.Models;

/// <summary>
///     Extensions for working with ValidationError class
/// </summary>
public static class ValidationErrorExtensions
{
    /// <summary>
    ///     Extension method for adding a new validation error to an existing list
    ///     of validation errors
    /// </summary>
    /// <param name="errorList">Existing list of validation errors</param>
    /// <param name="prefix">prefix</param>
    /// <param name="field">field</param>
    /// <param name="error">Error message</param>
    /// <returns>The list being operated on</returns>
    public static List<ValidationError> AddValidationError(this List<ValidationError> errorList, string prefix, string field, string error)
    {
        errorList.Add(new ValidationError()
        {
            Field = String.IsNullOrWhiteSpace(prefix) ? field : $"{prefix}.{field}",
            Error = error
        });

        return errorList;
    }
}

/// <summary>
///     Class that describes a validation error
/// </summary>
public class ValidationError
{
    /// <summary>
    ///     The field th error occurred on
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    ///     The error that occurred
    /// </summary>
    public string Error { get; set; }
}