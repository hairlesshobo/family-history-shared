using System;

namespace Archiver.Shared.Models.Config
{
    public class TapeServerConfigPorts
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
        ///     TCP port used to stream data to the tape
        /// </summary>
        public int Stream { get; set; } = 34093; 

    }
}