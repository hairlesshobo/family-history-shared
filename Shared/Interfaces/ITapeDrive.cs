using System;
using Archiver.Shared.Classes;

namespace Archiver.Shared.Interfaces
{
    public interface ITapeDrive 
    {
        /**** Information ****/
        TapeDriveCapabilities GetCapabilities();
        TapeDriveDetails GetDriveDetails();
        TapeCartridgeDetails GetTapeInfo();

        /**** Drive Administration ****/
        void LoadTape();
        void RewindTape();
        void EjectTape();

        /**** Drive Configuration ****/
        void SetDriveCompression(bool newStatus);

        /**** High Level Tape IO Methods ****/
        void WriteText(string text, int filePosition);

        /**** Tape IO Methods ****/
        void WriteBlock(byte[] block);
        bool ReadBlock(byte[] buffer, Nullable<long> startPosition);
        bool ReadBlock(byte[] buffer);
        void WriteFilemark();


        /**** Tape Positioning Methods ****/
        void SeekToFilePosition(long fileNumber);
        void SeekToEndOfData();
        void SeekToBlock(long logicalBlock);
        long GetBlockPosition();
    }
}