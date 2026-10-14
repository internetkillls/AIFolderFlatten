param(
  [Parameter(Mandatory = $true)]
  [string]$SelectedPath,
  [switch]$InvokeFirst
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "native\AIFolderFlatten.ContextMenu.Smoke\AIFolderFlatten.ContextMenu.Smoke.vcxproj"
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path -LiteralPath $msbuild)) {
  throw "Visual C++ MSBuild was not found."
}
if (-not (Test-Path -LiteralPath $SelectedPath)) {
  throw "Selected path not found: $SelectedPath"
}

& $msbuild $project /m /p:Configuration=Release /p:Platform=x64 /nologo /verbosity:minimal
if ($LASTEXITCODE -ne 0) {
  throw "Modern context-menu smoke-test build failed."
}

$smoke = Join-Path $root "native\AIFolderFlatten.ContextMenu.Smoke\bin\x64\Release\AIFolderFlatten.ContextMenu.Smoke.exe"
$arguments = @($SelectedPath)
if ($InvokeFirst) {
  $arguments += "--invoke-first"
}

& $smoke @arguments
if ($LASTEXITCODE -ne 0) {
  throw "Modern context-menu smoke test failed with exit code $LASTEXITCODE."
}
