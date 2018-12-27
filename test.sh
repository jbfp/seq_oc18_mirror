#!/bin/bash

set -e # break on error.
cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" # cd to script location.
ls ./**/*.Test/*.csproj | xargs -L1 -P 0 dotnet test -c Release -v q
