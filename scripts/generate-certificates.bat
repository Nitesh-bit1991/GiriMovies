@echo off
echo GiriMovies Certificate Generator
echo ===================================
echo.

REM Check if PowerShell is available
powershell -Command "exit" >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: PowerShell is required but not found.
    pause
    exit /b 1
)

REM Get device information
set /p deviceName="Enter device name (e.g., John-Laptop): "
if "%deviceName%"=="" (
    echo ERROR: Device name cannot be empty.
    pause
    exit /b 1
)

echo.
echo Device Types:
echo 1. Computer
echo 2. Laptop  
echo 3. Tablet
echo 4. Mobile
echo 5. TV
echo.
set /p deviceChoice="Select device type (1-5): "

set deviceType=Computer
if "%deviceChoice%"=="1" set deviceType=Computer
if "%deviceChoice%"=="2" set deviceType=Laptop
if "%deviceChoice%"=="3" set deviceType=Tablet
if "%deviceChoice%"=="4" set deviceType=Mobile
if "%deviceChoice%"=="5" set deviceType=TV

echo.
echo Selected Configuration:
echo - Device Name: %deviceName%
echo - Device Type: %deviceType%
echo - Computer: %COMPUTERNAME%
echo.

set /p confirm="Continue? (y/n): "
if /i not "%confirm%"=="y" (
    echo Operation cancelled.
    pause
    exit /b 0
)

echo.
echo Generating certificates...

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Run PowerShell script with full path
powershell -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Generate-DeviceCertificates.ps1" -DeviceName "%deviceName%" -DeviceType "%deviceType%"

if %errorlevel% equ 0 (
    echo.
    echo SUCCESS: Certificates generated successfully!
    echo Check the 'certificates' folder for your files.
    echo.
    echo Next Steps:
    echo 1. Install the CA certificate: GiriMovies-ca.crt
    echo 2. Install your device certificate: %deviceName%.pfx
    echo 3. Configure your GiriMovies client
    echo.
) else (
    echo.
    echo ERROR: Certificate generation failed.
    echo Please check the error messages above.
)

pause