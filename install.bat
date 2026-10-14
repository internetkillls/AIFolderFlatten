@echo off
setlocal
cd /d "%~dp0"
echo Installing context menu...
set "installScript=%~dp0scripts\install-context-menu.ps1"
if exist "%~dp0install-context-menu.ps1" set "installScript=%~dp0install-context-menu.ps1"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%installScript%"
set "installResult=%ERRORLEVEL%"
if not "%installResult%"=="0" echo Installation failed. See the error above.
pause
exit /b %installResult%
