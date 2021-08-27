linux.so: linux.c
	mkdir -p bin
	gcc -c -fPIC linux.c    -o bin/linux.o
	gcc -shared bin/linux.o -o bin/linux.so

# linux_helpers: linux.c
# 	gcc linux.c -o linux_helpers