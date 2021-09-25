/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 * 
 *  Much of the code in this file is from the util-linux project. The 
 *  following copyrights also apply to the below code:
 * 
 *  Copyright (C) 1994-2005 Jeff Tranter (tranter@pobox.com)
 *  Copyright (C) 2012 Karel Zak <kzak@redhat.com>
 *  Copyright (C) Michal Luscon <mluscon@redhat.com>
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

#include <ctype.h>
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <linux/cdrom.h>
#include <scsi/scsi.h>
#include <scsi/sg.h>
#include <sys/ioctl.h>
#include <sys/mount.h>

#include "linux.h"

#define DEBUG


int eject_scsi(int fd)
{
	int status, k;
	sg_io_hdr_t io_hdr;
	unsigned char allowRmBlk[6] = {ALLOW_MEDIUM_REMOVAL, 0, 0, 0, 0, 0};
	unsigned char startStop1Blk[6] = {START_STOP, 0, 0, 0, 1, 0};
	unsigned char startStop2Blk[6] = {START_STOP, 0, 0, 0, 2, 0};
	unsigned char inqBuff[2];
	unsigned char sense_buffer[32];

	if ((ioctl(fd, SG_GET_VERSION_NUM, &k) < 0) || (k < 30000)) {
		printf("not an sg device, or old sg driver\n");
		return -1;
	}

	memset(&io_hdr, 0, sizeof(sg_io_hdr_t));
	io_hdr.interface_id = 'S';
	io_hdr.cmd_len = 6;
	io_hdr.mx_sb_len = sizeof(sense_buffer);
	io_hdr.dxfer_direction = SG_DXFER_NONE;
	io_hdr.dxfer_len = 0;
	io_hdr.dxferp = inqBuff;
	io_hdr.sbp = sense_buffer;
	io_hdr.timeout = 10000;

	io_hdr.cmdp = allowRmBlk;

    char b[sizeof(io_hdr)];
    memcpy(b, &io_hdr, sizeof(io_hdr));

	status = ioctl(fd, SG_IO, (void *)&io_hdr);
	if (status < 0 || io_hdr.host_status || io_hdr.driver_status)
		return -2;

	io_hdr.cmdp = startStop1Blk;
	status = ioctl(fd, SG_IO, (void *)&io_hdr);
	if (status < 0 || io_hdr.host_status)
		return -3;

	/* Ignore errors when there is not medium -- in this case driver sense
	 * buffer sets MEDIUM NOT PRESENT (3a) bit. For more details see:
	 * http://www.tldp.org/HOWTO/archived/SCSI-Programming-HOWTO/SCSI-Programming-HOWTO-22.html#sec-sensecodes
	 * -- kzak Jun 2013
	 */
	if (io_hdr.driver_status != 0 &&
	    !(io_hdr.driver_status == DRIVER_SENSE && io_hdr.sbp &&
		                                      io_hdr.sbp[12] == 0x3a))
		return -4;

	io_hdr.cmdp = startStop2Blk;
	status = ioctl(fd, SG_IO, (void *)&io_hdr);
	if (status < 0 || io_hdr.host_status || io_hdr.driver_status)
		return -5;

	/* force kernel to reread partition table when new disc inserted */
	ioctl(fd, BLKRRPART);
    return 0;
}

/*
 * Eject using CDROMEJECT ioctl.
 */
static int eject_cdrom(int fd)
{
#if defined(CDROMEJECT)
	int ret = ioctl(fd, CDROM_LOCKDOOR, 0);

	if (ret < 0)
		return ret;

	return ioctl(fd, CDROMEJECT) >= 0;

#elif defined(CDIOCEJECT)
	return ioctl(fd, CDIOCEJECT) >= 0;
#else
	errno = ENOSYS;
	return -1;
#endif
}

int eject(const char *__path)
{
    #ifdef DEBUG
    printf("opening %s...\n", __path);
    #endif

    int fd = open(__path, O_RDWR | O_NONBLOCK);

    if (fd < 0)
    {
        #ifdef DEBUG
        perror("Failed to open");
        #endif

        return -1;
    }

    #ifdef DEBUG
    printf("disabling auto-close...\n");
    #endif

    int result = ioctl(fd, CDROM_CLEAR_OPTIONS, CDO_AUTO_CLOSE);

    if (result < 0)
    {
        #ifdef DEBUG
        perror("failed to disable auto-close");
        #endif

        return -1;
    }

    #ifdef DEBUG
    printf("attempting cdrom eject ioctl call\n");
    #endif
    result = eject_cdrom(fd);

    if (result < 0)
    {
        #ifdef DEBUG
        printf("CDROM eject failed, trying raw scsi commands...\n");
        #endif

        result = eject_scsi(fd);

        #ifdef DEBUG
        printf("Result from eject_scsi: %d\n", result);
        #endif
    }

    
    if (fd >= 0)
    {
        #ifdef DEBUG
        printf("Closing file handle..\n");
        #endif
        
        if (close(fd) < 0)
        {
            #ifdef DEBUG
            perror("Close failed");
            #endif

            return -1;
        }
    }
}

// int main(int argc, char **argv)
// {
//     eject("/dev/sr0");
//     exit(0);
// }