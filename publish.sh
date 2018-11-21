#!/bin/bash

set -e # break on error.
cd /home/jbfp/projects/dotnet/sequence/

echo Building in Release mode...
dotnet build -c Release
cd ./src/Sequence.Api/wwwroot
yarn run build
cd ../../../

echo Running all tests...
ls ./test/**/*.csproj | xargs -L1 dotnet test -c Release -v q --no-build

echo Publishing...
cd ./src/Sequence.Api/
dotnet publish -r linux-x64 -c Release
cd ./bin/Release/netcoreapp2.1/publish/

echo Killing remote server process...
ssh jbfp@jbfp.dk pkill -x dotnet || true

echo Copying files...
scp -rp ./ jbfp@jbfp.dk:/home/jbfp/sequence/

echo Starting remote server process...
ssh jbfp@jbfp.dk screen -S sequence -d -m "/home/jbfp/run.sh"

echo Done!

