$ErrorActionPreference = "Stop" # break on error.
cd "$PSScriptRoot" # cd to script location.
cd ./src/wwwroot/
yarn start
