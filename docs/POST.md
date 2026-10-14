# POST Orientation

POST is the deterministic orientation stage that runs before domain rendering.
It scores observed file types, directory signals, and bounded lexical markers.

The result records:

- selected domain
- selected lens
- source state
- score-based confidence
- observed evidence
- alternative domains

Pack output writes `ORIENTATION.md`, `POST_ORIENTATION.json`,
`MODE_GUIDE.md`, and `AFFORDANCES.md`. Orientation is heuristic guidance, not
semantic understanding or an AI-generated conclusion.

