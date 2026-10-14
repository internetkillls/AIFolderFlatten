# Troubleshooting

## PowerShell blocks script execution

Run only for the current PowerShell process:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

## `dotnet` not found

Install the .NET SDK, then rerun:

```powershell
.\scripts\build-release.ps1
```

## Right-click menu does not appear

Reinstall:

```powershell
cd .\release\win-x64
.\install-context-menu.ps1
```

Then restart Explorer if needed:

```powershell
Stop-Process -Name explorer -Force
Start-Process explorer.exe
```

## Output misses files

Check:

- `.aiignore`
- `.gitignore`
- `--exclude`
- `--exclude-dir`
- `--max-file-bytes`
- `--max-total-chars`
- unsupported extension

Open `SKIPPED.md` or the `Skipped / Missing Context Report`.

## Output too large

Use:

```powershell
--content-level signatures
--split true
--max-total-chars 500000
```

## Private paths are visible

Use:

```powershell
--redact-paths true
```
