#!/bin/bash

set -e # break on error.
cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" # cd to script location.
dotnet build --verbosity q --configuration Release
dotnet test "test" -c Release --no-build --filter "Category=Integration"
dotnet test "test" -c Release --no-build --filter "Category!=Integration"

