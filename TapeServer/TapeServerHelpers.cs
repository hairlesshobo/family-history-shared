using System;
using Archiver.Shared;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Interfaces;
using Archiver.TapeServer.Classes.Config;
using Archiver.TapeServer.Providers;
using Microsoft.Extensions.Configuration;

namespace Archiver.TapeServer
{
    public static partial class TapeServerHelpers
    {
        internal static ITapeDrive GetTapeDrive(TapeServerConfig config)
        {
            string tapeDrive = config.TapeDrive;
            
            if (tapeDrive.ToLower().StartsWith("simulate-"))
            {
                string simulationType = tapeDrive.Substring(9).ToString();

                return new SimulatedTapeDrive(simulationType);
            }

            throw new TapeDriveNotFoundException(tapeDrive);
        }

        internal static TapeServerConfig ReadConfig()
        {
            IConfiguration _config = Utils.ReadConfig();

            TapeServerConfig config = new TapeServerConfig();
            _config.Bind("TapeServer", config);
            
            return config;
        }
    }
}