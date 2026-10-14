# AIFolderFlatten v0.2.1 Unified Grand Build Contract

> **Historical baseline:** This contract records the v0.2.1 consolidation
> target. The current product version is v0.2.3. Later v0.2.2 expert-system
> hardening and v0.2.3 development/Git DLCs supersede version strings and
> interpretive output filenames. Current operational behavior is documented in
> `README.md` and the other current files under `docs/`.

**Codename:** POST Unified Context Compiler  
**Status:** Build Contract / Single Source of Truth  
**Target Version:** `0.2.1`  
**Not Target:** `0.3.0`  
**Platform:** Windows 10/11  
**Runtime:** C# / .NET `net8.0-windows`  
**Interface:** CLI + Windows Explorer Context Menu  
**Design Principle:** Local-first, deterministic-first, privacy-first, artifact-preserving.

---

## 0. Executive Summary

AIFolderFlatten v0.2.1 is not merely a folder flattener, not merely a codebase context compiler, and not yet a full 0.3.0 engine. It is the **POST Unified Context Compiler**: a Windows-native local tool that reads a messy folder as a seed, detects what kinds of artifacts it contains, recommends the best orientation, applies the right lens and domain adapter, and outputs an AI-ready context pack.

The central pipeline is:

```text
Folder
  -> Artifact Scanner
  -> Parser Capability Registry
  -> POST Orientation Engine
  -> Lens Selection
  -> Domain Adapter
  -> IR Compilation
  -> Renderer Pack
  -> AI Handoff
```

v0.2.1 unifies all discussed directions:

```text
1. POST Orientation Engine
2. Core Lens System
3. Artifact Ledger
4. Parser Capability Registry
5. Document Ingestion Layer
6. Research Intelligence Layer
7. Mathematical / Modeling Intelligence Layer
8. Creative Reference Layer
9. Git State Selector
10. UX Mode Guide / Recommendation Layer
11. Renderer Pack System
12. Privacy / Redaction / Missing Context System
```

The goal is not to fully parse every possible research/media/math format immediately. The goal is to ensure that every artifact is either parsed, represented as metadata, preserved in a ledger, or explicitly marked as requiring an external tool. Nothing important should silently disappear just because the folder is messy or the extension is unusual.

---

## 1. Critical Version Realignment

The current working source previously drifted toward `0.3.0`. For this build, all source, documentation, manifest output, help text, and changelog must be realigned to `0.2.1`.

### 1.1 Required version targets

```yaml
version:
  product: "0.2.1"
  package_manifest: "0.2.1"
  help_text: "AIFolderFlatten v0.2.1"
  csproj_version: "0.2.1"
  changelog_section: "v0.2.1"
  readme_title: "AIFolderFlatten v0.2.1"
```

### 1.2 Version rules

```text
- No source file may keep AppVersion = "0.3.0".
- No README title may say v0.3.0.
- No generated MANIFEST.json may emit version 0.3.0.
- No Explorer menu installer text may imply this is v0.3.0.
- Roadmap may mention future v0.3.0, but only as future work.
```

### 1.3 Conceptual rule

```text
v0.2.1 is not a downgrade.
v0.2.1 is semantic consolidation before the real v0.3.0.
```

---

## 2. Product Identity

```yaml
project_name: AIFolderFlatten
target_version: 0.2.1
codename: POST Unified Context Compiler
platform:
  - Windows 10
  - Windows 11
runtime:
  language: C#
  framework: net8.0-windows
  winforms: true
interfaces:
  - CLI
  - Windows Explorer context menu
  - PowerShell installer scripts
core_values:
  - local_first
  - deterministic_first
  - privacy_first
  - artifact_preserving
  - AI_handoff_ready
  - no_silent_context_loss
```

---

## 3. Non-Goals for v0.2.1

v0.2.1 must not overreach into major engine territory.

```text
NG-001 No cloud AI API calls.
NG-002 No mandatory embedding engine.
NG-003 No full OCR engine.
NG-004 No full PDF layout engine.
NG-005 No full EPUB/MOBI conversion engine.
NG-006 No base64 embedding of full media.
NG-007 No full DSP/sound reverse engineering engine.
NG-008 No full computer vision motion engine.
NG-009 No full GUI application.
NG-010 No COM Shell Extension.
NG-011 No remote scholarly API fetch by default.
NG-012 No mutation of source folder contents.
```

v0.2.1 may generate ledgers, metadata, extraction summaries, query packs, and IR placeholders for later adapters.

---

## 4. The POST Principle

POST means:

```text
Purpose-Oriented Seed Transform
```

A messy folder is not assumed to be a repo, paper, vault, dataset, or media project. It is treated as a **seed**.

### 4.1 Wrong model

```text
folder -> choose canonical profile -> export
```

### 4.2 Correct v0.2.1 model

```text
folder -> detect artifacts -> infer affordances -> orient purpose -> choose lens -> apply domain pack -> export
```

### 4.3 POST must separate observation from inference

Every auto decision must produce:

```text
Observed evidence
Hypothesis
Confidence
Chosen output
Alternatives
Missing context
```

Example:

```text
Observed:
- Many .md files
- BibTeX file detected
- Terms: method, hypothesis, corpus, provenance
- No strong source-code dominance

Hypothesis:
- This is a research seed folder.

Confidence:
- 0.76

Recommended pack:
- Research Intelligence Pack

Alternative:
- Obsidian Pack if the folder is primarily used as a note vault.
```

---

## 5. Core Lens System

The four existing profiles are not enough as product categories, but they are still essential as **core lenses**.

```text
overview     = first-look lens
debug        = full-evidence lens
architecture = structure/dependency lens
obsidian     = knowledge-vault lens
```

They are not domains. Domains are things like research, creative, document, modeling, code, or mixed.

### 5.1 Lens table

| Lens | Use when | Output emphasis | Default content level |
|---|---|---|---|
| `overview` | You are seeing the folder for the first time | summary, file index, project signals | outline/full depending on size |
| `debug` | You need full evidence, errors, line citations, AI coding agent handoff | full content, line numbers, risks | full |
| `architecture` | You need structure, dependencies, modules, signatures | imports, symbols, configs, dependency hints | signatures |
| `obsidian` | You are working with notes, wikilinks, frontmatter, tags, tasks | note graph, headings, links, tags, tasks | full with line numbers |

### 5.2 Why only four lenses?

Because these are cross-domain views:

```text
A research folder can be viewed as overview/debug/architecture/obsidian.
A creative folder can be viewed as overview/debug/architecture/obsidian.
A code folder can be viewed as overview/debug/architecture/obsidian.
A messy POST folder can be viewed as overview/debug/architecture/obsidian.
```

So v0.2.1 should not add twenty random “profiles” at the same level. It should use:

```text
Domain + Lens + Source State
```

Example:

```text
Domain: research
Lens: overview
Source State: working folder
```

or:

```text
Domain: code
Lens: debug
Source State: git changed
```

---

## 6. Domain System

v0.2.1 introduces domain adapters:

```yaml
domains:
  auto:
    purpose: infer best domain
  code:
    purpose: source code / software project context
  obsidian:
    purpose: vault / note graph / markdown knowledge system
  research:
    purpose: research intelligence, method, citation, provenance, gap
  documents:
    purpose: PDF/EPUB/HTML/LaTeX/notebooks/citation libraries
  modeling:
    purpose: math, scientific modeling, equations, variables, assumptions
  creative:
    purpose: sound, motion, visual, media reference seed
  mixed:
    purpose: folders that strongly contain more than one domain
  unknown:
    purpose: safe orientation when confidence is low
```

---

## 7. Source State System

Git is not a profile. Git is a **source-state selector**.

```yaml
source_states:
  working_folder:
    meaning: actual current filesystem, including untracked files
  git_head:
    meaning: committed HEAD snapshot
  git_changed:
    meaning: modified files since HEAD
  git_staged:
    meaning: files staged for commit
```

### 7.1 Git mode guide

```text
Use normal folder when you want the actual current filesystem.
Use Git HEAD when you want a clean committed repository state.
Use Git Changed when you want only modified files.
Use Git Staged when you want what will be committed.
```

### 7.2 Git UX rule

`--git-ref choose` must remain CLI-only. Do not add interactive Git choose to Explorer context menu because Explorer right-click actions cannot reliably handle interactive console prompts.

---

## 8. Windows Explorer UX Contract

v0.2.1 must replace the flat profile-list UX with a task-oriented cascade.

### 8.1 Final Explorer menu

```text
AI Compile Folder
├── Auto
│   ├── Orient This Folder
│   ├── Recommend Best Pack
│   └── Research/Creative/Modeling Auto
├── Core Lens
│   ├── Overview
│   ├── Debug
│   ├── Architecture
│   └── Obsidian
├── Documents
│   ├── HTML/PDF/EPUB/MOBI Ledger
│   ├── Citation Library Pack
│   └── LaTeX/Notebook Pack
├── Research
│   ├── Research Intelligence Pack
│   ├── Method + Inference Pack
│   ├── Provenance + Sampling Pack
│   └── External Debate Query Pack
├── Modeling / Math
│   ├── Mathematical Modeling Pack
│   ├── Thermal Modeling Audit
│   ├── Economics Modeling Audit
│   ├── Statistical Inference Audit
│   ├── Optimization Audit
│   └── Simulation Audit
├── Creative
│   ├── Creative Reference Pack
│   ├── Sound Reverse Engineering Seed
│   ├── Motion Reference Seed
│   └── Script Target Handoff
└── Git State
    ├── HEAD Snapshot
    ├── Changed Files
    └── Staged Files
```

### 8.2 Hover guidance limitation

Windows Explorer registry menus are limited. Rich hover tooltips are not guaranteed. Therefore v0.2.1 must implement guidance through:

```text
1. Descriptive menu labels
2. MODE_GUIDE.md in every pack
3. --explain-choice CLI output
4. Optional GUIDE files in the output pack
```

### 8.3 Required `MODE_GUIDE.md`

Every pack must include a `MODE_GUIDE.md` explaining:

```text
- What mode/lens/domain was selected
- Why it was selected
- When to use overview/debug/architecture/obsidian
- When to use research/creative/modeling/document packs
- When to use Git HEAD/Changed/Staged
- Alternative modes and why they were not chosen
```

---

## 9. CLI Contract

### 9.1 Base CLI

```powershell
AIFolderFlatten.exe --input <folder> [options]
AIFolderFlatten.exe --verify-manifest <manifest-json>
AIFolderFlatten.exe --help
```

### 9.2 New POST options

```text
--auto true|false
--domain auto|code|obsidian|research|documents|modeling|creative|mixed|unknown
--orientation "<free text>"
--explain-choice true|false
--hover-guide true|false
--unknown-policy ledger|skip
```

### 9.3 Lens options

```text
--lens overview|debug|architecture|obsidian
--profile overview|debug|architecture|obsidian|research|prompt-pack
--mode post|overview|debug|architecture|obsidian|research|creative|modeling|documents|full
```

### 9.4 Research options

```text
--research-intelligence true|false
--external-debate off|query-pack
--citation-mode off|metadata|full
--ngram true|false
--method-map true|false
--provenance-map true|false
--feasibility true|false
```

### 9.5 Document options

```text
--document-ingestion true|false
--pdf-mode off|metadata|text|external
--ebook-mode off|metadata|text|external
--html-mode off|structure|full
--latex-mode off|structure|full
--notebook-mode off|structure|full
--citation-library true|false
```

### 9.6 Modeling options

```text
--modeling-intelligence true|false
--math-domain auto|thermal|economics|statistics|optimization|simulation|mixed
--equation-extraction true|false
--unit-detection true|false
--model-audit true|false
```

### 9.7 Creative options

```text
--creative-reference true|false
--media-policy ledger|manifest|sidecar
--sound-seed true|false
--motion-seed true|false
--script-targets true|false
--embed-media none|thumbnails|previews|all
```

Default:

```text
--embed-media none
```

### 9.8 Git options

```text
--git-ref <ref>|choose
--git-staged true|false
--git-changed true|false
--git-summary true|false
```

### 9.9 Example command

```powershell
AIFolderFlatten.exe `
  --input "D:\ResearchSeed" `
  --auto true `
  --domain auto `
  --research-intelligence true `
  --modeling-intelligence true `
  --document-ingestion true `
  --creative-reference true `
  --format pack `
  --external-debate query-pack `
  --media-policy ledger `
  --explain-choice true
```

---

## 10. Runtime State Model

### 10.1 AppState

```csharp
internal sealed record AppState(
    string AppVersion,
    string SourceRoot,
    DateTimeOffset StartedAt,
    AppConfig Config,
    RuntimeState Runtime,
    ScanState Scan,
    OrientationState Orientation,
    ExportState Export
);
```

### 10.2 RuntimeState

```csharp
internal sealed record RuntimeState(
    bool IsWindows,
    bool GitAvailable,
    bool IsGitRepo,
    bool FfprobeAvailable,
    bool PythonAvailable,
    IReadOnlyList<string> Warnings
);
```

### 10.3 ScanState

```csharp
internal sealed record ScanState(
    int TotalFilesSeen,
    int IncludedTextFiles,
    int SkippedFiles,
    int UnknownArtifacts,
    int MediaArtifacts,
    int ResearchSignals,
    int CreativeSignals,
    int CodeSignals,
    int ObsidianSignals,
    int DocumentSignals,
    int ModelingSignals
);
```

### 10.4 OrientationState

```csharp
internal sealed record OrientationState(
    string SelectedDomain,
    string SelectedLens,
    string SelectedSourceState,
    double Confidence,
    IReadOnlyList<OrientationHypothesis> Hypotheses,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> Alternatives
);
```

### 10.5 ExportState

```csharp
internal sealed record ExportState(
    string Format,
    string OutputRoot,
    IReadOnlyList<string> OutputFiles,
    bool OpenAfterExport,
    bool Notify,
    IReadOnlyList<string> Warnings
);
```

---

## 11. Artifact Scanner and Ledger

v0.2.1 must stop treating unsupported extensions as simple skipped noise. Unsupported research/creative/math artifacts must be preserved in the artifact ledger.

### 11.1 Artifact statuses

```text
parsed
metadata_only
ledger_only
external_tool_needed
unsupported_preserved
skipped_by_policy
skipped_for_safety
```

### 11.2 Artifact IR

```json
{
  "kind": "ArtifactIR",
  "version": "0.2.1",
  "id": "A0001",
  "path": "papers/model.pdf",
  "extension": ".pdf",
  "artifact_type": "research_document",
  "parser_tier": "T1_metadata",
  "status": "metadata_only",
  "size_bytes": 0,
  "sha256": "",
  "modified_utc": "",
  "capabilities": [
    "can_extract_metadata",
    "can_preserve_reference"
  ],
  "warnings": []
}
```

### 11.3 Required artifact ledger outputs

```text
ARTIFACT_LEDGER.md
PARSER_CAPABILITY.md
UNKNOWN_ARTIFACTS.md
```

---

## 12. Parser Capability Registry

v0.2.1 must implement a registry that says what each extension can do.

### 12.1 Parser tiers

```text
T0 ledger_only
T1 metadata
T2 text_extract
T3 structural_extract
T4 domain_ir
```

### 12.2 Registry example

```json
{
  ".html": {
    "artifact_type": "document",
    "tier": "T3_structural_extract",
    "parser": "HtmlDocumentParser"
  },
  ".pdf": {
    "artifact_type": "document",
    "tier": "T1_metadata_or_T2_text_optional",
    "parser": "PdfDocumentAdapter",
    "requires_optional_dependency": true
  },
  ".epub": {
    "artifact_type": "ebook",
    "tier": "T2_text_optional",
    "parser": "EpubDocumentAdapter",
    "requires_optional_dependency": true
  },
  ".mobi": {
    "artifact_type": "ebook",
    "tier": "T0_ledger_only",
    "parser": null,
    "external_tool_suggested": "ebook-convert"
  },
  ".tex": {
    "artifact_type": "latex",
    "tier": "T3_structural_extract",
    "parser": "LatexDocumentParser"
  },
  ".bib": {
    "artifact_type": "citation_library",
    "tier": "T3_structural_extract",
    "parser": "BibTeXParser"
  },
  ".ris": {
    "artifact_type": "citation_library",
    "tier": "T3_structural_extract",
    "parser": "RisParser"
  },
  ".ipynb": {
    "artifact_type": "notebook",
    "tier": "T3_structural_extract",
    "parser": "JupyterNotebookParser"
  },
  ".nb": {
    "artifact_type": "wolfram_notebook",
    "tier": "T0_ledger_only",
    "parser": null
  },
  ".wl": {
    "artifact_type": "wolfram_script",
    "tier": "T2_text_extract",
    "parser": "WolframScriptLiteParser"
  }
}
```

---

## 13. Format Support Matrix

### 13.1 Already supported text/code extensions

Keep current support for:

```text
.md, .markdown, .txt, .cs, .csproj, .sln, .json, .jsonl,
.yaml, .yml, .xml, .html, .htm, .css, .scss, .js, .jsx,
.ts, .tsx, .py, .ps1, .bat, .cmd, .sh, .sql, .ini, .cfg,
.toml, .csv, .log, .java, .kt, .kts, .swift, .cpp, .c, .h,
.hpp, .go, .rs, .php, .rb, .r, .dockerfile, .gitignore, .aiignore
```

### 13.2 Required new research/document formats

```text
.pdf, .epub, .mobi, .azw, .azw3, .docx, .odt,
.tex, .bib, .ris, .csl.json, .rdf,
.ipynb, .qmd, .Rmd,
.nb, .wl, .wls,
.xlsx, .xls, .parquet, .feather,
.h5, .hdf5, .nc, .netcdf, .mat,
.sav, .dta, .rds, .rda,
.geojson, .shp, .gpkg, .kml
```

### 13.3 Required media/creative formats

```text
.wav, .mp3, .flac, .aiff, .m4a,
.mp4, .mov, .mkv, .webm,
.png, .jpg, .jpeg, .webp, .svg,
.blend, .pd, .scd
```

v0.2.1 should ledger these and infer affordances. It does not need to fully parse all of them.

---

## 14. Document Ingestion Layer

### 14.1 Document IR

```json
{
  "kind": "DocumentIR",
  "version": "0.2.1",
  "artifact_id": "A0001",
  "document_type": "pdf|epub|html|docx|latex|notebook|unknown",
  "title": "",
  "authors": [],
  "date": "",
  "abstract": "",
  "sections": [],
  "headings": [],
  "links": [],
  "citations": [],
  "figures": [],
  "tables": [],
  "equations": [],
  "extraction_quality": {
    "text_available": false,
    "layout_preserved": false,
    "ocr_needed": false,
    "confidence": 0.0
  }
}
```

### 14.2 HTML parser

HTML must be treated as a document, not just text.

Extract:

```text
title
meta description
headings
links
body text preview
script/style stripping
```

Security rule:

```text
Never execute HTML scripts.
Never fetch remote links.
```

### 14.3 PDF adapter

v0.2.1 behavior:

```text
- Always ledger PDF path, size, hash, modified time.
- If text extraction adapter is available, extract text layer.
- If not, mark as metadata_only or external_tool_needed.
- Do not OCR by default.
- Do not base64 PDF.
```

### 14.4 EPUB adapter

v0.2.1 behavior:

```text
- Ledger EPUB.
- If safe unzip/text adapter exists, extract metadata and chapter headings/text preview.
- If not, mark external_tool_needed.
```

### 14.5 MOBI/AZW adapter

v0.2.1 behavior:

```text
- Ledger only by default.
- Suggest external converter if user wants text extraction.
- Do not silently skip.
```

### 14.6 LaTeX parser

Extract:

```text
documentclass
packages
sections/subsections
equations
labels
refs
citations
theorem/lemma/proof environments
bibliography commands
```

### 14.7 Notebook parser

For `.ipynb`, extract:

```text
markdown cells
code cells
execution counts
imports
outputs metadata
LaTeX equations in markdown
parameters/config cells
```

Outputs must warn:

```text
Notebook outputs are generated artifacts and may be stale.
```

---

## 15. Research Intelligence Layer

Research mode must reverse-engineer the folder as a research seed.

### 15.1 Research outputs

```text
RESEARCH_INTELLIGENCE.md
TERM_BANK.md
CAMP_LEXICON.md
NGRAMS.md
METHOD_MAP.md
INFERENCE_MAP.md
HYPOTHESIS_MAP.md
PROVENANCE_MAP.md
CITATION_ECOLOGY.md
DATA_RESOURCE_MAP.md
SAMPLING_MAP.md
TECH_STACK_MAP.md
STATE_OF_THE_ART_QUERIES.md
GAP_AND_CONTRIBUTION.md
FEASIBILITY.md
RESEARCH_INTELLIGENCE.json
```

### 15.2 Research Intelligence IR

```json
{
  "kind": "ResearchIntelligenceIR",
  "version": "0.2.1",
  "documents": [],
  "citations": [],
  "term_bank": {
    "unigrams": [],
    "bigrams": [],
    "trigrams": [],
    "domain_terms": [],
    "native_camp_terms": [],
    "mathematical_terms": [],
    "modeling_terms": []
  },
  "method_map": [],
  "inference_map": [],
  "hypothesis_map": [],
  "provenance_map": [],
  "data_resource_map": [],
  "sampling_map": [],
  "technology_stack": [],
  "modeling_map": [],
  "state_of_the_art_queries": [],
  "gap_and_contribution": [],
  "feasibility": []
}
```

### 15.3 N-gram requirements

N-gram is not just frequency. It must expose community wording.

Extract:

```text
unigram
bigram
trigram
collocation
method phrase
inference phrase
hypothesis phrase
provenance phrase
camp marker
technology phrase
mathematical phrase
modeling phrase
```

### 15.4 Research method map

```markdown
| Method Signal | Evidence | Explicit/Implicit | Risk |
|---|---|---|---|
| n-gram analysis | F0007:L20-L40 | explicit | needs corpus cleaning |
| purposive sampling | F0012:L88-L94 | implicit | criteria unclear |
```

### 15.5 Inference map

```markdown
| Claim | Inference Type | Evidence | Strength | Missing |
|---|---|---|---|---|
| folder targets DH/HCI | abductive | term bank + file types | medium | external debate check |
```

### 15.6 Feasibility map

```markdown
| Dimension | Score | Reason | Risk | Mitigation |
|---|---:|---|---|---|
| Academic | 0.72 | gap plausible | external debate not checked | generate query pack |
| Effort | 0.58 | many parsers | scope creep | POST-first pack |
| Financial | 0.81 | local/open tools | paid papers/APIs | use metadata-first |
| Technical | 0.68 | C# + optional Python feasible | NLP complexity | deterministic heuristics first |
```

### 15.7 External debate mode

v0.2.1 must not browse or fetch online metadata itself. It outputs a query pack:

```text
STATE_OF_THE_ART_QUERIES.md
```

The query pack should include:

```text
- Google Scholar style queries
- OpenAlex query seeds
- Crossref query seeds
- DOI lookup candidates
- camp vocabulary expansion queries
- state-of-the-art check prompts
- research gap validation prompts
```

---

## 16. Mathematical / Modeling Intelligence Layer

Mathematical and scientific research tasks must be first-class.

### 16.1 Modeling IR

```json
{
  "kind": "ModelingIR",
  "version": "0.2.1",
  "model_domain": "thermal|economics|network|statistical|ml|simulation|optimization|unknown",
  "questions": [],
  "variables": [
    {
      "symbol": "T",
      "name": "temperature",
      "unit": "K|C|unknown",
      "role": "state|parameter|observation|control|output",
      "source": "F0003:L20-L25"
    }
  ],
  "parameters": [],
  "observations": [],
  "equations": [],
  "assumptions": [],
  "boundary_conditions": [],
  "initial_conditions": [],
  "objective_functions": [],
  "constraints": [],
  "uncertainty": [],
  "validation": [],
  "software_stack": []
}
```

### 16.2 Universal modeling rule

Every model must expose:

```text
question
variables
units
assumptions
parameters
observations
equations
data provenance
method
uncertainty
validation
feasibility
```

### 16.3 Modeling outputs

```text
MODELING_ORIENTATION.md
EQUATIONS.md
VARIABLES.md
UNITS.md
ASSUMPTIONS.md
PARAMETERS.md
OBSERVATIONS.md
MODEL_STRUCTURE.md
THERMAL_MODEL_MAP.md
ECON_MODEL_MAP.md
STATISTICAL_INFERENCE.md
OPTIMIZATION_MAP.md
SIMULATION_MAP.md
MODELING_IR.json
```

### 16.4 Thermal modeling audit

Detect:

```text
heat
temperature
conduction
convection
radiation
boundary condition
initial condition
finite difference
finite element
PDE
thermal diffusivity
heat flux
```

Flag missing:

```text
- boundary conditions
- initial conditions
- units
- material parameters
- geometry/domain
- validation data
```

### 16.5 Economics modeling audit

Detect:

```text
utility
elasticity
demand
supply
equilibrium
regression
panel data
difference-in-differences
instrumental variable
DSGE
game theory
market clearing
welfare
```

Flag missing:

```text
- sampling frame
- identification strategy
- variable definitions
- data provenance
- robustness checks
- financial feasibility
```

### 16.6 Statistical inference audit

Detect:

```text
hypothesis
null hypothesis
confidence interval
p-value
posterior
likelihood
estimator
standard error
robustness
Bayesian
frequentist
causal inference
```

Flag missing:

```text
- hypothesis declaration
- estimator choice
- uncertainty model
- sample size
- assumptions
- robustness checks
```

### 16.7 Optimization audit

Detect:

```text
objective
constraint
minimize
maximize
linear programming
convex
nonconvex
gradient
Lagrangian
penalty
regularization
```

Flag missing:

```text
- objective function
- decision variables
- constraints
- solver
- convergence criteria
```

### 16.8 Simulation audit

Detect:

```text
Monte Carlo
agent-based
system dynamics
sensitivity analysis
scenario
calibration
validation
random seed
stochastic
```

Flag missing:

```text
- scenario assumptions
- random seed
- calibration data
- sensitivity analysis
- validation method
```

---

## 17. Creative Reference Layer

Creative mode in v0.2.1 is a **reference seed compiler**, not a full media analyzer.

### 17.1 Creative outputs

```text
CREATIVE_REFERENCE.md
MEDIA_LEDGER.md
SOUND_AFFORDANCES.md
MOTION_AFFORDANCES.md
STATIC_TO_MOTION_SEED.md
SCRIPT_TARGETS.md
CREATIVE_REFERENCE.json
```

### 17.2 Creative Reference IR

```json
{
  "kind": "CreativeReferenceIR",
  "version": "0.2.1",
  "reference_assets": [],
  "sound_affordances": [],
  "motion_affordances": [],
  "static_visual_affordances": [],
  "script_target_candidates": [
    "p5.js",
    "three.js",
    "blender-python",
    "supercollider",
    "puredata",
    "python-audio",
    "vst-parameter-map"
  ],
  "no_full_media_embedding": true,
  "media_handling": "ledger-only-in-0.2.1"
}
```

### 17.3 Media policy

```text
- Do not base64 full audio/video/image assets.
- Do not embed heavy media by default.
- Keep path, size, extension, hash, and affordance.
- Mark optional future tools: ffprobe, waveform, spectrogram, keyframes, optical flow.
- Provide script target hints, not full script generation unless enough text/context exists.
```

### 17.4 Script target table

```markdown
| Target | Use |
|---|---|
| p5.js | generative poster, browser motion |
| Three.js | 3D web motion |
| Blender Python | 3D animation/keyframes |
| SuperCollider | synthesis/sound design |
| PureData | modular patch |
| Python Audio | prototyping DSP |
| VST Parameter Map | future plugin scaffold |
```

---

## 18. POST Orientation Engine

### 18.1 Domain signals

#### Code signals

```text
.csproj, .sln, package.json, pyproject.toml, Cargo.toml, go.mod,
src/, tests/, Program.Main, imports, using, function, class
```

#### Obsidian signals

```text
.obsidian/, wikilinks, frontmatter, tags, tasks, dataview, callouts
```

#### Research signals

```text
citation, DOI, bibliography, method, hypothesis, sample, corpus, dataset,
provenance, theory, literature review, abstract, references, finding, limitation
```

#### Document signals

```text
.pdf, .epub, .mobi, .docx, .html, .tex, .bib, .ris, .ipynb
```

#### Modeling signals

```text
equation, variable, unit, model, simulation, parameter, objective, constraint,
thermal, economics, regression, optimization, PDE, likelihood, posterior
```

#### Creative signals

```text
.wav, .mp3, .flac, .mp4, .mov, .webm, .png, .jpg, reference, moodboard,
motion, synth, sound, timbre, storyboard, blender, puredata, supercollider
```

### 18.2 Scoring formula

```text
domain_score =
  file_type_score
+ lexical_signal_score
+ structure_score
+ metadata_score
+ user_orientation_score
- noise_penalty
```

### 18.3 Output

```text
ORIENTATION.md
POST_ORIENTATION.json
MODE_GUIDE.md
```

---

## 19. Output Pack Contract

### 19.1 Unified pack root

```text
POST_CONTEXT_PACK/
├── CONTEXT.md
├── FILES.md
├── ARTIFACT_LEDGER.md
├── PARSER_CAPABILITY.md
├── MODE_GUIDE.md
├── ORIENTATION.md
├── AFFORDANCES.md
├── UNKNOWN_ARTIFACTS.md
├── DOCUMENTS.md
├── CITATIONS.md
├── SYMBOLS.md
├── DEPENDENCIES.md
├── RISKS.md
├── SKIPPED.md
├── REDACTIONS.md
├── RESEARCH/
├── MODELING/
├── CREATIVE/
├── GIT/
├── USER_PROMPT.md
├── MANIFEST.json
└── POST_ORIENTATION.json
```

### 19.2 Research subfolder

```text
RESEARCH/
├── RESEARCH_INTELLIGENCE.md
├── TERM_BANK.md
├── CAMP_LEXICON.md
├── NGRAMS.md
├── METHOD_MAP.md
├── INFERENCE_MAP.md
├── HYPOTHESIS_MAP.md
├── PROVENANCE_MAP.md
├── DATA_RESOURCE_MAP.md
├── SAMPLING_MAP.md
├── TECH_STACK_MAP.md
├── STATE_OF_THE_ART_QUERIES.md
├── GAP_AND_CONTRIBUTION.md
├── FEASIBILITY.md
└── RESEARCH_INTELLIGENCE.json
```

### 19.3 Modeling subfolder

```text
MODELING/
├── MODELING_ORIENTATION.md
├── EQUATIONS.md
├── VARIABLES.md
├── UNITS.md
├── ASSUMPTIONS.md
├── PARAMETERS.md
├── OBSERVATIONS.md
├── MODEL_STRUCTURE.md
├── THERMAL_MODEL_MAP.md
├── ECON_MODEL_MAP.md
├── STATISTICAL_INFERENCE.md
├── OPTIMIZATION_MAP.md
├── SIMULATION_MAP.md
└── MODELING_IR.json
```

### 19.4 Creative subfolder

```text
CREATIVE/
├── CREATIVE_REFERENCE.md
├── MEDIA_LEDGER.md
├── SOUND_AFFORDANCES.md
├── MOTION_AFFORDANCES.md
├── STATIC_TO_MOTION_SEED.md
├── SCRIPT_TARGETS.md
└── CREATIVE_REFERENCE.json
```

### 19.5 Git subfolder

```text
GIT/
└── GIT_STATE.md
```

---

## 20. Required Source File Structure

```text
src/AIFolderFlatten/
├── Program.cs
├── AppConfig.cs
├── Models.cs
├── ExitCodes.cs
├── ExportWriter.cs
├── FolderScanner.cs
├── FileText.cs
├── IgnoreRules.cs
├── Redactor.cs
├── PrivacyScanner.cs
├── GitContext.cs
├── ManifestVerifier.cs
├── MarkdownRenderer.cs
├── TextRenderer.cs
├── Post/
│   ├── PostOrientationEngine.cs
│   ├── AffordanceDetector.cs
│   ├── ModeGuideRenderer.cs
│   └── OrientationModels.cs
├── Artifacts/
│   ├── ArtifactModels.cs
│   ├── ArtifactScanner.cs
│   ├── ArtifactClassifier.cs
│   ├── ParserCapabilityRegistry.cs
│   └── UnknownArtifactLedger.cs
├── Documents/
│   ├── DocumentModels.cs
│   ├── HtmlDocumentParser.cs
│   ├── PdfDocumentAdapter.cs
│   ├── EpubDocumentAdapter.cs
│   ├── MobiDocumentAdapter.cs
│   ├── LatexDocumentParser.cs
│   ├── CitationParser.cs
│   └── DocumentRenderer.cs
├── Research/
│   ├── ResearchIntelligenceAnalyzer.cs
│   ├── NGramAnalyzer.cs
│   ├── MethodSignalExtractor.cs
│   ├── InferenceSignalExtractor.cs
│   ├── ProvenanceSignalExtractor.cs
│   ├── SamplingSignalExtractor.cs
│   ├── TechnologySignalExtractor.cs
│   └── ResearchRenderer.cs
├── Modeling/
│   ├── ModelingModels.cs
│   ├── EquationExtractor.cs
│   ├── VariableDictionaryBuilder.cs
│   ├── UnitDetector.cs
│   ├── ModelDomainClassifier.cs
│   ├── ThermalModelAnalyzer.cs
│   ├── EconomicsModelAnalyzer.cs
│   ├── StatisticalInferenceAnalyzer.cs
│   ├── OptimizationAnalyzer.cs
│   ├── SimulationAnalyzer.cs
│   └── ModelingRenderer.cs
└── Creative/
    ├── CreativeReferenceAnalyzer.cs
    ├── MediaLedgerBuilder.cs
    ├── SoundAffordanceDetector.cs
    ├── MotionAffordanceDetector.cs
    └── CreativeRenderer.cs
```

---

## 21. Documentation Requirements

```text
README.md
CHANGELOG.md
docs/ARCHITECTURE.md
docs/POST.md
docs/UX_MODE_GUIDE.md
docs/EXPLORER_MENU.md
docs/DOCUMENT_INGESTION.md
docs/RESEARCH_INTELLIGENCE.md
docs/MODELING_INTELLIGENCE.md
docs/CREATIVE_REFERENCE.md
docs/GIT_STATE.md
docs/SECURITY.md
docs/TESTING.md
docs/TROUBLESHOOTING.md
docs/ROADMAP.md
```

---

## 22. Tests

### 22.1 Required test suites

```text
PostOrientationTests.cs
ArtifactScannerTests.cs
ParserCapabilityRegistryTests.cs
HtmlDocumentParserTests.cs
LatexDocumentParserTests.cs
CitationParserTests.cs
NGramAnalyzerTests.cs
ResearchSignalTests.cs
ModelingAnalyzerTests.cs
CreativeAffordanceTests.cs
ExplorerMenuScriptTests.cs
GitStateTests.cs
ManifestTests.cs
PrivacyTests.cs
RendererTests.cs
```

### 22.2 Required positive tests

```text
T-001 Version emits 0.2.1.
T-002 Auto mode creates ORIENTATION.md.
T-003 Mode guide explains overview/debug/architecture/obsidian.
T-004 Unknown extension enters UNKNOWN_ARTIFACTS.md.
T-005 HTML title/headings/links are extracted.
T-006 PDF is ledgered even without parser.
T-007 EPUB is ledgered even without parser.
T-008 MOBI is ledgered and marked external_tool_needed.
T-009 LaTeX sections/equations/citations are extracted.
T-010 BibTeX entries are parsed.
T-011 RIS entries are parsed.
T-012 IPYNB markdown/code cells are extracted.
T-013 Research pack emits term/method/provenance/gap files.
T-014 Modeling pack emits variables/equations/units/assumptions.
T-015 Thermal audit detects thermal signals.
T-016 Economics audit detects economics signals.
T-017 Creative pack emits media ledger and script targets.
T-018 Git changed/staged modes work in repo.
T-019 Git modes fail clearly outside repo.
T-020 No media or scientific binary is base64 embedded.
```

### 22.3 Required negative tests

```text
NEG-001 PDF without text layer must not be marked fully parsed.
NEG-002 EPUB parse failure falls back to ledger.
NEG-003 MOBI is not silently skipped.
NEG-004 HTML scripts are not executed.
NEG-005 Malformed BibTeX does not crash.
NEG-006 Equation without variable definitions is flagged.
NEG-007 Variable without unit is flagged when numeric model is implied.
NEG-008 Parameter and observation are not merged.
NEG-009 Thermal model without boundary conditions is incomplete.
NEG-010 Economics model without data provenance is weak.
NEG-011 Optimization without objective/constraints is incomplete.
NEG-012 Simulation without uncertainty/scenario assumptions is incomplete.
NEG-013 HDF5/NetCDF/Parquet are not embedded raw.
NEG-014 External-debate connected mode is rejected in 0.2.1.
NEG-015 Version 0.3.0 strings must not remain.
```

---

## 23. Acceptance Criteria

```text
AC-001 App prints version 0.2.1.
AC-002 .csproj version is 0.2.1.
AC-003 README and CHANGELOG say 0.2.1.
AC-004 Existing overview/debug/architecture/obsidian still work.
AC-005 Explorer menu shows Auto/Core/Documents/Research/Modeling/Creative/Git groups.
AC-006 Auto Orient creates ORIENTATION.md and MODE_GUIDE.md.
AC-007 Mode recommendation explains why profile/domain/lens was chosen.
AC-008 Unknown files are logged in UNKNOWN_ARTIFACTS.md, not silently lost.
AC-009 HTML is parsed as document, not generic text only.
AC-010 PDF files appear in Artifact Ledger even if text extraction is unavailable.
AC-011 EPUB files appear in Artifact Ledger.
AC-012 MOBI/AZW files are preserved as ledger-only with external-tool-needed status.
AC-013 .tex files are included and equations/citations/sections are extracted.
AC-014 .bib and .ris files are parsed into Citation Ecology.
AC-015 .ipynb files are parsed into Notebook/Modeling context.
AC-016 .wl/.wls files are included as Wolfram scripts.
AC-017 .nb files are ledger-only unless parser exists.
AC-018 Modeling pack outputs equations, variables, units, assumptions, parameters, observations.
AC-019 Thermal modeling audit detects thermal vocabulary and missing boundary-condition signals.
AC-020 Economics modeling audit detects economic modeling vocabulary and inference signals.
AC-021 Statistical inference audit detects hypothesis, estimator, uncertainty, robustness terms.
AC-022 Scientific binary files are never embedded raw.
AC-023 Research Intelligence Pack creates term, method, inference, provenance, gap, feasibility outputs.
AC-024 Creative Reference Pack creates media ledger, affordances, and script target outputs.
AC-025 Git modes are source-state modifiers, not normal profiles.
AC-026 Git choose remains CLI-only.
AC-027 No full media base64 is embedded.
AC-028 External debate is query-pack only, no online API.
AC-029 dotnet build passes.
AC-030 dotnet test passes.
AC-031 Existing manifest verification still passes.
AC-032 Existing privacy strict still passes.
AC-033 Redaction report still works.
AC-034 Documentation explains when to use each mode.
```

---

## 24. Build Commands

```powershell
cd "D:\Path\To\AIFolderFlatten"

dotnet restore
dotnet build
dotnet test

Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\build-release.ps1
```

Install Explorer menu:

```powershell
cd .\release\win-x64
.\install-context-menu.ps1
```

Example POST auto compile:

```powershell
.\AIFolderFlatten.exe `
  --input "D:\SeedFolder" `
  --auto true `
  --domain auto `
  --document-ingestion true `
  --research-intelligence true `
  --modeling-intelligence true `
  --creative-reference true `
  --external-debate query-pack `
  --media-policy ledger `
  --format pack `
  --open true
```

---

## 25. Implementation Order

```text
P1 Version realignment to 0.2.1.
P2 Introduce POST/domain/lens/source-state config model.
P3 Add ArtifactScanner + ParserCapabilityRegistry.
P4 Add UNKNOWN_ARTIFACTS and ARTIFACT_LEDGER output.
P5 Add ModeGuideRenderer.
P6 Add PostOrientationEngine and explain-choice output.
P7 Add document ingestion layer: HTML, PDF ledger, EPUB ledger, MOBI ledger, LaTeX, BibTeX/RIS, IPYNB.
P8 Add Research Intelligence minimal deterministic layer.
P9 Add Modeling Intelligence minimal deterministic layer.
P10 Add Creative Reference minimal deterministic layer.
P11 Redesign Explorer menu cascade.
P12 Update README, CHANGELOG, docs.
P13 Add tests.
P14 Build, test, publish.
P15 Verify generated pack and Explorer menu.
```

---

## 26. Definition of Done

v0.2.1 unified is done only when:

```text
1. Version is consistently 0.2.1.
2. Auto orientation works.
3. Explorer UX is cascaded by purpose, not flat profile list.
4. Overview/debug/architecture/obsidian are explained as core lenses.
5. Research, Creative, Modeling, and Documents are implemented as domain packs.
6. Git is implemented as source-state modifier.
7. MODE_GUIDE.md explains choices.
8. Unknown artifacts are preserved in ledger.
9. Research Intelligence outputs term/method/inference/provenance/gap/feasibility.
10. Document ingestion supports at least ledger-level PDF/EPUB/MOBI and structural HTML/LaTeX/citation/notebook parsing.
11. Modeling Intelligence outputs variables/equations/units/assumptions/parameters/observations.
12. Creative Reference outputs media/sound/motion/script-target affordances.
13. No heavy media or scientific binary is embedded by default.
14. Tests pass.
15. Generated packs are safe, structured, and AI-handoff ready.
```

---

## 27. Final Identity Statement

```text
AIFolderFlatten v0.2.1
= POST Unified Context Compiler
+ Core Lenses
+ Artifact Ledger
+ Document Ingestion
+ Research Intelligence
+ Mathematical / Modeling Intelligence
+ Creative Reference Ledger
+ Git State Selector
+ AI Handoff Pack
```

Not:

```text
folder-to-markdown only
```

Not yet:

```text
full OCR / full media reverse engineering / full scholarly API agent
```

The final rule:

```text
Every artifact must either be parsed, represented by metadata, preserved as unknown, or marked as requiring an external tool.
Nothing research-relevant, creative-relevant, or modeling-relevant should disappear simply because the folder is messy or the extension is unusual.
```
