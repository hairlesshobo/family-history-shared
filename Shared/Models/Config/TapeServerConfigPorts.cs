using System;
using System.Collections.Generic;
using Archiver.Shared.Interfaces;

namespace Archiver.Shared.Models.Config
{
    public class TapeServerConfigPorts : IValidatableConfig
    {
        /// <summary>
        ///     UDP port used to send UDP broadcasts on. This is used so that the archive app can automatically
        ///     find the tape server on the network
        /// </summary>
        public int Broadcast { get; set; } = 34091; 

        /// <summary>
        ///     TCP port used to control the tape server
        /// </summary>
        public int Control { get; set; } = 34092; 

        /// <summary>
        ///     TCP port used to stream data to/from the tape
        /// </summary>
        public int Stream { get; set; } = 34093;

        public List<ValidationError> Validate(string prefix = null)
        {
            List<ValidationError> results = new List<ValidationError>();

            if (Broadcast <= 0)
                results.AddValidationError(prefix, nameof(Broadcast), "Port must be greater than 0");

            if (Control <= 0)
                results.AddValidationError(prefix, nameof(Control), "Port must be greater than 0");

            if (Stream <= 0)
                results.AddValidationError(prefix, nameof(Stream), "Port must be greater than 0");

            return results;
        }
    }
}