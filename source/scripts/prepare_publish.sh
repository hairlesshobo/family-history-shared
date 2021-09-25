#!/bin/sh

# for development...
if [ -z $DRONE_TAG ]; then
    DRONE_TAG="release/2.0.0-beta1"
fi

SCRIPT_DIR="$( cd "$( dirname "$0" )" && pwd )"
BUILD_PROPS="$SCRIPT_DIR/../Directory.Build.props"
. $SCRIPT_DIR/super-tiny-colors.bash

if [ ! -f "$BUILD_PROPS" ]; then
    echo -e "${nR}ERROR: Cannot find the Directory.Build.props file... aborting!"
    exit 1
fi

if [ "$(echo $DRONE_TAG | cut -n -c-8)" != "release/" ]; then
    echo -e "${nR}The current tag is NOT a release tag.. aborting!${S}"
    exit 1
fi

project_name=$(cat $SCRIPT_DIR/../Directory.Build.props | grep Product | awk -F'>' '{print $2}' | awk -F'<' '{print $1}' | tr '[:upper:]' '[:lower:]')
project_version=$(cat $SCRIPT_DIR/../Directory.Build.props | grep Version | awk -F'>' '{print $2}' | awk -F'<' '{print $1}')
tag_version=$(echo $DRONE_TAG | cut -n -c9-)
out_name_root="${project_name}_${project_version}_source"

if [ "$project_version" != "$tag_version" ]; then
    echo -e "${nR}The current tag version does not match the project version.. aborting!${S}"
    exit 1
fi

echo -e "${nG}Preparing publish for ${project_name} v${tag_version}${S}"

cd $SCRIPT_DIR/../

echo -n "Creating dist directory... "
mkdir -p dist
echo "Done"

echo -e "${nB}Creating '${out_name_root}.tar.gz'...${S}"
rm -f ./dist/${out_name_root}.tar.gz
tar --exclude "./dist" --exclude=".git" --exclude="**/obj" --exclude="**/bin" -czvf ./dist/${out_name_root}.tar.gz ./ 2>&1 | sed 's/^/[tar] /'

echo -e "${nB}Creating '${out_name_root}.zip'...${S}"
rm -f ./dist/${out_name_root}.zip
zip -v -r ./dist/${out_name_root}.zip ./ -x dist\* -x .git\* -x \*/obj\* -x \*/bin\* 2>&1 | sed 's/^/[zip] /'

echo -e "${nB}Copying NuGet packages to dist...${S}"
rm -f ./dist/*.nupkg
find . -name "*${project_version}.nupkg" ! -wholename "*/dist/*" -exec cp -v {} ./dist/ \; 2>&1 | sed 's/^/[find] /'