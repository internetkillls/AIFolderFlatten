@echo off
setlocal
cd /d "%~dp0"
echo Repairing context menu (uninstalling first)...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\uninstall-context-menu.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\install-context-menu.ps1"
pause
