using System;
using System.Collections.Generic;
using System.IO;
using Archiver.Classes.Tape;
using ICSharpCode.SharpZipLib.Tar;

namespace Archiver.Utilities.Tape
{
    public class TapeTarWriter
    {
        private SlidingStream _bufferStream;
        private Stream _outputStream;
        private CustomTarArchive _archive;
        private TapeDetail _tapeDetail;
        private TarOutputStream _tarOutStream;
        private int _blockSize;
        private long _dataWrittenToTape = 0;

        public TapeTarWriter(TapeDetail tapeDetail, Stream outputStream, int BlockSize)
        {
            _tapeDetail = tapeDetail;
            _outputStream = outputStream;
            _blockSize = BlockSize;

            _bufferStream = new SlidingStream();
            _tarOutStream = new TarOutputStream(_bufferStream, Config.TapeBlockingFactor);
            _archive = new CustomTarArchive(_tarOutStream);
        }

        public void StartCreatingTar()
        {
            ArchiveTapeSourceFiles(_tapeDetail.Files);

            ArchiveTapeSourceDirectory(_tapeDetail.Directories);
            _tarOutStream.Close();

            _bufferStream.InputStreamComplete();
        }

        public void StartWriting()
        {
            byte[] buffer = new byte[_blockSize];

            while (_bufferStream.EndOfStream == false)
            {
                _bufferStream.Read(buffer, 0, _blockSize);
                _outputStream.Write(buffer, 0, _blockSize);

                _dataWrittenToTape += buffer.Length;
            }
        }

        private void ArchiveTapeSourceDirectory(IEnumerable<TapeSourceDirectory> directories)
        {
            foreach (TapeSourceDirectory dir in directories)
            {
                ArchiveTapeSourceDirectory(dir);
            }
        }

        private void ArchiveTapeSourceDirectory(TapeSourceDirectory directory)
        {
            TarEntry dirEntry = TarEntry.CreateEntryFromFile(directory.FullPath);
            _archive.WriteDirectoryEntry(dirEntry);

            ArchiveTapeSourceDirectory(directory.Directories);
            ArchiveTapeSourceFiles(directory.Files);
        }

        private void ArchiveTapeSourceFiles(IEnumerable<TapeSourceFile> files)
        {
            foreach (TapeSourceFile file in files)
            {
                TarEntry entry = TarEntry.CreateEntryFromFile(file.FullPath);
                _archive.WriteFileEntry(entry, file);
            }
        }
    }
}