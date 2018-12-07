#!/bin/bash

set -e # break on error.
cd /home/jbfp/projects/dotnet/sequence/

echo Building in Release mode...
dotnet build -c Release -v q

echo Running all tests...
ls ./test/**/*.csproj | xargs -L1 -P 0 dotnet test -c Release -v q --no-build

cd ./src/Sequence.Api/

echo Cleaning previous publish output...
rm -rf ./bin/Release/netcoreapp2.2/publish

echo Building wwwroot...
cd ./wwwroot
yarn run build
cd ../

echo Publishing...
dotnet publish -c Release -v q
cd ./bin/Release/netcoreapp2.2/publish/

echo Killing remote server process...
ssh jbfp@jbfp.dk pkill -x dotnet || true

echo Copying files...
git rev-parse HEAD > ./wwwroot/build/hash.txt
rsync -ru --progress ./* jbfp@jbfp.dk:/home/jbfp/sequence

echo Starting remote server process...
ssh jbfp@jbfp.dk screen -S sequence -d -m "/home/jbfp/run.sh"

echo Done!

