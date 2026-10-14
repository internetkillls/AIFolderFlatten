param(
  [string]$DlcRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) "dlc")
)

$ErrorActionPreference = "Stop"

$requiredFiles = @(
  "CODEX_IMPLEMENTATION_PROMPT.md",
  "CODEX_TEST_PROMPT.md",
  "CODEX_REVIEW_PROMPT.md",
  "ACCEPTANCE_CRITERIA.md",
  "STOP_CONDITIONS.md"
)

$requiredPromptSections = @(
  "## Exact Scope",
  "## Forbidden Changes",
  "## Tests To Run",
  "## Expected Files",
  "## Manifest And Provenance Requirements",
  "## Expert-System-Only Boundary"
)

if (-not (Test-Path -LiteralPath $DlcRoot -PathType Container)) {
  throw "DLC root not found: $DlcRoot"
}

$versions = @(Get-ChildItem -LiteralPath $DlcRoot -Directory)
if ($versions.Count -eq 0) {
  throw "No DLC version directories found under: $DlcRoot"
}

foreach ($version in $versions) {
  foreach ($fileName in $requiredFiles) {
    $path = Join-Path $version.FullName $fileName
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
      throw "DLC '$($version.Name)' is missing required file: $fileName"
    }

    if ((Get-Item -LiteralPath $path).Length -eq 0) {
      throw "DLC '$($version.Name)' contains an empty required file: $fileName"
    }
  }

  foreach ($promptName in $requiredFiles[0..2]) {
    $promptPath = Join-Path $version.FullName $promptName
    $content = Get-Content -Raw -LiteralPath $promptPath

    foreach ($section in $requiredPromptSections) {
      if ($content.IndexOf($section, [StringComparison]::Ordinal) -lt 0) {
        throw "DLC '$($version.Name)' prompt '$promptName' is missing section: $section"
      }
    }

    foreach ($boundaryTerm in @("manifest", "provenance", "expert-system-only")) {
      if ($content.IndexOf($boundaryTerm, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "DLC '$($version.Name)' prompt '$promptName' is missing requirement term: $boundaryTerm"
      }
    }
  }
}

Write-Host "DLC validation passed for $($versions.Count) version(s): $($versions.Name -join ', ')" -ForegroundColor Green
