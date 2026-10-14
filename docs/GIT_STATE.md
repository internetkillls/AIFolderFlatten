# Git Source State

Git options modify the source state; they are not analysis profiles.

- `--git-ref <ref>` compiles a temporary snapshot of a branch, tag, or commit.
- `--git-ref choose` opens the CLI revision chooser.
- `--git-changed true` limits included text to changed files.
- `--git-staged true` limits included text to staged files.
- `--git-summary true` enables Git summary information.

Pack output can include:

- `GIT/GIT_STATE.md`
- `GIT/DELTA_CONTEXT.md`
- `GIT/CHANGED_FILES.md`
- `GIT/STAGED_FILES.md`
- `GIT/DIFF_SUMMARY.md`
- `GIT/PATCH_HANDOFF_PROMPT.md`

Interactive `choose` remains CLI-only.

