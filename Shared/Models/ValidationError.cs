using System;
using System.Collections.Generic;

namespace Archiver.Shared.Models
{
    public static class Extensions
    {
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

    public class ValidationError
    {
        public string Field { get; set; }
        public string Error { get; set; }
    }
}