#!/bin/bash

echo Executing after success scripts on branch $GITHUB_REF_NAME
echo Triggering NuGet package build

cd src/CRMScraper.Library

echo Restoring NuGet packages...
dotnet restore

echo Packing the library...
dotnet pack -c release /p:PackageVersion=1.1.$GITHUB_RUN_NUMBER --no-restore -o .

echo Library package created for branch $GITHUB_REF_NAME

case "$GITHUB_REF_NAME" in
  "main")
    echo Uploading CRMScraper.Library package to NuGet
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;
  *)
    echo "Not on main branch, skipping NuGet package upload."
    ;;
esac
