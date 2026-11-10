# Documentation & Governance Update Plan (implementation-ready)

## Goal
Deliver, in documentation only (no gameplay/code changes): root `agents.md`, root `README.md`, and verified/refined art-direction hierarchy at `docs/art_direction.md` (global master/index, immutable palette, inheritance) and `docs/word_search/art_direction.md` (Word Search-specific metrics/behavior), aligned to the requested SoT hierarchy, SemVer 2.0, Godot 4.x/C#, 360×640 portrait, and static-grid standards.

## Current state (verified)
- **`agents.md` (root):** missing. `AGENTS.md` also absent.
- **`README.md` (root):** missing.
- **`docs/art_direction.md`:** exists, v1.0 global master — has immutable palette matrix, inheritance rule (§7), master index (§8), global components/typography/safe-area. Needs a targeted pass to ensure it is *strictly* global (no minigame-only specifics), explicitly documents inheritance of base colors **and button relief styles and font tokens**, and index/links are complete and clean.
- **`docs/word_search/art_direction.md`:** exists, v1.0 — references global palette/tokens, documents layout, baseline $8\times8$/$30\times30$px/$6$px gap, zoom $+/-$, footer ~70px target, confetti (~300ms). Needs alignment to requested emphasis: “static $8\times8$ visible letter grid, $30\times30$px cells, $6$px gaps” stated clearly as the static/reference baseline, zoom controls $+/-$, WordsPanel/footer ~70px min, confetti using approved palette tokens; keep target-vs-runtime notes accurate (no false precision).
- **SoT inputs:** `docs/spec.md` (global SDD), `docs/word_search/spec.md`, `core/ThemeData.cs`, `core/ui/PixelStyles.cs`, `docs/word_search/wathersoup.jpg`.

## Decisions / assumptions (recommended)
1. **Badges (README):** use Shields.io static/informational badges for Build Status and Test Coverage if CI config is absent (do not invent CI URLs); set Tech Stack `Godot 4 .NET (C#)`, SemVer `2.0.0` (project baseline), License `MIT` (verify `LICENSE` exists; if missing, badge still states MIT per request and add note to add LICENSE file — out of scope to guess terms beyond request). **Recommended:** detect `.github/workflows/*` and `LICENSE` during implementation and wire real links/badges where possible.
2. **`agents.md` location/name:** root `agents.md` (lowercase) as requested; keep concise/token-efficient.
3. **Static grid standard:** document $8\times8$ visible letters, $30\times30$px cells, $6$px gaps as the **static/reference baseline** (Fácil/reference fidelity) while preserving accurate difficulty/runtime-fit notes (8/10/12/15, current fit-to-area behavior) to avoid contradicting implementation/spec.
4. **Art-direction refinements:** preserve existing v1.0 structure/content where correct; apply surgical edits only to satisfy the four directives (global-only focus/master index/inheritance explicit; Word Search metrics/zoom/footer/confetti explicit and palette-token-referenced).

## Implementation tasks (ordered)

### 1) Create `agents.md` (root, concise)
- **Path:** `agents.md`
- **Sections (token-efficient):**
  - **Purpose & scope:** brief agent guidelines for this repo.
  - **Hierarchy of Sources of Truth (SoT):** 1) `docs/spec.md` (global product/spec), 2) `docs/art_direction.md` (global art direction, palette, inheritance), 3) minigame docs `docs/<minigame>/spec.md` and `docs/<minigame>/art_direction.md` (e.g. `docs/word_search/*`). State precedence and “do not contradict higher SoT”.
  - **Engineering guardrails:** Godot **4.x / C# (.NET)**; Clean Architecture & DDD principles; SRP/composition; **strict SemVer 2.0** for releases/versioning; adhere to **immutable global color palette matrix** and visual inheritance (`docs/art_direction.md`); mobile portrait viewport **360×640**; **static grid baseline** $8\times8$ / $30\times30$px / $6$px gap for Word Search reference (difficulty variants per spec, fit without clipping) — treat as standards, not gameplay changes.
  - **Doc/code hygiene:** update docs with behavior/visual decisions; keep changes minimal and verifiable; reference `wathersoup.jpg` for 1:1 visual checks where applicable.
- **Style:** bullets, short, no duplication of full specs.

### 2) Create `README.md` (root)
- **Path:** `README.md`
- **Badges (Shields.io, top):** Build Status, Test Coverage, Tech Stack (`Godot 4`, `C#/.NET`), SemVer `2.0.0`, License `MIT`. Use real workflow/coverage links if discoverable; otherwise clearly labeled placeholders.
- **Overview:** one-paragraph description (Clásicos Mente Activa 2D, accessible mental-agility minigames, Word Search module) consistent with `docs/spec.md`.
- **Architecture flowchart (Mermaid `graph TD`):** connect **Core Managers** (`GameManager`/`SaveManager`/`ThemeManager`/`AudioManager` as applicable), **Theme Engine & Global Art Direction** (`docs/art_direction.md`, tokens/palette/inheritance), to **Word Search minigame module** (`minigames/word_search/*`, scene/grid/HUD) and its **specific art direction/spec** (`docs/word_search/art_direction.md`, `docs/word_search/spec.md`). Keep node labels readable.
- **“Decisiones Clave de Arquitectura”:**
  - **Clean Architecture & DDD:** separation of concerns, domain/game logic vs UI/presentation, composition over heavy coupling (Godot nodes + C#).
  - **Spec-Driven Development (SDD):** specs/art-direction as SoT before/alongside implementation; hierarchical inheritance.
  - **SemVer 2.0:** MAJOR.MINOR.PATCH, compatibility/breaking-change discipline.
- **Quick Start Guide:** prerequisites (Godot 4.x .NET, .NET SDK), clone, open/run in Godot (portrait 360×640), and tests: `dotnet test` (mention `GODOT_BIN` if required by repo conventions — verify in `tests/README.md` or existing docs; include the canonical command form).
- **Docs links:** point to `docs/spec.md`, `docs/art_direction.md`, `docs/word_search/`, and `agents.md`.

### 3) Refine `docs/art_direction.md` (global master/index)
- **Keep:** immutable palette matrix (§4), inheritance rule (§7), master index (§8), global standards/components/typography, theme system — as authoritative.
- **Refinements:**
  - Ensure language states: “strictly global UI standards, universal base components, immutable global color matrix, and **Master Index** referencing all minigame art directions”.
  - Make inheritance explicit: **all minigames inherit base colors, button relief styles, and font tokens** from global (no redefinition); metrics/layout/behavior local.
  - Remove/relocate any remaining Word Search-only specifics if found (keep only pointers/links).
  - Confirm index lists Word Search and states convention for future minigames; verify relative links.
- **No palette/token redefinitions**; only clarification/structuring.

### 4) Refine `docs/word_search/art_direction.md` (minigame-specific)
- **Keep:** inheritance declaration, token-based states/colors, target-vs-runtime honesty, references.
- **Refinements (explicitly document):**
  - **Layout metrics:** static/reference grid **$8\times8$ visible letters, $30\times30$px cells, $6$px gaps** (baseline/fidelity target, e.g. Fácil vs `wathersoup.jpg`); include difficulty variants (10×10/12×12/15×15) with per-difficulty target/fit behavior labeled and no-clipping/hit-test alignment.
  - **Zoom controls:** **`+` / `−`** (and centrar/restablecer) as accessible alternatives; pinch support if applicable; note current implementation status vs target where relevant.
  - **Footer/WordsPanel:** **~70px minimum height** (effective target) and anti-clipping/anchoring in 360×640 safe area; note current vs target if they differ.
  - **Confetti particle behavior:** victory trigger, ~300ms before modal (per spec), pixel-square particles (e.g. 4×4/6×6), colors derived from **approved global palette tokens** only.
  - **Cross-reference** global matrix/tokens and do not restate hex values as local authority.

### 5) Consistency & verification pass
- Validate relative links: `README.md` ↔ `docs/*`, `docs/art_direction.md` ↔ `docs/word_search/art_direction.md`, `agents.md` refs.
- Grep for stale/conflicting statements (e.g. duplicate palette definitions, broken paths); fix in docs only if clearly incorrect.
- Confirm metrics/behaviors marked “verify against `docs/word_search/wathersoup.jpg`/implementation” where not provable from source alone.
- Check `LICENSE`, `tests/README.md`, `.github/workflows/` during implementation to finalize README badges/Quick Start accurately.

## Affected files
- **Create:** `agents.md`, `README.md`
- **Edit (refine):** `docs/art_direction.md`, `docs/word_search/art_direction.md`
- **Read/reference only:** `docs/spec.md`, `docs/word_search/spec.md`, `docs/word_search/plan.md`, `docs/word_search/wathersoup.jpg`, `core/ThemeData.cs`, `core/ui/PixelStyles.cs`, `.github/workflows/*` (if any), `LICENSE` (if any), `tests/README.md` (if any)

## Validation checklist
- [ ] `agents.md` exists, is concise, states SoT hierarchy and all requested guardrails (Godot 4.x/C#, Clean Architecture, DDD, strict SemVer 2.0, immutable global palette adherence, 360×640 portrait, static grid baseline).
- [ ] `README.md` includes badges, Mermaid `graph TD` (Core Managers/Theme Engine/Global Art Direction → Word Search module + its art/spec), “Decisiones Clave de Arquitectura”, and Quick Start with `dotnet test`.
- [ ] `docs/art_direction.md` is global-only: standards, universal components, immutable palette matrix, visual inheritance, Master Index linking minigame art directions.
- [ ] `docs/word_search/art_direction.md` specifies static $8\times8$/$30\times30$px/$6$px baseline, $+/-$ zoom, ~70px WordsPanel/footer min, confetti via approved palette tokens; references global tokens without redefining them.
- [ ] Links resolve; no gameplay/code files modified.