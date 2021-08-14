#!/bin/sh

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
