#!/bin/bash
set -e
ls ./test/**/*.csproj | xargs -L1 dotnet test -c Release -v q
