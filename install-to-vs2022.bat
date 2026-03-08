@echo off
setlocal

:: Find VS 2022 installation path using vswhere via PowerShell
for /f "usebackq tokens=*" %%i in (`powershell -Command "& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -version '[17.0,18.0)' -property installationPath"`) do set "VS_PATH=%%i"

if not defined VS_PATH (
    echo Error: Visual Studio 2022 not found
    exit /b 1
)

:: Get VSIX path from argument or use default
if "%~1"=="" (
    set "VSIX_PATH=%~dp0src\CodingWithCalvin.MCPServer\bin\Release\net48\CodingWithCalvin.MCPServer.vsix"
) else (
    set "VSIX_PATH=%~1"
)

if not exist "%VSIX_PATH%" (
    echo Error: VSIX not found at %VSIX_PATH%
    exit /b 1
)

echo Installing VSIX to: %VS_PATH%
"%VS_PATH%\Common7\IDE\VSIXInstaller.exe" "%VSIX_PATH%"

endlocal
