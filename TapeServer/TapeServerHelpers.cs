using System;
using Archiver.Shared;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Interfaces;
using Archiver.Shared.Models.Config;
using Archiver.Shared.Utilities;
using Archiver.TapeServer.Providers;
using Microsoft.Extensions.Configuration;

namespace Archiver.TapeServer
{
    public static partial class TapeServerHelpers
    {
        internal static ITapeDrive GetTapeDrive(TapeServerConfig config)
        {
            string tapeDrive = config.DrivePath;
            
            if (tapeDrive.ToLower().StartsWith("simulate-"))
            {
                string simulationType = tapeDrive.Substring(9).ToString();

                return new SimulatedTapeDrive(simulationType);
            }

            throw new TapeDriveNotFoundException(tapeDrive);
        }
    }
}