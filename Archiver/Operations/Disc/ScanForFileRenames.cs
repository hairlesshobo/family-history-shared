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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archiver.Classes.Disc;
using Archiver.Shared.Utilities;
using Archiver.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.Disc
{
    public static class ScanForFileRenames
    {
        public static async Task StartOperationAsync()
        {
            DiscGlobals._destinationDiscs = await Helpers.ReadDiscIndexAsync();
            Console.Clear();

            Formatting.WriteLineC(ConsoleColor.Magenta, "Preparing...");
            Status.Initialize();

            DiscProcessing.IndexAndCountFiles();

            if (DiscGlobals._newlyFoundFiles > 0)
            {
                DiscProcessing.SizeFiles();
                
                List<DiscSourceFile> newFiles = DiscGlobals._newFileEntries;
                Dictionary<DiscSourceFile, DiscSourceFile> potentialRenames = new Dictionary<DiscSourceFile, DiscSourceFile>();

                long totalFileCount = newFiles.Count;
                long currentFileCount = 0;

                foreach (DiscSourceFile newFile in newFiles)
                {
                    currentFileCount += 1;

                    var existingFile = DiscGlobals._discSourceFiles.Where(x => x.Size == newFile.Size
                                                                            && x.Extension == newFile.Extension
                                                                            && x.CreationTimeUtc == newFile.CreationTimeUtc
                                                                            && x.FullPath != newFile.FullPath)
                                                                   .FirstOrDefault();

                    if (existingFile != null)
                        potentialRenames.Add(existingFile, newFile);

                    Status.RenameScanned(currentFileCount, totalFileCount, potentialRenames.Count);
                }

                long totalPotentialRenames = potentialRenames.Count;
                Status.RenameScanned(currentFileCount, totalFileCount, totalPotentialRenames, true);
                Console.WriteLine();
                Console.WriteLine();

                if (potentialRenames.Count > 0)
                {
                    currentFileCount = 0;
                    int renamedFiles = 0;

                    foreach (DiscSourceFile existingFile in potentialRenames.Keys)
                    {
                        currentFileCount += 1;

                        DiscSourceFile newFile = potentialRenames[existingFile];

                        if (ConfirmFileRename(currentFileCount, totalPotentialRenames, existingFile, newFile))
                        {
                            renamedFiles += 1;

                            if (existingFile.OriginalFile == null)
                                existingFile.OriginalFile = existingFile.CloneDiscPaths();

                            existingFile.Name = newFile.Name;
                            existingFile.FullPath = newFile.FullPath;
                            existingFile.RelativeDirectory = newFile.RelativeDirectory;
                            existingFile.RelativePath = newFile.RelativePath;

                            if (existingFile.OriginalFile.Name == existingFile.Name
                             && existingFile.OriginalFile.FullPath == existingFile.FullPath
                             && existingFile.RelativePath == existingFile.RelativePath
                             && existingFile.RelativeDirectory == existingFile.RelativeDirectory)
                                existingFile.OriginalFile = null;

                            Console.Write($"Saving {DiscFormatting.GetDiscName(existingFile.DestinationDisc)} ...");
                            existingFile.DestinationDisc.SaveToJson();
                            StatusHelpers.ClearLine();
                        }
                    }
                }
                else
                {
                    Status.ProcessComplete();

                    Console.WriteLine("No renamed files found. Nothing to do.");
                }
            }
            else
            {
                Status.ProcessComplete();

                Console.WriteLine("No new files found. Nothing to do.");
            }

            DiscGlobals.Reset();
        }

        private static int _confirmStartLine = -1;

        private static bool ConfirmFileRename(long currentCount, long totalCount, DiscSourceFile sourceFile, DiscSourceFile destFile)
        {
            if (_confirmStartLine < 0)
                _confirmStartLine = Console.CursorTop;

            Console.SetCursorPosition(0, _confirmStartLine);
            
            Console.WriteLine($"Potentially renamed file {currentCount} / {totalCount}");
            Console.WriteLine();

            StatusHelpers.ClearLine();
            Formatting.WriteC(ConsoleColor.Blue, "    Previous Path: ");
            Console.WriteLine(sourceFile.FullPath);
            
            StatusHelpers.ClearLine();
            Formatting.WriteC(ConsoleColor.Blue, "         New Path: ");
            Console.WriteLine(destFile.FullPath);

            StatusHelpers.ClearLine();
            Formatting.WriteC(ConsoleColor.Blue, "             Size: ");
            Console.WriteLine(Formatting.GetFriendlySize(sourceFile.Size));

            StatusHelpers.ClearLine();
            Formatting.WriteC(ConsoleColor.Blue, "    Creation Date: ");
            Console.WriteLine(sourceFile.CreationTimeUtc.ToLocalTime());

            StatusHelpers.ClearLine();
            Formatting.WriteC(ConsoleColor.Blue, "    Existing Disc: ");
            Console.WriteLine(DiscFormatting.GetDiscName(sourceFile.DestinationDisc));
            Console.WriteLine();

            StatusHelpers.ClearLine();
            Console.Write("Are the two files above the same file? (yes/");
            Formatting.WriteC(ConsoleColor.Blue, "NO");
            Console.Write(") ");

            Console.CursorVisible = true;
            Console.TreatControlCAsInput = false;
            string response = Console.ReadLine();
            Console.TreatControlCAsInput = true;
            Console.CursorVisible = false;

            bool doProcess = response.ToLower().StartsWith("yes");
            Console.WriteLine();

            return doProcess;
        }
    }
}