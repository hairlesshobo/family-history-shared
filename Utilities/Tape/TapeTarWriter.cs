using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Archiver.Classes.Tape;
using ICSharpCode.SharpZipLib.Tar;

namespace Archiver.Utilities.Tape
{
    public delegate void TapeTarCompleteDelegate(string hash);
    public delegate void TapeTarProgressDelegate(TapeTarWriterProgress progress);

    public enum TapeTarWriterStatus
    {
        Idle = 0,
        Active = 1,
        Complete = 2
    }

    public class TapeTarWriterProgress
    {
        public double TotalBytes { get; set; }

        public double TapeWritePercent { 
            get
            {
                return ((double)this.TapeBytesWritten / (double)this.TotalBytes) * 100.0;
            }
        }
        public long TapeBytesWritten { get; set; }
        public double TapeInstantTransferRate { get; set; }
        public double TapeAverageTransferRate { get; set; }
        public TapeTarWriterStatus TapeStatus { get; set; }

        public double TarWritePercent { 
            get
            {
                return ((double)this.TarBytesWritten / (double)this.TotalBytes) * 100.0;
            }
        }
        public long TarBytesWritten { get; set; }
        public long TarTotalFiles { get; set; }
        public double TarInstantTransferRate { get; set; }
        public double TarAverageTransferRate { get; set; }
        public long TarCurrentFileCount { get; set; }
        public long TarCurrentFileSizeBytes { get; set; }
        public string TarCurrentFileName { get; set; }
        public TapeTarWriterStatus TarStatus { get; set; }

        public double BufferPercent { get; set; }
        public long BufferFilledBytes { get; set; }
        public long BufferCapacityBytes { get; set; }

        public TimeSpan Elapsed { get; set; }

    }

    public class TapeTarWriter
    {
        private const int _updateMilliseconds = 500;
        private TapeBuffer _tapeBuffer;
        private TapeOperator _tape;
        private CustomTarArchive _archive;
        private TapeDetail _tapeDetail;
        private TarOutputStream _tarOutStream;
        private long _dataWrittenToTape = 0;
        private readonly object _lastFileReadLock = new object();
        private string _lastFileRead = String.Empty;
        private long _lastFileSize = 0;
        private volatile int _tarFileCount = 0;

        private readonly uint _blockCount;
        private readonly uint _blockSize;

        public event TapeTarProgressDelegate OnProgressChanged;
        public event TapeTarCompleteDelegate OnTapeComplete;

        public TapeTarWriter(TapeDetail tapeDetail, TapeOperator tape, uint BlockSize, uint BufferBlockCount)
        {
            _tapeDetail = tapeDetail;
            _tape = tape;
            _blockSize = BlockSize;
            _blockCount = BufferBlockCount;

            // TODO: move buffer count to config
            _tapeBuffer = new TapeBuffer(_blockSize, _blockCount);
            _tarOutStream = new TarOutputStream(_tapeBuffer, Config.TapeBlockingFactor);
            _archive = new CustomTarArchive(_tarOutStream);

            this.OnProgressChanged += delegate {};
            this.OnTapeComplete += delegate {};
        }

        public void Start()
        {
            Thread tarThread = new Thread(this.StartCreatingTar);
            Thread writeThread = new Thread(this.StartWriting);

            tarThread.Start();
            writeThread.Start();

            Stopwatch sw = Stopwatch.StartNew();

            long lastUpdate = -1 * _updateMilliseconds;
            TapeTarWriterProgress progress = new TapeTarWriterProgress()
            {
                TotalBytes = _tapeDetail.TotalArchiveBytes,
                TarTotalFiles = _tapeDetail.FileCount
            };

            long tarLastBytes = 0;
            long tapeLastBytes = 0;
            long tapeSampleCount = 0;
            long tarSampleCount = 0;

            // we loop here until both threads complete, that way we can
            // send out status updates while the threads are running
            while (tarThread.IsAlive || writeThread.IsAlive)
            {
                if ((sw.ElapsedMilliseconds - lastUpdate) > _updateMilliseconds)
                {
                    progress.TarBytesWritten = _tapeBuffer.BytesWritten;
                    progress.TapeBytesWritten = _tapeBuffer.BytesRead;
                    progress.BufferPercent = _tapeBuffer.CurrentBufferPercent;
                    progress.BufferCapacityBytes = (uint)(512 * _tapeDetail.BlockingFactor);
                    progress.BufferFilledBytes = _tapeBuffer.BlocksFull * _blockSize;

                    if (_tapeBuffer.InputComplete)
                        progress.TarStatus = TapeTarWriterStatus.Complete;
                    else
                        progress.TarStatus = (_tapeBuffer.CanWrite ? TapeTarWriterStatus.Active : TapeTarWriterStatus.Idle);

                    progress.TapeStatus = (_tapeBuffer.CanRead ? TapeTarWriterStatus.Active : TapeTarWriterStatus.Idle);

                    if (progress.TarStatus == TapeTarWriterStatus.Active)
                    {
                        tarSampleCount++;
                        
                        // calculate the tar transfer rates
                        long tarBytesSinceLastUpdate = progress.TarBytesWritten - tarLastBytes;
                        double tarTimeSinceLastUpdate = (double)(sw.ElapsedMilliseconds - lastUpdate) / 1000.0;
                        tarLastBytes = progress.TarBytesWritten;

                        progress.TarInstantTransferRate = (double)tarBytesSinceLastUpdate / tarTimeSinceLastUpdate;;

                        if (tarSampleCount == 1)
                            progress.TarAverageTransferRate = progress.TarInstantTransferRate;
                        else
                            progress.TarAverageTransferRate = progress.TarAverageTransferRate + (progress.TarInstantTransferRate - progress.TarAverageTransferRate) / tarSampleCount;
                    }
                    else
                        progress.TarInstantTransferRate = 0.0;

                    if (progress.TapeStatus == TapeTarWriterStatus.Active)
                    {
                        tapeSampleCount++;

                        // calculate the tape transfer rates
                        long tapeBytesSinceLastUpdate = progress.TapeBytesWritten - tapeLastBytes;
                        double tapeTimeSinceLastUpdate = (double)(sw.ElapsedMilliseconds - lastUpdate) / 1000.0;
                        tapeLastBytes = progress.TapeBytesWritten;

                        progress.TapeInstantTransferRate = (double)tapeBytesSinceLastUpdate / tapeTimeSinceLastUpdate;;

                        if (tapeSampleCount == 1)
                            progress.TapeAverageTransferRate = progress.TapeInstantTransferRate;
                        else
                            progress.TapeAverageTransferRate = progress.TapeAverageTransferRate + (progress.TapeInstantTransferRate - progress.TapeAverageTransferRate) / tapeSampleCount;
                    }
                    else
                        progress.TapeInstantTransferRate = 0.0;

                    lock (_lastFileReadLock)
                    {
                        progress.TarCurrentFileCount = _tarFileCount;

                        if (progress.TarStatus == TapeTarWriterStatus.Complete)
                        {
                            progress.TarCurrentFileName = String.Empty;
                            progress.TarCurrentFileSizeBytes = 0;
                        }
                        else
                        {
                            progress.TarCurrentFileName = _lastFileRead;
                            progress.TarCurrentFileSizeBytes = _lastFileSize;
                        }
                    }

                    progress.Elapsed = sw.Elapsed;

                    OnProgressChanged(progress);

                    lastUpdate = sw.ElapsedMilliseconds;
                }
            }

            sw.Stop();

            progress.Elapsed = sw.Elapsed;
            progress.TapeInstantTransferRate = 0.0;
            progress.TarInstantTransferRate = 0.0;
            progress.BufferPercent = _tapeBuffer.CurrentBufferPercent;
            progress.BufferFilledBytes = _tapeBuffer.BlocksFull * _blockSize;
            progress.TarCurrentFileName = String.Empty;
            progress.TarCurrentFileSizeBytes = 0;
            progress.TapeStatus = TapeTarWriterStatus.Complete;
            progress.TarStatus = TapeTarWriterStatus.Complete;
            progress.TarBytesWritten = _tapeBuffer.BytesWritten;
            progress.TapeBytesWritten = _tapeBuffer.BytesRead;

            OnProgressChanged(progress);
        }

        private void StartCreatingTar()
        {
            ArchiveTapeSourceFiles(_tapeDetail.Files);

            ArchiveTapeSourceDirectory(_tapeDetail.Directories);
            _tarOutStream.Close();

            _tapeBuffer.InputStreamComplete();
        }

        private void StartWriting()
        {
            byte[] buffer = new byte[_blockSize];

            using (MD5 md5 = MD5.Create())
            {
                while (_tapeBuffer.EndOfStream == false)
                {
                    _tapeBuffer.ReadBlock(buffer);
                    _tape.Write(buffer);
                    md5.TransformBlock(buffer, 0, buffer.Length, buffer, 0);

                    _dataWrittenToTape += buffer.Length;
                }

                md5.TransformFinalBlock(new byte[] { }, 0, 0);

                string hash = BitConverter.ToString(md5.Hash).Replace("-","").ToLower();
                _tapeDetail.Hash = hash;

                OnTapeComplete(hash);
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
                lock(_lastFileReadLock)
                {
                    _lastFileRead = file.Name;
                    _lastFileSize = file.Size;
                    _tarFileCount++;
                }

                TarEntry entry = TarEntry.CreateEntryFromFile(file.FullPath);
                _archive.WriteFileEntry(entry, file);
            }
        }
    }
}