#!/bin/bash

script_path="$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")"

set -e # break on error.
cd $script_path # cd to script location.
cd ./api/Sequence.Api/wwwroot

rm -rf ./build
yarn run build
git rev-parse HEAD > ./build/hash.txt
rsync -ru --progress --exclude="node_modules" ./* jbfp@jbfp.dk:/home/jbfp/sequence/wwwroot

echo Done!

