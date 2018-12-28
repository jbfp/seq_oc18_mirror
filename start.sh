#!/bin/bash

set -e
script_path="$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")"
cd $script_path
cd ./api/Sequence.Api/wwwroot/
yarn start



