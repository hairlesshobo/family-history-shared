using System;

namespace Archiver.Shared.Models.Config
{
    public class TapeServerConfig
    {
        public string TapeDrive { get; set; } = "auto";
        public string ListenAddress { get; set; } = "127.0.0.1";
        public TapeServerConfigPorts Ports { get; set; } = new TapeServerConfigPorts();
    }
}