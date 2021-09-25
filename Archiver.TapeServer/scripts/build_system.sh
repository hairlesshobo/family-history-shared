#!/bin/sh
#
#  Archiver - Cross platform, multi-destination backup and archiving utility
# 
#  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
#
#  This program is free software; you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation; either version 2 of the License, or
#  (at your option) any later version.
#  
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#  
#  You should have received a copy of the GNU General Public License along
#  with this program; if not, write to the Free Software Foundation, Inc.,
#  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
#


wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

apt-get update
apt-get install -y \
	apt-transport-https \
	beep \
	buffer \
	git \
	mt-st \
	mtx \
	netcat-openbsd \
	pv \
	rsync \
	sg3-utils

apt-get update 

apt-get install -y dotnet-sdk-5.0

#RUN rm -rf /var/lib/apt/lists/*

#ENTRYPOINT ["dotnet", "dotnetapp.dll"]
