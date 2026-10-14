$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $Root "src\AIFolderFlatten\AIFolderFlatten.csproj"
$Release = Join-Path $Root "release\win-x64"
$DlcRoot = Join-Path $Root "dlc"
$DlcValidator = Join-Path $PSScriptRoot "validate-dlc.ps1"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  throw "dotnet SDK not found. Install .NET SDK, then rerun this script."
}

& $DlcValidator -DlcRoot $DlcRoot

if (Test-Path $Release) {
  $ResolvedRelease = (Resolve-Path -LiteralPath $Release).Path
  $ExpectedRelease = [IO.Path]::GetFullPath((Join-Path $Root "release\win-x64"))
  if ($ResolvedRelease -ne $ExpectedRelease -or (Get-Item -LiteralPath $Release).Attributes.HasFlag([IO.FileAttributes]::ReparsePoint)) {
    throw "Refusing to remove unexpected release directory: $ResolvedRelease"
  }
  Remove-Item -LiteralPath $ResolvedRelease -Recurse -Force
}

New-Item -ItemType Directory -Path $Release -Force | Out-Null

dotnet publish $Project `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishReadyToRun=true `
  -o $Release

if ($LASTEXITCODE -ne 0) { throw "Release publish failed with exit code $LASTEXITCODE." }

Copy-Item -LiteralPath (Join-Path $Root "install.bat") -Destination $Release -Force
Copy-Item (Join-Path $PSScriptRoot "install-context-menu.ps1")   $Release -Force
Copy-Item (Join-Path $PSScriptRoot "uninstall-context-menu.ps1") $Release -Force
Copy-Item (Join-Path $PSScriptRoot "invoke-multi-flatten.ps1")   $Release -Force
Copy-Item $DlcRoot (Join-Path $Release "DLC") -Recurse -Force

# Copy prompt-starters config so bundled templates are available alongside the exe
$StartersSource = Join-Path $Root "config\prompt-starters"
$StartersDest   = Join-Path $Release "config\prompt-starters"
if (Test-Path $StartersSource) {
  New-Item -ItemType Directory -Path $StartersDest -Force | Out-Null
  Copy-Item (Join-Path $StartersSource "*.txt") $StartersDest -Force
}

& (Join-Path $PSScriptRoot "build-modern-context-menu.ps1") -ReleasePath $Release

Write-Host "Build complete: $Release" -ForegroundColor Green
Write-Host "Next: double-click '$Release\install.bat' to install the Explorer menu." -ForegroundColor Cyan
