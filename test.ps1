$ErrorActionPreference = "Stop" # break on error.
cd "$PSScriptRoot" # cd to script location.
dotnet build --verbosity q --configuration Release
dotnet test "test" -c Release --no-build --filter "Category=Integration"
dotnet test "test" -c Release --no-build --filter "Category!=Integration"
