#!/bin/bash

cd "$(dirname "$0")"

./download_net_runtime.py windows

# Clear out previous build.
rm -r **/bin bin/publish/Windows
rm Editor_Windows.zip

dotnet publish Editor/Editor.csproj /p:FullRelease=True -c Release --no-self-contained -r win-x64 /nologo

./exe_set_subsystem.py "Editor/bin/Release/net5.0/win-x64/publish/Editor.exe" 2
./exe_set_subsystem.py "Editor/bin/Release/net5.0/win-x64/publish/Editor.exe" 2

# Create intermediate directories.
mkdir -p bin/publish/Windows/bin
mkdir -p bin/publish/Windows/bin/loader
mkdir -p bin/publish/Windows/dotnet

cp -r Dependencies/dotnet/windows/* bin/publish/Windows/dotnet
cp Editor/bin/Release/net5.0/win-x64/publish/* bin/publish/Windows/bin

pushd bin/publish/Windows
zip -r ../../../Editor_Windows.zip *
popd
