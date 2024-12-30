#!/bin/sh
cd TranslationSheetEditor/

# Clean up old builds if present
rm -rf publish-win-x64
rm -rf publish-linux-x64

# Build application

## Linux - standalone / trimmed
dotnet publish -c Release -o ./publish-linux-x64-standalone-trimmed -f net8.0 -r linux-x64 --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true

## Linux - needs framework / single file
dotnet publish -c Release -o ./publish-linux-x64-default -f net8.0 -r linux-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

## Linux - needs framework / simple-theme
dotnet publish -c Release -o ./publish-linux-x64-experimental -f net8.0 -r linux-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -p SIMPLE_THEME=true

## Win - standalone / trimmed
dotnet publish -c Release -o ./publish-win-x64-standalone-trimmed -f net8.0 -r win-x64 --self-contained /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true

## Win - standalone
dotnet publish -c Release -o ./publish-win-x64-standalone -f net8.0 -r win-x64 --self-contained /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

## Win - needs framework / single file
dotnet publish -c Release -o ./publish-win-x64-default -f net8.0 -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

## Win - needs framework / simple-theme
dotnet publish -c Release -o ./publish-win-x64-experimental -f net8.0 -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -p SIMPLE_THEME=true

## Win - needs framework / mess
dotnet publish -c Release -o ./publish-win-x64-with-parts -f net8.0 -r win-x64
