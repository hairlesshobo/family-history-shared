using System;

namespace Archiver.Shared.Native
{
    public static partial class Linux
    {
        #pragma warning disable CS0414

        public enum OpenType
        {
            ReadOnly = 0,
            WriteOnly = 1,
            ReadWrite = 2
        }

        public const uint O_RDONLY = 0x00000000;
        public const uint O_WRONLY = 0x00000001;
        public const uint O_RDWR = 0x00000002;
        public const uint O_NONBLOCK = 0x00000800; // non blocking open


        /* CDROM Options */
        public const uint CDO_AUTO_CLOSE = 0x00000001; // don't auto close

        /* IOCTL calls */
        public const uint MTIOCTOP = 0x40086D01; // const struct mtop *
        public const uint MTIOCGET = 0x801C6D02; // struct mtget *
        public const uint MTIOCPOS = 0x80046D03; // struct mtpos *
        public const uint BLKGETSIZE64 = 0x80081272; // get the byte size of a device
        public const uint CDROMREADTOCHDR = 0x00005305; // Read TOC header (struct cdrom_tochdr)
        public const uint CDROM_SET_OPTIONS = 0x00005320; // set CDROM Options
        public const uint CDROM_CLEAR_OPTIONS = 0x00005321; // clear CDROM Options
        public const uint CDROMSTART = 0x00005308;
        public const uint CDROMEJECT = 0x00005309;
        public const uint CDROM_LOCKDOOR = 0x00005329;
        public const uint CDROM_DRIVE_STATUS = 0x00005326; // Check drive status

        public const int CDS_NO_INFO = 0;	/* if not implemented */
        public const int CDS_NO_DISC = 1;
        public const int CDS_TRAY_OPEN = 2;
        public const int CDS_DRIVE_NOT_READY = 3;
        public const int CDS_DISC_OK = 4;

        /* Magnetic Tape operations [Not all operations supported by all drivers]: */
        public enum TapeOpType
        {
            /// <summary>
            /// +reset drive in case of problems
            /// </summary>
            MTRESET = 0,

            /// <summary>
            /// forward space over FileMark, position at first record of next file
            /// </summary>
            MTFSF = 1,

            /// <summary>
            /// backward space FileMark (position before FM)
            /// </summary>
            MTBSF = 2,

            /// <summary>
            /// forward space record
            /// </summary>
            MTFSR = 3,

            /// <summary>
            /// backward space record
            /// </summary>
            MTBSR = 4,

            /// <summary>
            /// write an end-of-file record (mark)
            /// </summary>
            MTWEOF = 5,

            /// <summary>
            /// rewind
            /// </summary>
            MTREW = 6,


            /// <summary>
            /// rewind and put the drive offline (eject?)
            /// </summary>
            MTOFFL = 7,

            /// <summary>
            /// no op, set status only (read with MTIOCGET)
            /// </summary>
            MTNOP = 8,

            /// <summary>
            /// retension tape
            /// </summary>
            MTRETEN = 9,

            /// <summary>
            /// +backward space FileMark, position at FM
            /// </summary>
            MTBSFM = 10,

            /// <summary>
            /// +forward space FileMark, position at FM
            /// </summary>
            MTFSFM = 11,

            /// <summary>
            /// goto end of recorded media (for appending files). MTEOM positions after the last FM, ready for appending another file.
            /// </summary>
            MTEOM = 12,

            /// <summary>
            /// erase tape -- be careful!
            /// </summary>
            MTERASE = 13,

            /// <summary>
            /// run self test 1 (nondestructive)
            /// </summary>
            MTRAS1 = 14,

            /// <summary>
            /// run self test 2 (destructive)
            /// </summary>
            MTRAS2 = 15,

            /// <summary>
            /// reserved for self test 3
            /// </summary>
            MTRAS3 = 16,

            /// <summary>
            /// set block length (SCSI)
            /// </summary>
            MTSETBLK = 20,

            /// <summary>
            /// set tape density (SCSI)
            /// </summary>
            MTSETDENSITY = 21,

            /// <summary>
            /// seek to block (Tandberg, etc.)
            /// </summary>
            MTSEEK = 22,

            /// <summary>
            /// tell block (Tandberg, etc.)
            /// </summary>
            MTTELL = 23,

            /// <summary>
            /// set the drive buffering according to SCSI-2 ordinary buffered operation with code 1
            /// </summary>
            MTSETDRVBUFFER = 24,

            /// <summary>
            /// space forward over setmarks
            /// </summary>
            MTFSS = 25,

            /// <summary>
            /// space backward over setmarks
            /// </summary>
            MTBSS = 26,

            /// <summary>
            /// write setmarks
            /// </summary>
            MTWSM = 27,

            /// <summary>
            /// lock the drive door
            /// </summary>
            MTLOCK = 28,

            /// <summary>
            /// unlock the drive door
            /// </summary>
            MTUNLOCK = 29,

            /// <summary>
            /// execute the SCSI load command
            /// </summary>
            MTLOAD = 30,

            /// <summary>
            /// execute the SCSI unload command
            /// </summary>
            MTUNLOAD = 31,

            /// <summary>
            /// control compression with SCSI mode page 15
            /// </summary>
            MTCOMPRESSION = 32,

            /// <summary>
            /// Change the active tape partition
            /// </summary>
            MTSETPART = 33,

            /// <summary>
            /// Format the tape with one or two partitions
            /// </summary>
            MTMKPART = 34
        }

        /// <summary>
        ///     No medium found
        /// </summary>
        public const int ENOMEDIUM = 123;

        #pragma warning restore	CS0414
    }
}