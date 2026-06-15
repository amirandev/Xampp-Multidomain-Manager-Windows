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

:: Sign the executable with a self-signed certificate
set CERT_NAME=XROW.ASIA
set EXE_PATH=build-installable\XamppMultidomainManager.exe

where signtool >nul 2>&1
if %errorLevel% equ 0 (
    :: Create a self-signed certificate if it doesn't exist
    powershell -Command "$c = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert | Where-Object { $_.Subject -eq 'CN=%CERT_NAME%' } | Select-Object -First 1; if (-not $c) { $c = New-SelfSignedCertificate -Type Custom -Subject 'CN=%CERT_NAME%' -KeyUsage DigitalSignature -TextExtension '2.5.29.37={text}1.3.6.1.5.5.7.3.3' -CertStoreLocation 'Cert:\CurrentUser\My'; } $c.Thumbprint" > "%TEMP%\cert_thumb.txt"
    set /p CERT_THUMB=<"%TEMP%\cert_thumb.txt"
    signtool sign /fd SHA256 /sha1 "%CERT_THUMB%" /tr http://timestamp.digicert.com /td SHA256 "%EXE_PATH%" >nul
    if %errorLevel% equ 0 (
        echo Signed successfully with self-signed certificate
    ) else (
        echo Warning: Signing failed, continuing unsigned
    )
) else (
    echo Warning: signtool not found (install Windows SDK to enable signing)
)

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
