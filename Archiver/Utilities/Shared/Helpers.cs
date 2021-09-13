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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared;
using Archiver.Shared.Classes.CSD;
using Archiver.Shared.Classes.Disc;
using Archiver.Shared.Classes.Tape;
using Archiver.Shared.Interfaces;
using Archiver.Shared.Models;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Disc;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Utilities.Shared
{
    public class Helpers
    {

        internal static Task<List<TapeDetail>> ReadTapeIndexAsync()
            => ReadMediaIndexAsync<TapeDetail>("tape");

        internal static Task<List<DiscDetail>> ReadDiscIndexAsync()
            => ReadMediaIndexAsync<DiscDetail>("disc");

        internal static Task<List<CsdDetail>> ReadCsdIndexAsync()
            => ReadMediaIndexAsync<CsdDetail>("csd");


        private static Task<List<TMedia>> ReadMediaIndexAsync<TMedia>(string mediaType)
            where TMedia : IMediaDetail, new()
        {
            string[] validMediaTypes = new string[] { "disc", "tape", "csd" };

            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException($"'{nameof(mediaType)}' cannot be null or whitespace.", nameof(mediaType));

            mediaType = mediaType.ToLower();

            if (!validMediaTypes.Contains(mediaType))
                throw new ArgumentException($"Invalid media type specified: {mediaType}. Valid Options: {String.Join(", ", validMediaTypes)}", nameof(mediaType));

            string mediaTypeTc = mediaType.Substring(0, 1).ToUpper() + mediaType.Substring(1);

            CancellationTokenSource cts = new CancellationTokenSource();

            
            Terminal.Clear();
            Terminal.Header.UpdateLeft($"Read {mediaTypeTc} Index...");
            Terminal.StatusBar.ShowItems(
                new StatusBarItem(
                    "Cancel",
                    (key) => {
                        cts.Cancel();
                        return Task.CompletedTask;
                    },
                    Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                )
            );

            Text text = new Text();
            Terminal.NextLine();
            ProgressBar progress = new ProgressBar();
            Terminal.NextLine();

            progress.Show();
            text.Show();

            return HelpersNew.ReadMediaIndexAsync<TMedia>(mediaType, cts.Token, (currentFile, totalFiles) => 
            {
                double currentPct = (double)currentFile / (double)totalFiles;

                progress.UpdateProgress(currentPct);

                text.UpdateValue($"Reading {mediaType} index files... {currentFile.ToString().PadLeft(totalFiles.ToString().Length)}/{totalFiles}");
            });
        }
        

        public static void CreateIndexIso()
        {
            Console.WriteLine();
            Formatting.WriteC(ConsoleColor.Magenta, "Creating index iso file...");
            Console.WriteLine();

            string isoPath = PathUtils.CleanPathCombine(SysInfo.Directories.ISO, "index.iso");
            string isoName = "Archive Index";

            ISO_Creator creator = new ISO_Creator(isoName, PathUtils.DirtyPath(SysInfo.Directories.Index), isoPath);

            creator.OnProgressChanged += (currentPercent) => {
                string line = StatusHelpers.GeneratePercentBar(Console.WindowWidth, 0, 0, currentPercent, (currentPercent == 100));
                Console.CursorLeft = 0;
                Console.Write(line);
            };

            creator.OnComplete += () => {
                string line = StatusHelpers.GeneratePercentBar(Console.WindowWidth, 0, 0, 100, true);
                Console.CursorLeft = 0;
                Console.Write(line);
            };

            Thread isoThread = new Thread(creator.CreateISO);
            isoThread.Start();
            isoThread.Join();

            Console.CursorLeft = 0;
            Console.CursorTop = Console.CursorTop+2;
        }
    }
}