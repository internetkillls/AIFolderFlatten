@echo off
setlocal
echo Building AIFolderFlatten for Windows...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\build-release.ps1"
set "buildResult=%ERRORLEVEL%"
if not "%buildResult%"=="0" echo Build failed. See the error above.
pause
exit /b %buildResult%
