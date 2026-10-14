# Stop Conditions

Stop implementation, testing, review, or release packaging when any of these conditions is true:

- A requested change requires an LLM, agent, embedding model, network service, telemetry endpoint, or cloud API in the core pipeline.
- The expert-system-only manifest values cannot be truthfully attested.
- `EXPERT_SYSTEM_PROVENANCE.md` is missing, contradictory, or unable to disclose external tools and fallbacks.
- A required DLC file or mandatory prompt clause is missing.
- Manifest verification, expert-system boundary tests, or DLC validation fails.
- The work would weaken privacy scanning, redaction, deterministic behavior, or local-first operation.
- The requested scope expands beyond v0.2.2 boundary, provenance, manifest, parser registry, templates, tests, and directly related documentation without a new approved DLC.

Do not ship a partial or knowingly non-compliant deliverable. Record the failing condition and leave the release unpublished.

