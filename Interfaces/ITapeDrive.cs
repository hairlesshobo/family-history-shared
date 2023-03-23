//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared.Interfaces;

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