@echo off
net session >nul 2>&1
if %errorLevel% neq 0 (
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)
cd /d "%~dp0"
echo Building Release...
"C:\Program Files\dotnet\dotnet.exe" publish -c Release -p:PublishTrimmed=false
if %errorLevel% neq 0 (
    echo Build failed!
    pause
    exit /b
)
echo Starting app...
start "" "bin\Release\net10.0-windows10.0.26100.0\win-x64\publish\XamppMultidomainManager.exe"
pause
