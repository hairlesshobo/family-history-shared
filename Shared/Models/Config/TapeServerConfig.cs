using System;
using System.Collections.Generic;
using Archiver.Shared.Interfaces;

namespace Archiver.Shared.Models.Config
{
    public class TapeServerConfig : IValidatableConfig
    {
        public string DrivePath { get; set; } = "auto";
        public string ListenAddress { get; set; } = "127.0.0.1";
        public TapeServerConfigPorts Ports { get; set; } = new TapeServerConfigPorts();

        public List<string> Validate(string prefix = null)
        {
            List<string> results = new List<string>();

            results.AddRange(Ports.Validate(prefix));

            return results;
        }
    }
}