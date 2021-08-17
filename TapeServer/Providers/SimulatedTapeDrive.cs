using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Archiver.Shared;
using Archiver.Shared.Models;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Interfaces;
using Archiver.Shared.Utilities;

namespace Archiver.TapeServer.Providers
{
    public class SimulatedTapeDrive : ITapeDrive
    {
        private SimulatedTapeSpecs _tapeSpecs;

        public SimulatedTapeDrive(string driveType)
        {
            string emulationJsonPath = Path.Combine(ConfigUtils.GetConfigDirectory(), "emulation.json");
            string emulationJson = File.ReadAllText(emulationJsonPath);
            List<SimulatedTapeSpecs> driveTypes = JsonSerializer.Deserialize<List<SimulatedTapeSpecs>>(emulationJson);

            SimulatedTapeSpecs specs = driveTypes.FirstOrDefault(x => x.DriveType.Equals(driveType, StringComparison.InvariantCultureIgnoreCase));

            if (specs == null)
                throw new TapeDriveNotFoundException($"simulated://{driveType}");

            _tapeSpecs = specs;
        }

        public void EjectTape()
        {
            throw new NotImplementedException();
        }

        public long GetBlockPosition()
        {
            throw new NotImplementedException();
        }

        public TapeDriveCapabilities GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public TapeDriveDetails GetDriveDetails()
        {
            throw new NotImplementedException();
        }

        public TapeCartridgeDetails GetTapeInfo()
        {
            return new TapeCartridgeDetails()
            {
                Capacity = _tapeSpecs.Capacity
            };
        }

        public void LoadTape()
        {
            throw new NotImplementedException();
        }

        public bool ReadBlock(byte[] buffer, long? startPosition)
        {
            throw new NotImplementedException();
        }

        public bool ReadBlock(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void RewindTape()
        {
            throw new NotImplementedException();
        }

        public void SeekToBlock(long logicalBlock)
        {
            throw new NotImplementedException();
        }

        public void SeekToEndOfData()
        {
            throw new NotImplementedException();
        }

        public void SeekToFilePosition(long fileNumber)
        {
            throw new NotImplementedException();
        }

        public void SetDriveCompression(bool newStatus)
        {
            throw new NotImplementedException();
        }

        public void WriteBlock(byte[] block)
        {
            throw new NotImplementedException();
        }

        public void WriteFilemark()
        {
            throw new NotImplementedException();
        }

        public void WriteText(string text, int filePosition)
        {
            throw new NotImplementedException();
        }

        public class SimulatedTapeSpecs
        {
            public string DriveType { get; set; }
            public long Capacity { get; set; }
        }
    }
}