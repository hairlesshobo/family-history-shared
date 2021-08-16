using System;
using System.Collections.Generic;
using Archiver.Shared.Interfaces;

namespace Archiver.Shared.Models.Config
{
    public class ArchiverConfig : IValidatableConfig
    {
        public DiscConfig Disc { get; set; }
        public TapeConfig Tape { get; set; }
        public CsdConfig CSD { get; set; }
        public TapeServerConfig TapeServer { get; set; }

        //": "./tools/cdbxp/cdbxpcmd.exe",
        public string CdbxpPath { get; set; } 
        
        //": "./tools/dd/dd.exe"
        public string DdPath { get; set; }

        public List<string> Validate(string prefix = null)
        {
            List<string> results = new List<string>();

            results.AddRange(Disc.Validate("Disc"));
            results.AddRange(Tape.Validate("Tape"));
            results.AddRange(CSD.Validate("CSD"));
            results.AddRange(TapeServer.Validate("TapeServer"));

            return results;
        }
    }
}