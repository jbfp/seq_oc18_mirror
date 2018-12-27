#!/bin/bash

set -e # break on error.
cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" # cd to script location.
ls ./test/**/*.csproj | xargs -L1 -P 0 dotnet test -c Release -v q

dotnet test -c Release -v q ./auth/Sequence.Auth.Test/
dotnet test -c Release -v q ./postgres/Sequence.Postgres.Test/

