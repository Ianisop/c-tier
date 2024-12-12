@echo off
cd c-tier

set CONFIGURATION=Release
set TRIMMED=false

echo Building for Windows (win-x64)
dotnet publish -r win-x64 -c %CONFIGURATION% /p:PublishSingleFile=true /p:PublishTrimmed=%TRIMMED%
echo Windows build complete.

echo Building for Linux (linux-x64)
dotnet publish -r linux-x64 -c %CONFIGURATION% /p:PublishSingleFile=true /p:PublishTrimmed=%TRIMMED%
echo Linux x64 build complete.

echo Building for AArch64 Linux (linux-arm64)
dotnet publish -r linux-arm64 -c %CONFIGURATION% /p:PublishSingleFile=true /p:PublishTrimmed=%TRIMMED%
echo AArch64 Linux build complete.

echo Building for macOS (osx-x64)
dotnet publish -r osx-x64 -c %CONFIGURATION% /p:PublishSingleFile=true /p:PublishTrimmed=%TRIMMED%
echo MacOS build complete.

echo Copying files to root directory...
cd ..
copy ".\c-tier\bin\Release\net8.0\win-x64\publish\c-tier.exe" ".\win-x64.exe"
copy ".\c-tier\bin\Release\net8.0\linux-arm64\publish\c-tier" ".\linux-aarch64"
copy ".\c-tier\bin\Release\net8.0\linux-x64\publish\c-tier" ".\linux-x64"
copy ".\c-tier\bin\Release\net8.0\osx-x64\publish\c-tier" ".\osx-x64"

echo Completed building
pause
