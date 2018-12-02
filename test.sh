#!/bin/bash
set -e
ls ./test/**/*.csproj | xargs -L1 -P 0 dotnet test -c Release -v q
