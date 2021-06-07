using System;
using System.Collections.Generic;
using System.Linq;
using Archiver.Classes.Disc;
using Archiver.Utilities;
using Archiver.Utilities.Disc;
using Archiver.Utilities.Shared;

namespace Archiver.Operations.Disc
{
    public static class ScanForFileRenames
    {
        public static void StartOperation()
        {
            DiscGlobals._destinationDiscs = Helpers.ReadDiscIndex();
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Preparing...");
            Console.ResetColor();
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

                            Console.Write($"Saving {Formatting.GetDiscName(existingFile.DestinationDisc)} ...");
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
            
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.WriteLine($"Potentially renamed file {currentCount} / {totalCount}");
            Console.WriteLine();

            StatusHelpers.ClearLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("    Previous Path: ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine(sourceFile.FullPath);
            
            StatusHelpers.ClearLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("         New Path: ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine(destFile.FullPath);

            StatusHelpers.ClearLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("             Size: ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine(Formatting.GetFriendlySize(sourceFile.Size));

            StatusHelpers.ClearLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("    Creation Date: ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine(sourceFile.CreationTimeUtc.ToLocalTime());

            StatusHelpers.ClearLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("    Existing Disc: ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine(Formatting.GetDiscName(sourceFile.DestinationDisc));
            Console.WriteLine();

            StatusHelpers.ClearLine();
            Console.Write("Are the two files above the same file? (yes/");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("NO");
            Console.ForegroundColor = originalColor;
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