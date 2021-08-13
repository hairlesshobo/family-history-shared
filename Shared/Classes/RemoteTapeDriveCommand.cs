using System;

namespace Archiver.Shared.Classes
{
    public class RemoteTapeDriveCommand
    {
        public string CommandType { get; set; }
        public GenericValue Value { get; set; }

        public RemoteTapeDriveCommand GetDriveInfo()
            => new RemoteTapeDriveCommand() { CommandType = "GetDriveInfo" };

        public RemoteTapeDriveCommand GetTapeInfo()
            => new RemoteTapeDriveCommand() { CommandType = "GetTapeInfo" };

        public RemoteTapeDriveCommand PrepareDataWrite()
            => new RemoteTapeDriveCommand() { CommandType = "PrepareDataWrite" };
    }
}