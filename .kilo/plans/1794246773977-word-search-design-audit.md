# Art Direction Documentation Restructure Plan (plan only, no implementation)

## Goal
Establish a clean hierarchy: `docs/art_direction.md` as **global master/index + immutable palette & inheritance authority**, and `docs/word_search/art_direction.md` as **Word Search-specific layout/behavior spec** that references (does not redefine) global tokens.

## Current state (verified)
- `docs/art_direction.md` exists (v0.5): mixed global rules (pixel-art, safe area, ThemeManager `ThemeData`, component library, game juice) and Word Search-specific typography mapping (`§4` cites `docs/word_search/spec.md`), no Master Index, no explicit visual-inheritance rule, no single immutable “global color palette matrix” section.
- `docs/word_search/art_direction.md` does **not** exist. Word Search visual details are scattered across `docs/word_search/spec.md`, `docs/word_search/plan.md`, `minigames/word_search/WordSearchVisuals.cs`, and code.
- Reference: `docs/word_search/wathersoup.jpg`.

## Target structure & ownership
| Document | Owns | Must not own |
|---|---|---|
| `docs/art_direction.md` (Global) | Global UI/UX standards, universal base components (main menu, settings, modals, HUD patterns where global), typography rules/tokens, **immutable global color palette matrix**, Master Index, inheritance rule | Minigame-specific grid metrics, per-minigame layout/particles except as linked examples |
| `docs/word_search/art_direction.md` (Minigame) | Word Search layout metrics, static grid rules, zoom controls, footer/WordsPanel dimensions, particle/confetti behavior, Word Search-specific components/overrides allowed by inheritance | Redefinition of global palette/typography/button-relief/font tokens; global component specs |

## Decisions (apply unless revised)
1. **Global palette matrix = immutable source of truth**: define once in `docs/art_direction.md` (table: token/name, hex, usage/role, source e.g. `ThemeData`/`PixelStyles`). `docs/word_search/art_direction.md` references tokens by name only.
2. **Visual inheritance rule (global)**: all minigames inherit base colors, button relief styles, and font tokens from global authority; minigames may specify layout/metrics/behavior and may only extend via documented, scoped overrides that do not redefine global token values.
3. **Typography split**: global defines font roles/tokens and fallback/accessibility rules (incl. Atkinson for accessible UI). Word Search doc maps those roles to its specific elements (grid, HUD, title, footer) without redefining global rules.
4. **Conflicts with existing text**: where current docs/code mention radius-0 “pixel art puro” vs requested rounded/relief in practice, state the authoritative global rule explicitly and note any scoped exception policy in the relevant minigame doc only if approved (flag as verification item).

## Implementation tasks (ordered, for an implementation-capable agent)

### 1) Refactor `docs/art_direction.md` into Global Master + Index
- **Retitle/version** as Global Art Direction (keep SDD tone); add “Last updated” metadata if consistent with repo style.
- **§ Global standards (keep/refine)**: pixel-art rendering rules, safe area/notch, texture/filter rules, accessibility baseline relevant globally.
- **§ Immutable Global Color Palette Matrix (new, authoritative)**:
  - Consolidate canonical tokens from `core/ThemeData.cs`, `core/ui/PixelStyles.cs`, and current doc §2.1 into one matrix (include at minimum: background/surface, header/subheader, cell idle/found text & bg/border, footer text/strikethrough, accent/selection, text dark/light, borders/shadows as applicable). Mark as **immutable**; note `ThemeManager`/themes may map variants but must not contradict base token meanings without a documented theme contract.
  - Resolve/annotate any drift (e.g. doc vs code hex differences) by choosing the authoritative source during implementation and recording it.
- **§ Universal base components**: retain/normalize component library entries that are truly global (buttons, banners/plates, modals, HUD/header/footer primitives, settings/menu patterns) and describe expected relief/press behavior in terms of global tokens; move Word Search-only specifics to minigame doc.
- **§ Typography rules (global)**: define role-based tokens (Title/Display, HUD/Mono, Grid/Word content, Accessible UI), licensing/Nearest-filter expectations; remove Word Search-only phrasing or convert to an example/link.
- **§ Visual inheritance rule (new, explicit)**: state inheritance of base colors, button relief styles, font tokens; define what minigames may override (metrics, layout, behavior) vs may not (global token values/palette redefinitions).
- **§ Master Index — Minigame Art Directions (new)**: table/list linking to `docs/word_search/art_direction.md` (and placeholder convention for future minigames, e.g. `docs/<minigame>/art_direction.md`). Brief one-line scope per entry.
- **§ Micro-animations / game juice**: keep global patterns; move Word Search-specific confetti/particle behavior to minigame doc (leave a pointer if helpful).

### 2) Create `docs/word_search/art_direction.md` (minigame-specific)
- **Header & scope**: Word Search art direction, status/version, reference to `docs/art_direction.md` and `wathersoup.jpg`; declare inheritance of global palette, button relief, and font tokens.
- **Layout & metrics (specific)**:
  - Overall scene regions (header/banner, nav/HUD, board area, footer/WordsPanel, modals) and safe-area usage per global rules.
  - **Grid rules**: document as requested — “static grid rules ($8 \times 8$ visible letters, $30 \times 30$px cells, $6$px gaps)” as the reference/static baseline, and include difficulty variants (10×10/12×12/15×15) with the project’s target/fit behavior **clearly labeled** (target vs resolved/fitted) to avoid contradicting runtime reality; reference `resources/difficulties/*` and layout logic.
  - **Zoom controls**: `$+$ / $-$` controls and any pinch/center behavior as implemented/planned (verify against `WordSearchMain`/container); state accessibility (buttons as non-gesture alternative).
  - **Footer / WordsPanel**: dimensions and anchoring — **~70px minimum height** target (effective), internal margins, `WordList` behavior, visibility/anti-clipping within 360×640 and safe insets; note current vs target if they differ.
- **Typography mapping (Word Search)**: which global font tokens apply to title, difficulty/subtitle, timer/counters, nav buttons, grid letters (incl. weight/legibility intent), and word list/found-state styling — **no redefinition of global tokens**.
- **Color/states**: reference global palette matrix tokens for idle/selected/found/hint/trace/footer-strike states; no hex redefinitions except citing token names.
- **Particles/confetti behavior**: document victory/confetti (`PixelConfettiEmitter`/`CpuParticles2D`/equivalent behavior per spec & implementation): trigger conditions, timing (e.g. post-completion delay before modal as per spec), particle style (pixel squares, size/colors tied to palette tokens), performance constraints.
- **Cross-references**: link to `docs/word_search/spec.md`, `plan.md`, and relevant code paths for maintainability.

### 3) Consistency & verification pass (during implementation)
- Ensure relative links work (`docs/art_direction.md` ↔ `docs/word_search/art_direction.md`).
- Grep/update stale references if other docs point to moved/renamed content (low-risk scan).
- Validate all stated metrics/behaviors (30×30, 6px, zoom ±, confetti timing/shape) against `wathersoup.jpg`, current implementation, and spec; mark any unverifiable-in-env detail for editor/device confirmation in the doc (short “Verification” note).

## Affected files (planned)
- Edit: `docs/art_direction.md`
- Create: `docs/word_search/art_direction.md`
- Read/reference only: `docs/word_search/spec.md`, `docs/word_search/plan.md`, `core/ThemeData.cs`, `core/ui/PixelStyles.cs`, `core/ThemeManager.cs`, `minigames/word_search/WordSearchVisuals.cs`, `minigames/word_search/WordSearchMain.tscn|.cs`

## Risks / notes
- **Metric authority conflict**: request states static 8×8 / 30×30px / 6px; current code uses fit-to-area with 6px gap and 24px min (no 30px target). Doc must be explicit (reference baseline vs runtime) to avoid false precision.
- **Palette drift**: `ThemeData`, `PixelStyles`, and existing art doc differ in some values/names — implementation must pick one canonical matrix and cite source.
- **Plan mode constraint**: this session is planning-only; documentation edits above must be executed by an implementation-capable agent after plan approval.

## Validation (after implementation)
- [ ] `docs/art_direction.md` contains only global standards/components/typography rules + immutable palette matrix + inheritance rule + Master Index linking minigame docs.
- [ ] `docs/word_search/art_direction.md` exists, covers grid/zoom/footer/confetti specifics, and references global palette/tokens without redefining them.
- [ ] All internal doc links resolve; terminology consistent (“WordPanel/footer”, token names).
- [ ] Metrics/behaviors flagged or confirmed against `wathersoup.jpg`/implementation.