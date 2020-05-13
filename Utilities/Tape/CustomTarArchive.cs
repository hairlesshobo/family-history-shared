using System;
using System.IO;
using System.Security.Cryptography;
using Archiver.Classes.Tape;
using ICSharpCode.SharpZipLib.Tar;

namespace Archiver.Utilities.Tape
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

        public CustomTarArchive(TarOutputStream tarOutputStream)
        {
            tarOut = tarOutputStream;
        }

        public void WriteDirectoryEntry(TarEntry sourceEntry)
        {
            sourceEntry.TrimLeadingFolder();
            var entry = (TarEntry)sourceEntry.Clone();

            tarOut.PutNextEntry(entry);
        }

        public void WriteFileEntry(TarEntry sourceEntry, TapeSourceFile sourceFile)
		{
			string entryFilename = sourceEntry.File;

            sourceEntry.TrimLeadingFolder();

			var entry = (TarEntry)sourceEntry.Clone();

			//OnProgressMessageEvent(entry, null);

			tarOut.PutNextEntry(entry);

			if (!entry.IsDirectory)
			{
                using (Stream inputStream = File.OpenRead(entryFilename))
                using (MD5 md5 = MD5.Create())
                {
                    // we use a 1mb buffer for performance reasons
                    byte[] localBuffer = new byte[1024 * 1024];

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
			GC.SuppressFinalize(this);
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