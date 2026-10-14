# Privacy Audit Report

> ⚠️ **Required Warning**: Review outputs before uploading to any cloud AI.
> The scanner is deterministic and cannot guarantee perfect privacy.
> This report is produced by expert-system pattern matching only.

## Summary

- Files scanned: {{FILES_SCANNED}}
- Redactions applied: {{REDACTION_COUNT}}
- Secret-like patterns detected: {{SECRET_PATTERN_COUNT}}
- Prompt-injection-like patterns detected: {{INJECTION_PATTERN_COUNT}}

## Redaction Details

{{REDACTIONS}}

## Prompt Injection Warnings

{{PROMPT_INJECTION_WARNINGS}}

## Limitations

This audit uses deterministic regex patterns. It cannot detect:
- Novel obfuscated secrets
- Context-dependent sensitive content
- Steganographic data
- LLM-specific prompt injections beyond known patterns
