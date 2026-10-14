param(
  [string]$ReleasePath = "",
  [switch]$Register
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($ReleasePath)) {
  $ReleasePath = Join-Path $root "release\win-x64"
}
$ReleasePath = [IO.Path]::GetFullPath($ReleasePath)

if ($Register) {
  Get-AppxPackage -Name "AIFolderFlatten.ContextMenu" -ErrorAction SilentlyContinue |
    Remove-AppxPackage -ErrorAction SilentlyContinue
}

$project = Join-Path $root "native\AIFolderFlatten.ContextMenu\AIFolderFlatten.ContextMenu.vcxproj"
$manifest = Join-Path $root "packaging\modern-context\AppxManifest.xml"
$msbuildCandidates = @(
  "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe",
  "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
  "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
  "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)
$msbuild = $msbuildCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $msbuild) {
  throw "Visual C++ MSBuild was not found. Install the Desktop development with C++ workload."
}

& $msbuild $project /restore /m /p:Configuration=Release /p:Platform=x64 /nologo /verbosity:minimal
if ($LASTEXITCODE -ne 0) {
  throw "Modern context-menu DLL build failed with exit code $LASTEXITCODE."
}

$packagePath = Join-Path $ReleasePath "modern-context"
$assetsPath = Join-Path $packagePath "Assets"
New-Item -ItemType Directory -Path $assetsPath -Force | Out-Null

$dll = Join-Path $root "native\AIFolderFlatten.ContextMenu\bin\x64\Release\AIFolderFlatten.ContextMenu.dll"
$exe = Join-Path $ReleasePath "AIFolderFlatten.exe"
foreach ($required in @($dll, $exe, $manifest)) {
  if (-not (Test-Path -LiteralPath $required)) {
    throw "Required modern context-menu file not found: $required"
  }
}

Copy-Item -LiteralPath $dll -Destination $packagePath -Force
Copy-Item -LiteralPath $exe -Destination $packagePath -Force
Copy-Item -LiteralPath $manifest -Destination (Join-Path $packagePath "AppxManifest.xml") -Force
Remove-Item -LiteralPath (Join-Path $packagePath "invoke-multi-flatten.ps1") -Force -ErrorAction SilentlyContinue

Add-Type -AssemblyName System.Drawing
function New-Logo {
  param([string]$Path, [int]$Width, [int]$Height)
  $bitmap = New-Object Drawing.Bitmap($Width, $Height)
  $graphics = [Drawing.Graphics]::FromImage($bitmap)
  try {
    $graphics.Clear([Drawing.Color]::FromArgb(25, 88, 180))
    $fontSize = [Math]::Max(10, [Math]::Floor([Math]::Min($Width, $Height) * 0.34))
    $font = New-Object Drawing.Font("Segoe UI", $fontSize, [Drawing.FontStyle]::Bold, [Drawing.GraphicsUnit]::Pixel)
    $brush = [Drawing.Brushes]::White
    $format = New-Object Drawing.StringFormat
    $format.Alignment = [Drawing.StringAlignment]::Center
    $format.LineAlignment = [Drawing.StringAlignment]::Center
    $graphics.DrawString("AI", $font, $brush, [Drawing.RectangleF]::new(0, 0, $Width, $Height), $format)
    $bitmap.Save($Path, [Drawing.Imaging.ImageFormat]::Png)
    $format.Dispose()
    $font.Dispose()
  }
  finally {
    $graphics.Dispose()
    $bitmap.Dispose()
  }
}

New-Logo (Join-Path $assetsPath "StoreLogo.png") 50 50
New-Logo (Join-Path $assetsPath "Square44x44Logo.png") 44 44
New-Logo (Join-Path $assetsPath "Square150x150Logo.png") 150 150

$iconBitmap = [Drawing.Bitmap]::FromFile((Join-Path $assetsPath "Square44x44Logo.png"))
$iconHandle = $iconBitmap.GetHicon()
$icon = [Drawing.Icon]::FromHandle($iconHandle)
$iconStream = [IO.File]::Create((Join-Path $assetsPath "AIFolderFlatten.ico"))
try {
  $icon.Save($iconStream)
}
finally {
  $iconStream.Dispose()
  $icon.Dispose()
  $iconBitmap.Dispose()
  Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class IconHandleCleanup {
  [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr handle);
}
'@ -ErrorAction SilentlyContinue
  [IconHandleCleanup]::DestroyIcon($iconHandle) | Out-Null
}

if ($Register) {
  Add-AppxPackage -Register (Join-Path $packagePath "AppxManifest.xml")
}

Write-Host "Modern context menu staged at: $packagePath" -ForegroundColor Green
