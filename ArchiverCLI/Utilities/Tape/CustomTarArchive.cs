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
using System.IO;
using System.Security.Cryptography;
using FoxHollow.Archiver.Shared.Classes.Tape;
using ICSharpCode.SharpZipLib.Tar;

namespace FoxHollow.Archiver.CLI.Utilities.Tape
{
    public class CustomTarArchive : IDisposable
    {
        public TarOutputStream Stream
        {
            get
            {
                return tarOut;
            }
        }

        private TarOutputStream tarOut;
        private bool isDisposed;
        private byte[] localBuffer;

        public CustomTarArchive(TarOutputStream tarOutputStream)
        {
            tarOut = tarOutputStream;

            // we use a 4mb buffer for performance reasons
            localBuffer = new byte[1024 * 1024 * 4];
        }

        public void WriteDirectoryEntry(TarEntry sourceEntry)
        {
            sourceEntry.TrimLeadingFolder();
            var entry = (TarEntry)sourceEntry.Clone();

            tarOut.PutNextEntry(entry);
        }

        public void WriteFileEntry(TarEntry sourceEntry, TapeSourceFile sourceFile)
		{
			var entry = (TarEntry)sourceEntry.Clone();

			tarOut.PutNextEntry(entry);

			if (!entry.IsDirectory && sourceFile.Size > 0)
			{
                //using (Stream inputStream = File.OpenRead(sourceFile.FullPath))
                using (Stream inputStream = new FileStream(sourceFile.FullPath, FileMode.Open, FileAccess.Read))
                using (MD5 md5 = MD5.Create())
                {
                    while (true)
                    {
                        int numRead = inputStream.Read(localBuffer, 0, localBuffer.Length);

                        if (numRead <= 0)
                            break;

                        md5.TransformBlock(localBuffer, 0, numRead, localBuffer, 0);
                        tarOut.Write(localBuffer, 0, numRead);
                    }

                    md5.TransformFinalBlock(new byte[] { }, 0, 0);
                    sourceFile.Hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                    sourceFile.ArchiveTimeUtc = DateTime.UtcNow;
                }

				tarOut.CloseEntry();
			}
		}


        /// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
            localBuffer = new byte[] { };
            GC.Collect();
            GC.WaitForPendingFinalizers();
		}

        /// <summary>
		/// Releases the unmanaged resources used by the FileStream and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources;
		/// false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				isDisposed = true;
				if (disposing)
				{
					if (tarOut != null)
					{
						tarOut.Flush();
						tarOut.Dispose();
					}
				}
			}
		}

        /// <summary>
		/// Closes the archive and releases any associated resources.
		/// </summary>
		public virtual void Close()
		{
			Dispose(true);
		}

        /// <summary>
		/// Ensures that resources are freed and other cleanup operations are performed
		/// when the garbage collector reclaims the <see cref="TarArchive"/>.
		/// </summary>
		~CustomTarArchive()
		{
			Dispose(false);
		}
    }

    public static class TarExtensions
    {
        public static void TrimLeadingFolder(this TarEntry entry)
        {
            entry.Name = entry.Name.Substring(entry.Name.IndexOf('/')+1);
        }
    }
}