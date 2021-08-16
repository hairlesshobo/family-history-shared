using System;

namespace Archiver.Shared.Models
{
    public class RemoteTapeDriveResponse
    {
        public bool Success { get; set; } = false;
        public string ErrorMessage { get; set; } = null;
        public GenericValue Value { get; set; } = null;
        public object Object { get; set; } = null;
    }
}