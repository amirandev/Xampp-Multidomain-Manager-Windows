@echo off
net session >nul 2>&1
if %errorLevel% neq 0 (
    powershell -Command "Start-Process '%~f0' -Verb RunAs -Wait"
    pause
    exit /b
)
cd /d "%~dp0"

:: Generate icons first
echo Generating icons...
where python >nul 2>&1
if %errorLevel% equ 0 (
    python generate_icons.py
) else (
    echo Python not found, skipping icon generation
)

echo Building Release...
set DOTNET="C:\Program Files\dotnet\dotnet.exe"
if not exist %DOTNET% set DOTNET="C:\Program Files (x86)\dotnet\dotnet.exe"
%DOTNET% publish -c Release -p:PublishTrimmed=false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if %errorLevel% neq 0 (
    echo Build failed!
    pause
    exit /b
)

set PUBLISH_DIR=bin\Release\net10.0-windows10.0.26100.0\win-x64\publish
set INSTALL_DIR=%ProgramW6432%\XamppMultidomainManager

if exist "%INSTALL_DIR%" rmdir /s /q "%INSTALL_DIR%"
mkdir "%INSTALL_DIR%"
copy "%PUBLISH_DIR%\XamppMultidomainManager.exe" "%INSTALL_DIR%\" >nul
if %errorLevel% neq 0 (
    echo Failed to copy to Program Files
    pause
    exit /b
)
echo Installed to %INSTALL_DIR%

:: Create Start Menu shortcut
set SHORTCUT=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Xampp Multidomain Manager.lnk
powershell -Command "$WS = New-Object -ComObject WScript.Shell; $SC = $WS.CreateShortcut('%SHORTCUT%'); $SC.TargetPath = '%INSTALL_DIR%\XamppMultidomainManager.exe'; $SC.WorkingDirectory = '%INSTALL_DIR%'; $SC.Description = 'Xampp Multidomain Manager — Powered By XROW.ASIA'; $SC.Save()"
echo Created Start Menu shortcut

:: Create Desktop shortcut
set DESKTOP_SHORTCUT=%PUBLIC%\Desktop\Xampp Multidomain Manager.lnk
powershell -Command "$WS = New-Object -ComObject WScript.Shell; $SC = $WS.CreateShortcut('%DESKTOP_SHORTCUT%'); $SC.TargetPath = '%INSTALL_DIR%\XamppMultidomainManager.exe'; $SC.WorkingDirectory = '%INSTALL_DIR%'; $SC.Description = 'Xampp Multidomain Manager — Powered By XROW.ASIA'; $SC.Save()"
echo Created Desktop shortcut

echo.
echo ==========================================
echo Installation complete!
echo Xampp Multidomain Manager installed to:
echo   %INSTALL_DIR%
echo.
echo You can now run it from Start Menu or Desktop.
echo ==========================================
pause
