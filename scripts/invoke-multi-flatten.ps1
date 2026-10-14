param(
  [string]$PathListFile = "",
  [Parameter(Position = 0, ValueFromRemainingArguments = $true)]
  [string[]]$Paths
)

$ErrorActionPreference = "Stop"

$paths = if (-not [string]::IsNullOrWhiteSpace($PathListFile)) {
  try {
    @(Get-Content -LiteralPath $PathListFile -Encoding Unicode |
      Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
  }
  finally {
    Remove-Item -LiteralPath $PathListFile -Force -ErrorAction SilentlyContinue
  }
} else {
  @($Paths | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}
$logDir = Join-Path $env:LOCALAPPDATA "AIFolderFlatten"
$logPath = Join-Path $logDir "context-menu.log"
$batchDir = Join-Path $logDir "pending"

function Write-ContextMenuError {
  param([string]$Message)
  New-Item -ItemType Directory -Path $logDir -Force | Out-Null
  $stamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
  Add-Content -LiteralPath $logPath -Value "[$stamp] $Message"
}

function Enter-Mutex {
  param(
    [System.Threading.Mutex]$Mutex,
    [int]$TimeoutMilliseconds
  )

  try {
    return $Mutex.WaitOne($TimeoutMilliseconds)
  }
  catch [System.Threading.AbandonedMutexException] {
    return $true
  }
}

function Get-ActionHash {
  $action = "complete-source-snapshot"

  $sha = [System.Security.Cryptography.SHA256]::Create()
  try {
    $bytes = [Text.Encoding]::UTF8.GetBytes($action)
    return ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
  }
  finally {
    $sha.Dispose()
  }
}

function Collect-ExplorerSelection {
  param([string[]]$IncomingPaths)

  New-Item -ItemType Directory -Path $batchDir -Force | Out-Null
  $hash = Get-ActionHash
  $queuePath = Join-Path $batchDir "$hash.queue"
  $appendMutex = New-Object System.Threading.Mutex($false, "Local\AIFolderFlatten.Append.$hash")
  $leaderMutex = New-Object System.Threading.Mutex($false, "Local\AIFolderFlatten.Leader.$hash")
  $appendOwned = $false
  $leaderOwned = $false

  try {
    $appendOwned = Enter-Mutex $appendMutex 5000
    if (-not $appendOwned) {
      throw "Timed out while collecting Explorer selection."
    }

    if ((Test-Path -LiteralPath $queuePath) -and
        ((Get-Date).ToUniversalTime() - (Get-Item -LiteralPath $queuePath).LastWriteTimeUtc).TotalSeconds -gt 10) {
      Remove-Item -LiteralPath $queuePath -Force
    }

    foreach ($path in $IncomingPaths) {
      Add-Content -LiteralPath $queuePath -Value (ConvertTo-Json ([string]$path) -Compress) -Encoding UTF8
    }
  }
  finally {
    if ($appendOwned) {
      $appendMutex.ReleaseMutex()
      $appendOwned = $false
    }
  }

  $leaderOwned = Enter-Mutex $leaderMutex 0
  if (-not $leaderOwned) {
    $appendMutex.Dispose()
    $leaderMutex.Dispose()
    return @()
  }

  try {
    $deadline = (Get-Date).AddSeconds(5)
    do {
      Start-Sleep -Milliseconds 150
      $lastWrite = (Get-Item -LiteralPath $queuePath -ErrorAction SilentlyContinue).LastWriteTimeUtc
      if ($lastWrite -and (((Get-Date).ToUniversalTime() - $lastWrite).TotalMilliseconds -ge 650)) {
        break
      }
    } while ((Get-Date) -lt $deadline)

    $appendOwned = Enter-Mutex $appendMutex 5000
    if (-not $appendOwned) {
      throw "Timed out while finalizing Explorer selection."
    }

    $lines = @(Get-Content -LiteralPath $queuePath -ErrorAction SilentlyContinue)
    Remove-Item -LiteralPath $queuePath -Force -ErrorAction SilentlyContinue

    return @($lines |
      Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
      ForEach-Object { ConvertFrom-Json $_ } |
      Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
      Select-Object -Unique)
  }
  finally {
    if ($appendOwned) {
      $appendMutex.ReleaseMutex()
    }
    if ($leaderOwned) {
      $leaderMutex.ReleaseMutex()
    }
    $appendMutex.Dispose()
    $leaderMutex.Dispose()
  }
}

try {
  if ($paths.Count -eq 0) {
    throw "No paths received from Explorer."
  }

  if ([string]::IsNullOrWhiteSpace($PathListFile)) {
    $paths = @(Collect-ExplorerSelection $paths)
    if ($paths.Count -eq 0) {
      exit 0
    }
  }

  $exe = Join-Path $PSScriptRoot "AIFolderFlatten.exe"
  if (-not (Test-Path -LiteralPath $exe)) {
    $exe = Join-Path (Split-Path -Parent $PSScriptRoot) "release\win-x64\AIFolderFlatten.exe"
  }
  if (-not (Test-Path -LiteralPath $exe)) {
    throw "AIFolderFlatten.exe not found next to this script or in release\win-x64."
  }

  $joined = $paths -join '|'

  $cmdArgs = @("--input", $joined, "--explorer-snapshot", "true", "--notify", "true")

  & $exe @cmdArgs
  if ($LASTEXITCODE -ne 0) {
    throw "AIFolderFlatten.exe exited with code $LASTEXITCODE for: $joined"
  }
}
catch {
  Write-ContextMenuError $_.Exception.Message
  exit 1
}
