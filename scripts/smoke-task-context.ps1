param([Parameter(Mandatory=$true)][string]$Executable)
$ErrorActionPreference = 'Stop'
$exe = (Resolve-Path -LiteralPath $Executable).Path
$root = Join-Path ([IO.Path]::GetTempPath()) ('aif-task-smoke-' + [guid]::NewGuid().ToString('N'))
$src = Join-Path $root 'source'
$out = Join-Path $root 'output'
try {
    New-Item -ItemType Directory -Path $src,$out | Out-Null
    [IO.File]::WriteAllText((Join-Path $src 'SettingsTab.cs'), 'public class SettingsTab { void RenderControls() {} }')
    [IO.File]::WriteAllText((Join-Path $src 'SettingsTabTests.cs'), 'class SettingsTabTests { }')
    [IO.File]::WriteAllText((Join-Path $src 'Unrelated.txt'), 'hello')
    & $exe --input $src --output $out --task-context 'Fix SettingsTab controls' --task-budget-chars 20000
    if ($LASTEXITCODE -ne 0) { throw 'Task compile exited unsuccessfully' }
    $contexts = @(Get-ChildItem $out -Directory)
    if ($contexts.Count -ne 1) { throw 'Expected one context directory' }
    $pack = $contexts[0].FullName
    $data = Get-Content (Join-Path $pack 'CONTEXT_DATA.json') -Raw | ConvertFrom-Json
    if ($data.SelectedCount -lt 2) { throw 'Expected relevant settings source and test' }
    if (-not (Test-Path (Join-Path $pack 'CONTEXT_REVIEW.html'))) { throw 'Cockpit absent' }
    & $exe --verify-task-context $pack --input $src
    if ($LASTEXITCODE -ne 0) { throw 'Verification rejected clean source' }
    Add-Content (Join-Path $src 'SettingsTab.cs') 'class Changed {}'
    & $exe --verify-task-context $pack --input $src
    if ($LASTEXITCODE -eq 0) { throw 'Source staleness was not detected' }
    Write-Host 'PASS: task selection, offline HTML, verifier, stale-source detection'
}
finally {
    if (Test-Path -LiteralPath $root) {
        $resolvedRoot = (Resolve-Path -LiteralPath $root).Path
        if ($resolvedRoot -ne [IO.Path]::GetFullPath($root) -or
            (Split-Path -Parent $resolvedRoot) -ne [IO.Path]::GetTempPath().TrimEnd('\') -or
            (Get-Item -LiteralPath $resolvedRoot).Attributes.HasFlag([IO.FileAttributes]::ReparsePoint)) {
            throw "Refusing cleanup outside smoke-test directory: $resolvedRoot"
        }
        Remove-Item -LiteralPath $resolvedRoot -Recurse -Force
    }
}
