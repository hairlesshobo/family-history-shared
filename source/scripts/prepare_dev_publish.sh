#!/bin/sh

SCRIPT_DIR="$( cd "$( dirname "$0" )" && pwd )"
BUILD_PROPS="$SCRIPT_DIR/../Directory.Build.props"
. $SCRIPT_DIR/super-tiny-colors.bash

if [ -z $DRONE_COMMIT_SHA ]; then
    if [ -z $1 ]; then
        echo -e "${nR}ERROR: No git sha provided ... aborting!"
    else
        DRONE_COMMIT_SHA="$1"
    fi
fi

if [ ! -f "$BUILD_PROPS" ]; then
    echo -e "${nR}ERROR: Cannot find the Directory.Build.props file... aborting!"
    exit 1
fi

project_version=$(cat $SCRIPT_DIR/../Directory.Build.props | grep Version | awk -F'>' '{print $2}' | awk -F'<' '{print $1}' | awk -F'-' '{print $1}')
project_name=$(cat $SCRIPT_DIR/../Directory.Build.props | grep Product | awk -F'>' '{print $2}' | awk -F'<' '{print $1}' | tr '[:upper:]' '[:lower:]')
out_name_root="${project_name}_${project_version}-${DRONE_COMMIT_SHA}_source"

echo -e "${nG}Preparing dev publish for ${project_name} v${project_version}${S}"

cd $SCRIPT_DIR/../../

echo -n "Creating dist directory... "
mkdir -p dist
echo "Done"

echo -e "${nB}Creating '${out_name_root}.tar.gz'...${S}"
rm -f ./dist/${out_name_root}.tar.gz
tar --exclude "./dist" --exclude="config/*.json" --exclude="config/tapes/*.json" --exclude="./json" --exclude "./index.txt" --exclude="./tools" --exclude=".git" --exclude="**/obj" --exclude="**/bin" -czvf ./dist/${out_name_root}.tar.gz ./ 2>&1 | sed 's/^/[tar] /'
