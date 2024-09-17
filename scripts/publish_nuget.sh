#!/bin/bash

echo Executing after success scripts on branch $GITHUB_REF_NAME
echo Triggering NuGet package build

cd src/CRMScraper.Library

echo Restoring NuGet packages...
dotnet restore

echo Packing the library...
dotnet pack -c release /p:PackageVersion=1.1.$GITHUB_RUN_NUMBER --no-restore -o ./nupkg

echo Library package created for branch $GITHUB_REF_NAME

case "$GITHUB_REF_NAME" in
  "main")
    echo Uploading CRMScraper.Library package to NuGet

    PACKAGE_PATH="./nupkg/CRMScraper.Library.1.1.$GITHUB_RUN_NUMBER.nupkg"

    if [ -f "$PACKAGE_PATH" ]; then
      echo "Package found: $PACKAGE_PATH"
      dotnet nuget push "$PACKAGE_PATH" -k "$NUGET_API_KEY" -s https://api.nuget.org/v3/index.json
    else
      echo "Error: Package $PACKAGE_PATH not found."
      exit 1
    fi
    ;;
  *)
    echo "Not on main branch, skipping NuGet package upload."
    ;;
esac
