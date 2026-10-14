# Contributing

Use English for documentation, issues and pull-request descriptions. Describe the
observed behavior, expected behavior and a minimal reproduction with synthetic data.
Do not include private snapshots, credentials, personal paths or customer source.

Run the .NET tests before submitting changes. For snapshot or menu changes, also
build on Windows and run the relevant smoke scripts. Keep the deterministic,
local-only execution boundary intact. Changes to coverage must report exclusions
and preserve integrity verification.

Include the problem, resulting behavior and validation in each pull request.
Contributions are provided under the project's [MIT License](LICENSE).
