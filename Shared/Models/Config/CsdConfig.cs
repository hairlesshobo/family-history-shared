using System;
using System.Collections.Generic;
using Archiver.Shared.Interfaces;

namespace Archiver.Shared.Models.Config
{
    public class CsdConfig : IValidatableConfig
    {
        // Amount of storage on CSD to reserve for index purposes
        // Default: 1 GiB
        public long ReservedCapacityBytes { get; set; } = 1073741824; 

        public string[] SourcePaths { get; set; }

        public string[] ExcludePaths { get; set; }
            
        public string[] ExcludeFiles { get; set; }


        /// <Summary>
        /// How often the indexes should be saved during archive operation
        ///
        /// Number provided is in seconds. This interval will be checked between every 
        /// file copy. If the timeout has expired, the index will be written prior to
        /// the next file being copied. 
        ///
        /// WARNING: Setting this value too low WILL decrease copy performance
        ///
        /// -1 = disable auto-save
        /// 0  = the index will be written out after every file copied (NOT RECOMMENDED)
        /// default = 300 (5 minutes)
        /// </Summary>
        public short AutoSaveInterval { get; set; } = 300;

        public List<ValidationError> Validate(string prefix = null)
        {
            List<ValidationError> results = new List<ValidationError>();

            return results;
        }
    }
    
}