@echo off
setlocal
cd /d "%~dp0"
echo Uninstalling context menu...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\uninstall-context-menu.ps1"
pause
