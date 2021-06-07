#!/bin/bash

cd "$(dirname "$0")"

./download_net_runtime.py linux

# Clear out previous build.
rm -r **/bin bin/publish/Linux
rm Editor_Linux.zip

dotnet publish Editor/Editor.csproj /p:FullRelease=True -c Release --no-self-contained -r linux-x64 /nologo

# Create intermediate directories.
mkdir -p bin/publish/Linux/bin
mkdir -p bin/publish/Linux/bin/loader
mkdir -p bin/publish/Linux/dotnet

cp PublishFiles/RSIEditor PublishFiles/RSIEditor.desktop bin/publish/Linux/
cp Editor/bin/Release/net5.0/linux-x64/publish/* bin/publish/Linux/bin/
cp -r Dependencies/dotnet/linux/* bin/publish/Linux/dotnet/

cd bin/publish/Linux
zip -r ../../../Editor_Linux.zip *
