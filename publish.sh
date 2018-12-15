#!/bin/bash

script_path="$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")"

set -e # break on error.
cd $script_path # cd to script location.

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

echo Publishing API...
dotnet publish -c Release -v q
cd ./bin/Release/netcoreapp2.2/publish/

echo Killing remote server processes...
ssh jbfp@jbfp.dk "pkill -x 'dotnet Sequence.Api.dll'" || true

echo Copying files...
git rev-parse HEAD > ./wwwroot/build/hash.txt
rsync -ru --progress ./* jbfp@jbfp.dk:/home/jbfp/sequence

echo Starting remote API process...
ssh jbfp@jbfp.dk screen -S sequence -d -m "/home/jbfp/run-sequence.sh"

cd $script_path

echo Publishing Auth...
cd ./src/Sequence.Auth/

dotnet publish -c Release -v q
cd ./bin/Release/netcoreapp2.2/publish/

echo Killing remote server processes...
ssh jbfp@jbfp.dk "pkill -x 'dotnet Sequence.Auth.dll'" || true

echo Copying files...
rsync -ru --progress ./* jbfp@jbfp.dk:/home/jbfp/auth

echo Starting remote auth process...
ssh jbfp@jbfp.dk screen -S auth -d -m "/home/jbfp/run-auth.sh"

echo Done!

