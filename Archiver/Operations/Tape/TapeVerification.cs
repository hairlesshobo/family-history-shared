/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Linq;
using System.Threading;
using FoxHollow.Archiver.Shared.Classes.Tape;
using FoxHollow.Archiver.Shared.Utilities;
using FoxHollow.Archiver.Utilities.Shared;
using FoxHollow.Archiver.Utilities.Tape;

namespace FoxHollow.Archiver.Operations.Tape
{
    public static class TapeVerification
    {
        public static void StartOperation()
        {
            Console.WriteLine("Reading tape information...");
            Console.WriteLine();

            if (TapeUtils.IsTapeLoaded() == false)
            {
                Formatting.WriteC(ConsoleColor.Red, "ERROR: ");
                Console.WriteLine("No tape is present in the drive, please insert tape and run this operation again.");
                Console.ReadLine();
            }
            else
            {
                if (TapeUtils.TapeHasJsonRecord() == false)
                {
                    Formatting.WriteC(ConsoleColor.Red, "ERROR: ");
                    Console.WriteLine("This tape does not contain a summary json record and therefore cannot be verified. Please insert a different tape and run this operation again.");
                    Console.ReadLine();
                }
                else
                {
                    TapeSummary tapeSummary = TapeUtils.ReadTapeSummaryFromTape();

                    TapeDetail tapeDetail = TapeUtils.GetTapeDetail(tapeSummary.ID);
                    Console.Clear();

                    Console.WriteLine("Tape Overview");
                    Console.WriteLine("========================================================");
                    Console.WriteLine("          Tape ID: " + tapeDetail.ID);
                    Console.WriteLine("        Tape Name: " + tapeDetail.Name);
                    Console.WriteLine("  Tape Write Date: " + tapeDetail.WriteDTM.ToLocalTime().ToString());
                    Console.WriteLine();
                    Console.WriteLine("        Data Size: " + Formatting.GetFriendlySize(tapeDetail.DataSizeBytes));
                    Console.WriteLine("  Directory Count: " + tapeDetail.FlattenDirectories().Count().ToString("N0"));
                    Console.WriteLine("       File Count: " + tapeDetail.FlattenFiles().Count().ToString("N0"));
                    Console.WriteLine();
                    Console.WriteLine("     Archive Size: " + Formatting.GetFriendlySize(tapeDetail.TotalArchiveBytes));
                    Console.WriteLine("     Size on Tape: " + Formatting.GetFriendlySize(tapeDetail.ArchiveBytesOnTape));
                    Console.WriteLine("Compression Ratio: " + tapeDetail.CompressionRatio.ToString("0.00") + ":1");
                    Console.WriteLine();
                    
                    MD5_Tape md5 = new MD5_Tape();
                    md5.OnProgressChanged += (progress) => {
                        Console.CursorLeft = 0;

                        string left = "           Verify: ";
                        left += Formatting.GetFriendlySize(progress.TotalBytesProcessed).PadLeft(10);
                        left += " ";
                        left += $"[{Formatting.GetFriendlyTransferRate(progress.InstantRate).PadLeft(12)}]";
                        left += " ";
                        left += $"[{Formatting.GetFriendlyTransferRate(progress.AverageRate).PadLeft(12)}]";

                        Console.Write(left + StatusHelpers.GeneratePercentBar(Console.BufferWidth, left.Length, 0, progress.PercentCompleted * 100.0, (progress.PercentCompleted == 1.0)));
                    };

                    Thread thread = new Thread(md5.GenerateHash);
                    thread.Start();
                    thread.Join();

                    TapeVerificationResult result = new TapeVerificationResult()
                    {
                        TapeValid = md5.MD5_Hash == tapeDetail.Hash,
                        VerificationDTM = DateTime.UtcNow
                    };

                    StatusHelpers.ClearLine();
                    Console.Write("           Verify: ");

                    if (result.TapeValid)
                        Formatting.WriteLineC(ConsoleColor.Green, "Data Valid!");
                    else
                        Formatting.WriteLineC(ConsoleColor.Red, "FAILED, data be corrupted!");

                    Console.ResetColor();

                    Console.WriteLine();
                    Console.Write("Saving tape to index... ");
                    tapeDetail.Verifications.Add(result);
                    tapeDetail.SaveToIndex();
                    Console.WriteLine("done");

                    Console.Write("Rewinding tape... ");
                    TapeUtils.RewindTape();
                    Console.WriteLine("done");
                }
            }
        }
    }
}