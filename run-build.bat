@echo off
net session >nul 2>&1
if %errorLevel% neq 0 (
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)
cd /d "%~dp0"
echo Building Release...
"C:\Program Files\dotnet\dotnet.exe" publish -c Release -p:PublishTrimmed=false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if %errorLevel% neq 0 (
    echo Build failed!
    pause
    exit /b
)

set PUBLISH_DIR=bin\Release\net10.0-windows10.0.26100.0\win-x64\publish
set BUILD_DIR=build

if exist "%BUILD_DIR%" rmdir /s /q "%BUILD_DIR%"
mkdir "%BUILD_DIR%"
xcopy "%PUBLISH_DIR%\*" "%BUILD_DIR%\" /e /i /h /y >nul
echo Copied to build\ folder

echo Starting app...
start "" "%BUILD_DIR%\XamppMultidomainManager.exe"
pause
