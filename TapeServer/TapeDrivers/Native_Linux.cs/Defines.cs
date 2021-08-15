using System;

namespace Archiver.TapeServer.TapeDrivers
{
    public partial class NativeLinuxTapeDriver
    {
        static readonly int OPEN_READ_ONLY = 0;
        static readonly int OPEN_WRITE_ONLY = 1;
        static readonly int OPEN_READ_WRITE = 2;

        static readonly uint MTIOCTOP = 0x40086D01; //         const struct mtop *
        static readonly uint MTIOCGET = 0x801C6D02; //         struct mtget *
        static readonly uint MTIOCPOS = 0x80046D03; //         struct mtpos *
        

        /* Magnetic Tape operations [Not all operations supported by all drivers]: */
        static readonly short MTRESET       = 0;	/* +reset drive in case of problems */
        static readonly short MTFSF         = 1;	/* forward space over FileMark,
                                     * position at first record of next file 
                                     */
        static readonly short MTBSF         = 2;	/* backward space FileMark (position before FM) */
        static readonly short MTFSR         = 3;	/* forward space record */
        static readonly short MTBSR         = 4;	/* backward space record */
        static readonly short MTWEOF        = 5;	/* write an end-of-file record (mark) */
        static readonly short MTREW         = 6;	/* rewind */
        static readonly short MTOFFL        = 7;	/* rewind and put the drive offline (eject?) */
        static readonly short MTNOP         = 8;	/* no op, set status only (read with MTIOCGET) */
        static readonly short MTRETEN       = 9;	/* retension tape */
        static readonly short MTBSFM        = 10;	/* +backward space FileMark, position at FM */
        static readonly short MTFSFM        = 11;	/* +forward space FileMark, position at FM */
        static readonly short MTEOM         = 12;	/* goto end of recorded media (for appending files).
                                     * MTEOM positions after the last FM, ready for
                                     * appending another file.
                                     */
        static readonly short MTERASE       = 13;	/* erase tape -- be careful! */

        static readonly short MTRAS1        = 14;	/* run self test 1 (nondestructive) */
        static readonly short MTRAS2        = 15;	/* run self test 2 (destructive) */
        static readonly short MTRAS3        = 16;	/* reserved for self test 3 */

        static readonly short MTSETBLK      = 20;	/* set block length (SCSI) */
        static readonly short MTSETDENSITY  = 21;	/* set tape density (SCSI) */
        static readonly short MTSEEK        = 22;	/* seek to block (Tandberg, etc.) */
        static readonly short MTTELL        = 23;	/* tell block (Tandberg, etc.) */
        static readonly short MTSETDRVBUFFER= 24;   /* set the drive buffering according to SCSI-2 */
                                    /* ordinary buffered operation with code 1 */
        static readonly short MTFSS         = 25;	/* space forward over setmarks */
        static readonly short MTBSS         = 26;	/* space backward over setmarks */
        static readonly short MTWSM         = 27;	/* write setmarks */

        static readonly short MTLOCK        = 28;	/* lock the drive door */
        static readonly short MTUNLOCK      = 29;	/* unlock the drive door */
        static readonly short MTLOAD        = 30;	/* execute the SCSI load command */
        static readonly short MTUNLOAD      = 31;	/* execute the SCSI unload command */
        static readonly short MTCOMPRESSION = 32;   /* control compression with SCSI mode page 15 */
        static readonly short MTSETPART     = 33;	/* Change the active tape partition */
        static readonly short MTMKPART      = 34;	/* Format the tape with one or two partitions */
    }
}