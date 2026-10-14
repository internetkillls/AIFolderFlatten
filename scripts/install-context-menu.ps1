param([string]$ExePath = "")
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ExePath)) {
  $ExePath = Join-Path $PSScriptRoot "AIFolderFlatten.exe"
  if (-not (Test-Path -LiteralPath $ExePath)) {
    $ExePath = Join-Path (Split-Path -Parent $PSScriptRoot) "release\win-x64\AIFolderFlatten.exe"
  }
}
if (-not (Test-Path -LiteralPath $ExePath)) { throw "Build AIFolderFlatten first, or pass -ExePath." }
$ExePath = (Resolve-Path -LiteralPath $ExePath).Path
$releaseRoot = Split-Path -Parent $ExePath
$WrapperPath = Join-Path $releaseRoot "invoke-multi-flatten.ps1"
$modernManifest = Join-Path $releaseRoot "modern-context\AppxManifest.xml"
foreach ($required in @($WrapperPath, $modernManifest)) {
  if (-not (Test-Path -LiteralPath $required)) { throw "Incomplete release: $required" }
}
$version = & $ExePath --version
if ($LASTEXITCODE -ne 0) { throw "Could not read executable version." }
# Replace only AI Flatten registrations; preserve unrelated Explorer settings.
Get-AppxPackage -Name "AIFolderFlatten.ContextMenu" -ErrorAction SilentlyContinue |
  Remove-AppxPackage -ErrorAction Stop
Add-AppxPackage -Register $modernManifest
$oldKeys = @("AIFolderFlattenOverview", "AIFolderFlattenDebug", "AIFolderFlattenArchitecture",
  "AIFolderFlattenObsidian", "AIFolderFlattenText", "AIFolderFlatten", "AIFolderFlattenFiles")
$allBases = @("HKCU:\Software\Classes\*\shell", "HKCU:\Software\Classes\Directory\shell",
  "HKCU:\Software\Classes\Directory\Background\shell")
foreach ($base in $allBases) {
  foreach ($name in $oldKeys) {
    $key = Join-Path $base $name
    if (Test-Path -LiteralPath $key) { Remove-Item -LiteralPath $key -Recurse -Force }
  }
}
# Background uses %V; file/folder fallback batches Explorer selection.
$registrations = @(
    @{ Base = $allBases[2]; Var = '"%V"'; MultiSelect = $false },
    @{ Base = $allBases[0]; Var = '"%1"'; MultiSelect = $true },
    @{ Base = $allBases[1]; Var = '"%1"'; MultiSelect = $true }
)
foreach ($reg in $registrations) {
  $key = Join-Path $reg.Base "AIFolderFlatten"
  $commandKey = Join-Path $key "command"
  $subKey = $commandKey -replace '^HKCU:\\', ''
  $createdKey = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($subKey)
  $createdKey.Dispose()
  New-ItemProperty -LiteralPath $key -Name "MUIVerb" -Value "AI Flatten - Complete Source Snapshot" -PropertyType String -Force | Out-Null
  New-ItemProperty -LiteralPath $key -Name "Icon" -Value "imageres.dll,-102" -PropertyType String -Force | Out-Null
  New-ItemProperty -LiteralPath $key -Name "NeverDefault" -Value "" -PropertyType String -Force | Out-Null
  if ($reg.MultiSelect) {
    New-ItemProperty -LiteralPath $key -Name "MultiSelectModel" -Value "Player" -PropertyType String -Force | Out-Null
  }
  $command = "powershell.exe -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$WrapperPath`" $($reg.Var)"
  Set-Item -LiteralPath $commandKey -Value $command
}
Write-Host "$version installed: Complete Source Snapshot only." -ForegroundColor Green
Write-Host "Files, folders and multi-select: AI Flatten > Complete Source Snapshot."
Write-Host "Folder background: AI Flatten - Complete Source Snapshot (Show more options)."
Write-Host "Output: new SOURCE_SNAPSHOT folder beside the selection's source root."
Write-Host "The completion dialog reports coverage, verification and the output location."
