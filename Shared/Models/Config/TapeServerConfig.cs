using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Archiver.Shared.Interfaces;
using Archiver.Shared.Utilities;

namespace Archiver.Shared.Models.Config
{
    public class TapeServerConfig : IValidatableConfig
    {
        /// <Summary>
        ///     Tape drive path to use
        ///
        ///     Possible values:
        ///       auto: automatically use the first tape drive on the system
        ///       tapeX: Where X is the number of the tape drive. This will automatically map to the proper device depending on the operating system
        ///       /dev/nstX: Linux only
        ///       \\\\.\\TapeX: Windows only (you can also use //./TapeX form)
        /// </Summary>
        public string DrivePath { get; set; } = "auto";

        /// <Summary> 
        ///     Local IP address to listen on
        ///
        ///     In most cases, this should be set to 0.0.0.0. This should only change if you have multiple network
        ///     interfaces or plan to run the TapeServer on localhost (127.0.0.1) only
        ///
        ///     Default: 0.0.0.0
        /// </Summary>
        public string ListenAddress { get; set; } = "0.0.0.0";

        /// <Summary>
        ///     Configure ports for the tape server to communicate on
        /// </Summary>
        public TapeServerConfigPorts Ports { get; set; } 

        public List<ValidationError> Validate(string prefix = null)
        {
            this.DrivePath = TapeUtilsNew.CleanTapeDrivePath(this.DrivePath);

            List<ValidationError> results = new List<ValidationError>();

            bool drivePathValid = false;

            // Check for Linux
            if (!drivePathValid && SysInfo.OSType == OSType.Linux && Regex.Match(this.DrivePath, @"/dev/nst\d").Success)
                drivePathValid = true;

            // check for Windows
            if (!drivePathValid && SysInfo.OSType == OSType.Windows && Regex.Match(this.DrivePath, @"\\\\\.\\Tape\d").Success)
                drivePathValid = true;

            if (!drivePathValid)
            {
                if (SysInfo.OSType == OSType.Linux)
                    results.AddValidationError(prefix, nameof(ListenAddress), $"Invalid path provided: `{this.DrivePath}`. On Linux, the path must be either `auto` or in the form of `/dev/nstX`");
                if (SysInfo.OSType == OSType.Windows)
                    results.AddValidationError(prefix, nameof(ListenAddress), @$"Invalid path provided: `{this.DrivePath}`. On Windows, the path must be either `auto` or in the form of `\\.\TapeX`");
            }

            if (!IPAddress.TryParse(this.ListenAddress, out _))
                results.AddValidationError(prefix, nameof(ListenAddress), $"Invalid IP address provided: {this.ListenAddress}");

            if (this.Ports == null)
                results.AddValidationError(prefix, nameof(Ports), $"Object must be provided!");
            else
                results.AddRange(Ports.Validate(Utils.BuildValidationPrefix(prefix, nameof(this.Ports))));

            return results;
        }
    }
}