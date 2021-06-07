#!/bin/bash

cd "$(dirname "$0")"

./download_net_runtime.py mac

# Clear out previous build.
rm -r **/bin bin/publish/macOS
rm Editor_macOS.zip

dotnet publish Editor/Editor.csproj /p:FullRelease=True -c Release --no-self-contained -r osx-x64 /nologo

# Create intermediate directories.
mkdir -p bin/publish/macOS

cp -r "PublishFiles/Editor.app" bin/publish/macOS

mkdir -p "bin/publish/macOS/Editor.app/Contents/Resources/dotnet/"
mkdir -p "bin/publish/macOS/Editor.app/Contents/Resources/bin/"

cp -r Dependencies/dotnet/mac/* "bin/publish/macOS/Editor.app/Contents/Resources/dotnet/"
cp -r Editor/bin/Release/net5.0/osx-x64/publish/* "bin/publish/macOS/Editor.app/Contents/Resources/bin/"
pushd bin/publish/macOS
zip -r ../../../Editor_macOS.zip *
popd
