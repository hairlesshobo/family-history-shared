using System;
using System.Collections.Generic;
using System.IO;
using Archiver.Shared.Interfaces;
using Archiver.Shared.Utilities;

namespace Archiver.Shared.Models.Config
{
    public class ArchiverConfig : IValidatableConfig
    {
        public DiscConfig Disc { get; set; }
        public TapeConfig Tape { get; set; }
        public CsdConfig CSD { get; set; }
        public TapeServerConfig TapeServer { get; set; }

        /// <Summary>
        /// Path to the cdbxpmd.exe tool used to generate ISO files on Windows
        ///
        /// Note: ONLY needed on windows
        /// </Summary>
        public string CdbxpPath { get; set; } 
        
        /// <Summary>
        /// Path to the dd.exe tool used to validate discs Windows
        ///
        /// Note: ONLY needed on windows
        /// </Summary>
        public string DdPath { get; set; }

        public List<ValidationError> Validate(string prefix = null)
        {
            // TODO: 
            // In the future, find a way to specify whether loading config for archiver or
            // tape server, this way not all field will actually be required
            
            List<ValidationError> results = new List<ValidationError>();

            results.AddRange(Disc.Validate(ConfigUtils.BuildValidationPrefix(prefix, nameof(this.Disc))));
            results.AddRange(Tape.Validate(ConfigUtils.BuildValidationPrefix(prefix, nameof(this.Tape))));
            results.AddRange(CSD.Validate(ConfigUtils.BuildValidationPrefix(prefix, nameof(this.CSD))));
            results.AddRange(TapeServer.Validate(ConfigUtils.BuildValidationPrefix(prefix, nameof(this.TapeServer))));

            if (SysInfo.OSType == OSType.Windows)
            {
                // TODO: 
                // in the future, make this a warning. Still allow the app to run, don't allow the relevant
                // functionality to work on windows
                if (String.IsNullOrWhiteSpace(this.CdbxpPath))
                    results.AddValidationError(prefix, nameof(this.CdbxpPath), "Path to utility not provided");

                if (String.IsNullOrWhiteSpace(this.DdPath))
                    results.AddValidationError(prefix, nameof(this.DdPath), "Path to utility not provided");

                if (!File.Exists(this.CdbxpPath))
                    results.AddValidationError(prefix, nameof(this.CdbxpPath), $"The path does not exist: {this.CdbxpPath}");

                if (!File.Exists(this.DdPath))
                    results.AddValidationError(prefix, nameof(this.DdPath), $"The path does not exist: {this.DdPath}");
            }

            return results;
        }
    }
}