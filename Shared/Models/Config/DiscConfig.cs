using System;
using System.Collections.Generic;
using Archiver.Shared.Interfaces;

namespace Archiver.Shared.Models.Config
{
    public class DiscConfig : IValidatableConfig
    {
        public ulong CapacityLimit { get; set; } = 24928845824;

        public ulong DVDCapacityLimit { get; set; } = 4566155264;

        public string[] SourcePaths { get; set; }

        public string[] ExcludePaths { get; set; }
            
        public string[] ExcludeFiles { get; set; }
        
        public string StagingDir { get; set; } = "../";

        public List<string> Validate(string prefix = null)
        {
            List<string> results = new List<string>();

            return results;
        }
    }
}