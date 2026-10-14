param(
    [Parameter(Mandatory = $true)] [string]$Executable
)
$ErrorActionPreference = 'Stop'
$exe = (Resolve-Path -LiteralPath $Executable).Path
$root = Join-Path ([IO.Path]::GetTempPath()) ("aif-snapshot-smoke-" + [guid]::NewGuid().ToString('N'))
$source = Join-Path $root 'source'
$out = Join-Path $root 'out'
try {
    New-Item -ItemType Directory -Path $source, $out | Out-Null
    for ($i = 0; $i -lt 40; $i++) {
        [IO.File]::WriteAllText((Join-Path $source ("Module{0:D2}.cs" -f $i)),
            ("// Module {0}`r`n" -f $i) + ('a' * 16000), [Text.UTF8Encoding]::new($false))
    }
    & $exe --input $source --snapshot-complete true --output $out `
        --redact false --max-total-chars 100 --max-file-bytes 100 `
        --split-size-chars 8000
    if ($LASTEXITCODE -ne 0) { throw 'Snapshot export failed.' }
    $snapshot = @(Get-ChildItem -LiteralPath $out -Directory)
    if ($snapshot.Count -ne 1) { throw 'Expected one snapshot directory.' }
    $manifest = Get-Content -LiteralPath (Join-Path $snapshot[0].FullName 'SOURCE_MANIFEST.json') -Raw | ConvertFrom-Json
    if ($manifest.IncludedFiles -ne 40 -or $manifest.LimitingOmissions -ne 0 -or $manifest.Parts.Count -le 1) {
        throw "Incomplete snapshot: included=$($manifest.IncludedFiles), omitted=$($manifest.LimitingOmissions)"
    }
    & $exe --verify-snapshot $snapshot[0].FullName
    if ($LASTEXITCODE -ne 0) { throw 'Clean snapshot verification failed.' }
    Add-Content -LiteralPath (Join-Path $snapshot[0].FullName $manifest.Parts[0]) -Value '{"tampered":true}'
    & $exe --verify-snapshot $snapshot[0].FullName
    if ($LASTEXITCODE -eq 0) { throw 'Corrupted snapshot should have failed verification.' }
    Write-Host 'PASS: complete coverage, chunking, verification, tamper detection.'
}
finally {
    if (Test-Path -LiteralPath $root) {
        $resolvedRoot = (Resolve-Path -LiteralPath $root).Path
        if ($resolvedRoot -ne [IO.Path]::GetFullPath($root) -or
            (Split-Path -Parent $resolvedRoot) -ne [IO.Path]::GetTempPath().TrimEnd('\') -or
            (Get-Item -LiteralPath $resolvedRoot).Attributes.HasFlag([IO.FileAttributes]::ReparsePoint)) {
            throw "Refusing cleanup outside smoke-test directory: $resolvedRoot"
        }
        Remove-Item -LiteralPath $resolvedRoot -Force -Recurse
    }
}
