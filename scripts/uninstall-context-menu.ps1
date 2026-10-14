$ErrorActionPreference = "Stop"

$keys = @(
  "AIFolderFlattenOverview", "AIFolderFlattenDebug", "AIFolderFlattenArchitecture",
  "AIFolderFlattenObsidian", "AIFolderFlattenText",
  "AIFolderFlatten", "AIFolderFlattenFiles"
)

$bases = @(
  "HKCU:\Software\Classes\*\shell",
  "HKCU:\Software\Classes\Directory\shell",
  "HKCU:\Software\Classes\Directory\Background\shell"
)

foreach ($base in $bases) {
  foreach ($key in $keys) {
    $path = Join-Path $base $key
    if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Recurse -Force; Write-Host "Removed $path" }
  }
}

Get-AppxPackage -Name "AIFolderFlatten.ContextMenu" -ErrorAction SilentlyContinue |
  Remove-AppxPackage -ErrorAction SilentlyContinue

$classicMenuRoot = "HKCU:\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"
if (Test-Path -LiteralPath $classicMenuRoot) {
  Remove-Item -LiteralPath $classicMenuRoot -Recurse -Force
}

Write-Host "AI Flatten context menu entries removed." -ForegroundColor Green
