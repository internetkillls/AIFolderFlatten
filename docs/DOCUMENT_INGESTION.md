# Document Ingestion

Document handling is local and tiered:

| Format | Behavior |
|---|---|
| HTML/HTM | Extract title, headings, links, description, and text preview; scripts are never executed. |
| PDF | Validate header and ledger metadata only. |
| EPUB | Read ZIP/container and OPF metadata when available. |
| MOBI/AZW/AZW3 | Ledger only; suggests a local external conversion tool. |
| LaTeX | Extract document class, packages, sections, equation candidates, and citation keys. |
| BibTeX/RIS | Parse citation entries with deterministic lightweight rules. |
| Jupyter `.ipynb` | Extract cell counts, Markdown headings, imports, and display-equation candidates. |
| Wolfram `.wl`/`.wls` | Include as text scripts. |
| Mathematica `.nb` | Ledger only unless an external local tool is used. |

Binary document content is never embedded raw.

