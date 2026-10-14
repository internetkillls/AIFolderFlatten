$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Fixtures = Join-Path $Root "tests\fixtures"
$Mixed = Join-Path $Fixtures "post-mixed"
$MediaOnly = Join-Path $Fixtures "media-only"
$ResearchDocs = Join-Path $Fixtures "research-docs"

# Recreate folders
if (Test-Path $Mixed) { Remove-Item $Mixed -Recurse -Force }
if (Test-Path $MediaOnly) { Remove-Item $MediaOnly -Recurse -Force }
if (Test-Path $ResearchDocs) { Remove-Item $ResearchDocs -Recurse -Force }

# 1. Recreate post-mixed
New-Item -ItemType Directory -Path (Join-Path $Mixed "notes") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Mixed "docs") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Mixed "papers") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Mixed "books") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Mixed "math") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Mixed "citations") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Mixed "media") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Mixed "src") -Force | Out-Null

Set-Content -Path (Join-Path $Mixed "README.md") -Value "# Mixed Post Project Seed"
Set-Content -Path (Join-Path $Mixed "notes/research.md") -Value "We apply purposive sampling. This implies hypothesis."
Set-Content -Path (Join-Path $Mixed "docs/sample.html") -Value "<html><head><title>HTML Title</title></head><body><h1>Heading</h1></body></html>"
Set-Content -Path (Join-Path $Mixed "papers/sample.pdf") -Value "%PDF-1.4`n%`n1 0 obj`n<< /Title (Sample PDF Title) /Author (John Doe) >>`nendobj"
Set-Content -Path (Join-Path $Mixed "math/model.tex") -Value "\documentclass{article}\begin{document}\section{Heat PDE}\begin{equation}T_t = \alpha \nabla^2 T\end{equation}\cite{einstein1905}\end{document}"
Set-Content -Path (Join-Path $Mixed "citations/sample.bib") -Value "@article{einstein1905, author={Einstein, A.}, title={Energy}, year={1905}}"
[System.IO.File]::WriteAllBytes((Join-Path $Mixed "media/ref.mp4"), (New-Object byte[] 100))
[System.IO.File]::WriteAllBytes((Join-Path $Mixed "media/hit.wav"), (New-Object byte[] 100))
Set-Content -Path (Join-Path $Mixed "src/example.cs") -Value "using System; class Program { static void Main() {} }"

# EPUB Zip
$TempEpub = Join-Path $env:TEMP "EpubTemp_$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path (Join-Path $TempEpub "META-INF") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $TempEpub "OEBPS") -Force | Out-Null
Set-Content -Path (Join-Path $TempEpub "META-INF/container.xml") -Value "<container version='1.0' xmlns='urn:oasis:names:tc:opendocument:xmlns:container'><rootfiles><rootfile full-path='OEBPS/content.opf' media-type='application/oebps-package+xml'/></rootfiles></container>"
Set-Content -Path (Join-Path $TempEpub "OEBPS/content.opf") -Value "<package xmlns='http://www.idpf.org/2007/opf' version='3.0'><metadata xmlns:dc='http://purl.org/dc/elements/1.1/'><dc:title>EPUB Book Title</dc:title><dc:creator>Jane Smith</dc:creator></metadata></package>"
$TempZip = Join-Path $env:TEMP "EpubTempZip_$([Guid]::NewGuid().ToString('N')).zip"
Compress-Archive -Path "$TempEpub\*" -DestinationPath $TempZip -Force
Move-Item -Path $TempZip -Destination (Join-Path $Mixed "books/sample.epub") -Force
Remove-Item $TempEpub -Recurse -Force

# 2. Create media-only
New-Item -ItemType Directory -Path $MediaOnly -Force | Out-Null
[System.IO.File]::WriteAllBytes((Join-Path $MediaOnly "audio.wav"), (New-Object byte[] 100))
[System.IO.File]::WriteAllBytes((Join-Path $MediaOnly "video.mp4"), (New-Object byte[] 100))
[System.IO.File]::WriteAllBytes((Join-Path $MediaOnly "model.blend"), (New-Object byte[] 100))

# 3. Create research-docs
New-Item -ItemType Directory -Path $ResearchDocs -Force | Out-Null
Set-Content -Path (Join-Path $ResearchDocs "paper.pdf") -Value "%PDF-1.4`n%`n1 0 obj`n<< /Title (Paper PDF) /Author (Scholar) >>`nendobj"
Set-Content -Path (Join-Path $ResearchDocs "ref.bib") -Value "@article{ref1, author={Author}, title={Title}, year={2026}}"
Set-Content -Path (Join-Path $ResearchDocs "notes.md") -Value "This is a research notes file with hypothesis."

# Copy EPUB to research-docs
Copy-Item (Join-Path $Mixed "books/sample.epub") (Join-Path $ResearchDocs "book.epub") -Force

Write-Host "Fixtures successfully created at: $Fixtures" -ForegroundColor Green
