@echo off
cd /d "%~dp0"

set DOTNET="C:\Program Files\dotnet\dotnet.exe"
if not exist %DOTNET% set DOTNET="C:\Program Files (x86)\dotnet\dotnet.exe"

echo Building installable single-file executable...

%DOTNET% publish -c Release -p:PublishTrimmed=false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if %errorLevel% neq 0 (
    echo Build failed!
    pause
    exit /b
)

set PUBLISH_DIR=bin\Release\net10.0-windows10.0.26100.0\win-x64\publish

if exist "build-installable" rmdir /s /q "build-installable"
mkdir "build-installable"
copy "%PUBLISH_DIR%\XamppMultidomainManager.exe" "build-installable\" >nul

echo.
echo ==========================================
echo Build complete!
echo Installable exe created at:
echo   %~dp0build-installable\XamppMultidomainManager.exe
echo.
echo Distribute this single file.
echo When run, it will show the setup wizard.
echo ==========================================
pause
