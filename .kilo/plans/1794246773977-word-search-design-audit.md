# Word Search (`minigames/word_search/`) — Retro Design Audit & Alignment Plan (audit only, no code changes)

## Scope / inputs reviewed
- Scene/layout: `minigames/word_search/WordSearchMain.tscn`, `WordSearchMain.cs`
- Grid/geometry: `WordGridFactory.cs`, `WordGridContainer.cs`, `WordSearchVisuals.cs`
- Cells: `WordCell.cs`, `WordCellFactory.cs`, `scenes/ui/components/PixelCell.cs`
- HUD/footer: `WordSearchHud.cs` (note: checklist mentions `WordsPanel.cs`; **no such file exists** — target `%WordPanel`/`%WordList` + HUD)
- Difficulty data: `DifficultyConfig.cs`, `DifficultyCatalog.cs`, `resources/difficulties/{facil,medio,dificil,maestro}.tres` (8×8/5, 10×10/8, 12×12/12, 15×15/20 — data is correct)
- Styling/theme: `core/ui/PixelStyles.cs`, `core/ThemeData.cs`, `core/ThemeManager.cs`, `core/AccessibleTheme.tres`, `ui/components/BigButton.cs|.tscn`, `core/SafeAreaManager.cs`
- Fonts: `assets/fonts/PixelOperator.ttf`, `PressStart2P.ttf`, `VCR_OSD_Mono.ttf`, `AtkinsonHyperlegible-*.ttf` (no bold companions for the pixel fonts)
- Docs: `docs/word_search/spec.md`, `docs/word_search/plan.md`, `docs/art_direction.md`
- Reference asset present (not visually rendered in this environment): `docs/word_search/wathersoup.jpg`. **Pixel-precise 1:1 values (corner radius, relief recipe, exact chrome heights, gaps) must be measured/verified against this image in the Godot editor/device before and after implementation.**

## Executive summary
Implementation is functionally close (Fases 2–3), but visual output diverges from the retro reference due to: (1) **split styling authorities** — `AccessibleTheme.tres` gives `BigButton`s 5px corners, `PixelStyles`/`PixelCell` hardcode 0px, and `WordSearchMain.ApplyTheme` only overrides panels; (2) **thin typography** — `PixelOperator`/VCR/PressStart are single-weight with no bold/embolden treatment (`outline_size=0`); (3) **footer starvation risk** — `WordPanel` min 48px + `RichTextLabel fit_content` + fixed chrome can clip/push the footer on 360×640 (needs ~70px effective min + allocation guarantee); (4) **uniform fit-to-available grid** — `WordGridFactory/Container` use fixed `gap=6`, `MinCellSize=24`, no per-difficulty target metrics, so requested 36/30/24/20 are not enforced and Maestro 20px is currently blocked.

## Decisions to confirm before implementation (recommended defaults)
| # | Decision | Recommendation | Why / impact |
|---|---|---|---|
| D-A | Corner/relief policy vs “pixel art puro / radius 0” | **3px corners + `AntiAliasing=false` + hard 2px offset shadow/bevel** for Word Search buttons/cells/panels (local to minigame/style contract); document as intentional compromise in `art_direction.md`/spec | Matches “subtle 2–4px + 3D relief”; avoids blurry AA. Update/remove strict radius-0 assertions where needed. |
| D-B | Grid sizing policy (36/30/24/20 @ 360×640 infeasible with current chrome) | **Target-then-fit with per-difficulty metrics**: set targets {8:36,10:30,12:24,15:20} + gaps from reference; compute ideal, fit proportionally inside available `BoardArea` with **zero clipping**; allow Maestro min 20px (override global 24) only for 15×15; record resolved size in debug/tests. Pair with layout-budget reductions (D-C). **Do not silently invent sizes.** | Exact targets don’t all fit 328px-wide board after chrome/footer (Medio/Maestro overflow by width/height math). Keeps 1:1 intent while guaranteeing no clip. |
| D-C | Footer minimum | **`WordPanel` effective min ~70px** (scene/code), `BoardArea` remains flex but panel cannot shrink below min; verify 20-word wrap (Maestro) and adjust font/padding or allow bounded behavior only if reference requires | Current 48px is below requirement and clips under pressure. |
| D-D | Bold letters | **True bold/thick pixel font variant for grid letters first** (asset/license decision). Fallback: controlled faux-bold (1px outline matching font color or 1px shadow) only if it passes pixel-crisp visual check; avoid dirty halos. Apply weight treatment consistently to key HUD labels per reference | No bold variants exist in `assets/fonts/`. Faux-bold is risky on bitmap fonts. |

> I can proceed to a final implementation-ready task plan once D-B is confirmed (target-then-fit recommended). D-A/C/D can use the recommendations unless you prefer different values.

---

## 1) Geometry & Rounded Corners (Buttons & Cells) — audit

### Observed behavior
- **Nav/action buttons (`BtnBack`, `BtnPause`, modal buttons) are `BigButton` (Button) using `core/AccessibleTheme.tres`** (`WordSearchMain.tscn:95,154,249,258,267,276,340,349`; `BigButton.tscn:3,11`). `BigButton.cs` only enforces min touch size/groups; it does **not** apply pixel `StyleBox` overrides.
- **`core/AccessibleTheme.tres` defines Button `StyleBoxFlat` with `corner_radius_* = 5`** for normal/focus/hover/pressed/disabled (lines ~5–107). This produces visibly rounded buttons, not “subtle rounded corners (~2–4px) with continuous 3D relief” in a controlled retro style (radius is 5px and styling is theme-driven, not per the word-search pixel language).
- **Game letter tiles are `WordCell : PixelCell`** (`WordCellFactory.Create` → `new WordCell().Setup`). `PixelCell.BuildStyleBoxes/MakeStyleBox` sets `AntiAliasing=false`, `CornerDetail=1`, **`SetCornerRadiusAll(0)`** and uses 2px borders (`PixelCell.cs:102–122`). `WordCell` inherits and overrides label visuals but does not add corner radius. Result: **sharp 0px square corners**, consistent with pixel-art-pure rule in docs, but deviating from the requested “authentic subtle rounded corners (~2–4px) with continuous 3D relief”.
- **Panels**: `PixelStyles` also uses `Radius=0` for `WordPanel/TitlePanel/SubtitlePanel/ModalCard` and most boxes (`PixelStyles.cs:13,100–161`), while `Button(fill)` helper exists but is not applied to Word Search `BigButton` instances (`WordSearchMain.ApplyTheme` overrides only panels). This split explains inconsistent geometry.

### Root-cause deviation
1. **Style authority split**: scene/theme (`AccessibleTheme.tres`) controls buttons; `PixelStyles` controls panels; `PixelCell` hardcodes cell boxes. No single Word Search visual pass enforces 2–4px + 3D relief.
2. **`PixelCell` bakes `corner_radius=0` and border widths** independent of `PixelStyles.Cell*()` (PixelStyles has `CellNormal/Selected/Found/Hint` with `Radius=0`; `PixelCell` duplicates similar boxes). Changing one without the other creates drift.
3. **3D relief is inconsistent**: `PixelStyles.MakeBox` adds hard shadow (`ShadowSize=2, offset (2,2)`, optional) for some elements, but cells/buttons in current runtime paths do not uniformly use that relief; `AccessibleTheme` button shadows/radii differ.

### Proposed engineering steps (implementation-ready)
1. **Geometry contract** (recommend `minigames/word_search/`-scoped helper, e.g. `WordSearchRetroStyles` or extend `WordSearchVisuals` + factory; avoid breaking global `PixelStyles` unless intentionally migrated):
   - `CornerRadius=3` (tune 2–4 vs `wathersoup.jpg`), `AntiAliasing=false`, `CornerDetail` minimal, `BorderWidth` per element, state colors from existing theme tokens.
   - 3D relief recipe (pick to match ref): e.g. `StyleBoxFlat.ShadowSize=2, ShadowOffset=(2,2)` opaque dark + optional top/left highlight border treatment; pressed state reduces/inverts relief.
2. **Cells (`PixelCell.cs` + `WordCell.cs`)**:
   - Replace/parameterize hardcoded `MakeStyleBox(...,0 radius)` in `PixelCell.BuildStyleBoxes` (or supply style provider). Idle/selected/found/hint get radius+relief; keep `MouseFilter.Ignore`, `ClipContents` behavior, theme updates (`ApplyTheme`/`OnThemeChanged`).
   - `WordCell` keeps state/`FoundBy`/label semantics; ensure label padding doesn’t clip with rounded corners.
3. **Buttons** (`WordSearchMain.tscn` + `WordSearchMain.ApplyTheme` or theme helper):
   - Override `normal/hover/pressed/disabled/focus` for `%BtnBack,%BtnPause,%BtnResume,%BtnHint,%BtnReset,%BtnPauseMenu,%BtnNextGame,%BtnWinMenu`. Keep `BigButton` min 56×56 and focus ring shape-based.
   - Consider a small local `Theme` resource for this scene to reduce per-node code, if clean.
4. **Header plates**: add runtime overrides for `%TitlePanel` and `%SubtitlePanel` (currently none) to align radius/relief with reference.
5. **Docs/tests impact**: if radius becomes 3px, reconcile `PixelStyles.Radius` comment/tests and art/spec text that mandate radius 0 (scope-limited to Word Search vs global). Add/adjust style-contract tests only if the project already tests these visuals.

### Risks / decisions needed
- “Subtle rounded corners” conflicts with current “pixel art puro / corner_radius=0” statements in `PixelStyles`/`PixelCell`/`art_direction.md`/`spec.md` for strict square pixels; implementation must explicitly document the intended compromise (small radius + `AntiAliasing=false` + pixel borders) and update docs/tests if policy changes.

---

## 2) Typography & Font Weight (Bold Letters) — audit

### Observed behavior
- **Grid letter labels**: `PixelCell.CreateLabel` creates a plain `Label` centered, no bold/outline (`PixelCell.cs:166–177`); `WordCell.Setup` sets text + `font_size` + assigns `Constants.FontCellPath` font (`PixelOperator.ttf` per spec/art; `WordCell.cs:54–60`) but never sets a bold variation, theme font weight, or faux-bold. `WordCell.UpdateVisuals` sets `font_color`, clears outline (`PixelCell`/`WordCell` both set `outline_size=0`). Result: **thin/fine PixelOperator rendering**, especially at small cell sizes.
- **UI labels/buttons**: scene uses `PressStart2P` (title, 22px), `VCR_OSD_Mono` (subtitle 11px, nav labels 15–18px, buttons 16–18px), `PixelOperator` (footer `RichTextLabel`, 18px) — `WordSearchMain.tscn:73,88,139,151,171,204,246,...`. None configured as bold; VCR/PixelOperator/PressStart2P families in `assets/fonts/` appear as single TTFs (no bold companion listed except Atkinson variants exist globally).
- **Dynamic cell font size**: `WordGridContainer.UpdateLayout` computes `fontSize = clamp(round(CellSize*0.52), 10, 24)` (`WordGridContainer.cs:356–360`, `WordSearchVisuals.Min/MaxCellFontSize=10/24`). Smaller/denser grids can drop toward minimum, amplifying thinness.

### Root-cause deviation
1. **No bold font asset/variation** for the retro fonts used in grid/UI (PixelOperator/PressStart2P/VCR OSD Mono are typically single-weight bitmap/pixel fonts).
2. **No faux-bold fallback** (e.g., `Label` outline-as-weight trick, shadow-offset double draw, or `FontVariation` embolden where supported) and `outline_size` is explicitly forced to `0`.
3. **Font size scaling trades legibility for fit** without a weight/contrast compensation, and Maestro/denser layouts approach min font.

### Proposed engineering steps (implementation-ready)
1. **Asset/approach decision (D-D)**: confirm bold source. Preferred: add/assign a thick pixel font (license-safe) and set `Constants.FontCellPath`/`WordCell` font accordingly; update `assets/fonts/README`/manifest/import as needed. Fallback: `FontVariation` embolden if Godot/.NET path supports clean output, else 1px `outline_size` same as `font_color` (test for halos) — keep `AntiAliasing/Nearest`.
2. **Grid letters** (`WordCell.cs`/`PixelCell.cs`):
   - Apply bold font/variation in `Setup` and any font-size update path. Keep `outline_size` policy explicit (0 if true bold; controlled if faux-bold chosen).
   - Revisit `CellFontRatio` and per-difficulty min font (section 4) so 20px-class cells remain readable.
3. **Footer/list** (`WordSearchMain.tscn` `%WordList`, `WordSearchHud.cs`): if reference shows heavier word labels, apply bold font or BBCode `[b]` where supported by the active font; preserve `~~/color` for found words and flow layout.
4. **HUD/buttons/title**: apply consistent weight for `TimeLabel`, `WordsCounter`, difficulty, `Btn*` text, modal titles/scores per reference; scene currently mixes PressStart/VCR/PixelOperator — change only where visually required.
5. **Verification**: normal/selected/found/hint grid states; 360×640 and Maestro 15×15; no clipping in cells after radius/padding changes.

### Risks
- Faux-bold can look blurry or dirty on pixel fonts; true bold bitmap font is safest for retro fidelity. License/attribution for any new font must follow project rules.

---

## 3) Footer & `WordsPanel` Visibility (`WordsPanel.cs` / `WordSearchMain.tscn`) — audit

> Note: checklist names `WordsPanel.cs`; actual implementation is the scene node `%WordPanel` + `%WordList` (`WordSearchMain.tscn:181–206`) driven by `WordSearchHud` (`WordSearchHud.cs`) and styled in `WordSearchMain.ApplyTheme`/`PixelStyles.WordPanel()`. No `WordsPanel.cs` file found — engineering work should target these real components (and only introduce a `WordsPanel` class if refactoring for SRP/clarity).

### Observed behavior
- **Layout**: root `VBoxContainer` (sep 6) contains Header, `NavRow` (56), `BoardArea` (`size_flags_vertical=3`, expands), `WordPanel` (`size_flags_vertical=1`, `custom_minimum_size (0,48)`), hidden `VictoryLabel` (`.tscn:51–214`). `SafeAreaManager` enforces `margin_top = max(config.MinTop=32, display inset)` and sets scene’s `MarginContainer` margin_top to 48 baseline (`.tscn:47`, `SafeAreaManager.cs:69–92`). Bottom margin 16.
- **Word panel internals**: `WordPanel` (PanelContainer, min height 48) → `WordMargin` (margins 4/2/4/2) → `WordList` `RichTextLabel` (`custom_minimum_size (0,28)`, `bbcode_enabled`, **`fit_content=true`**, `scroll_active=false`, font PixelOperator 18, line sep 2). `PixelStyles.WordPanel()` adds content margins (10/10/8/8) and 3px border. Effective minimum visual/content height can exceed 48 once style+inner margins apply.
- **Approx vertical budget at 360×640** (from layout constants): inner height after top/bottom margins 576; header 90 + nav 56 + separators 18 + word panel min 48 = 212 fixed/min before board flex behavior details ⇒ **~364px** nominally available to `BoardArea` if panel stays at minimum. With larger safe-area top inset, more words/wrapping, or style content margins, `BoardArea`/panel compete; on small heights the bottom panel can be pushed/clipped.
- **Content growth**: `fit_content=true` lets `RichTextLabel` request height for wrapped BBCode (`" | "` flow, found words with `[s][color]`), but container min is only 48 and scroll disabled — overflow/clipping risk when 8/12/20 words wrap (plan requires legibility for 5/8/12/20). `WordSearchHud.BuildWordList` renders all pending+found in one line-flow string.

### Root-cause deviation
1. **Insufficient fixed minimum / no guaranteed footer band**: `WordPanel` min 48px (and inner label min 28px) is below the requested **~70px fixed minimum** and below what wrapped retro word chips/list may need.
2. **Competing constraints**: `BoardArea` expands (`size_flags_vertical=3`) while footer is effectively shrink-prone (`size_flags_vertical=1` with small min + fit-content child). Without an explicit min/footer budget, Godot allocation under tight height can clip the bottom.
3. **Safe area / scene margins**: top margin fixed at 48 in scene plus safe-area logic can consume vertical space on notched devices; bottom inset handling is present but footer still needs a hard minimum inside safe area.
4. **Scroll disabled + fit content**: safest for “no scroll” small lists but becomes a clipping trap when content exceeds available height; needs either adequate min height, bounded wrapping strategy, or an intentional max/scroll behavior aligned to reference.

### Proposed engineering steps (implementation-ready)
1. **Scene layout** (`WordSearchMain.tscn`): set `WordPanel.custom_minimum_size.y` to **~70** (adjust to include `PixelStyles.WordPanel` content margins 8/8 + border + `WordMargin` 2/2 + `WordList` line/font). Keep/confirm `WordPanel` is not shrinkable below min in the VBox; `BoardArea` keeps `size_flags_vertical=3`.
2. **WordList/RichTextLabel**: keep `fit_content`/`scroll_active=false` only if 70px+padding accommodates 20 wrapped entries in the chosen font; otherwise set a deliberate policy (slightly larger min, reduced footer font size/line separation, or reference-matched wrapping). Do not allow invisible overflow.
3. **Layout budget recovery** (coordinate with section 4): measure `wathersoup.jpg` chrome; candidates: tighten `TitlePanel`/`SubtitlePanel` heights/padding, `NavRow` height, `VBox` separation, or `MarginContainer` top (while respecting `SafeAreaManager` minimums/display insets). Validate against 360×640 + notch insets.
4. **Safe area**: confirm `SafeAreaManager` bottom inset + scene bottom margin keep `WordPanel` fully visible; no change expected to `ReadDisplayInsets`, but verify on device.
5. **Optional SRP**: extract `WordsPanel` controller only if it simplifies invariants (min height, font scaling, theme). Not required.
6. **Acceptance**: all difficulties (5/8/12/20 words), 360×640, `fit_content` wrap, bottom safe inset present — footer fully visible, grid not overlapped.

### Risks
- Increasing footer min reduces board height; combined with area 4, Maestro 15×15 target 20px cells becomes less feasible — must be handled holistically (see grid sizing).

---

## 4) Grid Sizing & Difficulty Proportions (`WordGridFactory`, `WordGridContainer`) — audit

### Observed behavior
- **Data/config is correct for grid dimensions**: `facil.tres` 8×8/5, `medio` 10×10/8, `dificil` 12×12/12, `maestro` 15×15/20, with expected directions/categories (`resources/difficulties/*.tres`). Catalog/config classes match.
- **`WordGridFactory.ComputeLayout`** (`WordGridFactory.cs:46–64`): uses available `BoardArea.Size`, fixed `gap = WordSearchVisuals.CellGap` (**6px**), computes `stepX/stepY = (available - gap*(n-1))/n`, picks `cellSize = max(MinCellSize=24, floor(min(stepX,stepY)))`, centers via `origin`. **No per-difficulty target cell size** (36/30/24/20) and no max/preferred target — it always fits available space uniformly.
- **`WordGridContainer`** (`.cs:347–362`): on resize/setup, applies layout, sets `CellSize`, computes font as `clamp(round(CellSize*0.52),10,24)`. Hit-testing uses the same `CellSize`, `gap`, `_origin` (`TryGetCell/GetCellRect/GetCellCenter`), so **hit-testing is aligned to rendered layout** today. Changing gap/cell formula must update *all* these paths together.
- **`WordSearchVisuals`**: `CellGap=6`, `MinCellSize=24`, `BoardMargin=19`/`MarginScalePerLevel` defined but **not used** by grid factory/container (potentially dead/intent mismatch with art direction/plan).

### Feasibility check (requested targets vs 360×640, current layout budget)
Using scene constants: width 360 − 16 − 16 = **328px**; estimated board height ≈ 640 − 48(top) − 16(bottom) − 90(header) − 56(nav) − 18(seps) − 70(footer target) = **342px** (safe-area top can increase). Cell pitch needed = `n*cell + (n-1)*gap`:

| Difficulty | n | Target cell | gap=6 pitch (w×h needed) | Fits 328×342? | Current uniform-fit (approx, before 70px footer) would choose ~`floor(min)` based on actual `BoardArea` |
|---|---:|---:|---|:---:|---|
| Fácil | 8 | 36 | 330×330 | **W tight / borderline** (330 vs 328) | will fit smaller/larger by available area, not fixed 36 |
| Medio | 10 | 30 | 354×354 | **No** | fits to area |
| Difícil | 12 | 24 | 330×330 | **borderline** | fits to area; min cell 24 matches floor |
| Maestro | 15 | 20 | 384×384 | **No** (also `MinCellSize=24` currently blocks 20) | currently cannot reach 20px; hits 24px floor then may clip if math/constraints misbehave — must be audited in editor |

**Conclusion**: Exact requested targets (36/30/24/20) cannot all be honored simultaneously on 360×640 with current chrome/gap without layout changes. A 1:1 visual match requires either (1) per-difficulty chrome/gap adjustments, (2) treating targets as *preferred/max* with deterministic fit + documented actual sizes, (3) scroll/zoom reliance (undesirable; plan states full board at 100%), or (4) revised targets approved against `wathersoup.jpg`. **Do not silently pick arbitrary sizes during implementation without confirming intent.**

### Root-cause deviation
1. **Fit-to-available algorithm** ignores design targets per difficulty.
2. **Global `MinCellSize=24` conflicts with Maestro target 20px**; also affects legibility/touch — any reduction must be validated (plan notes 15×15 cells shouldn’t drop below 24px visible without zoom; that conflicts with a 20px target and needs resolution).
3. **`CellGap` fixed at 6px** regardless of difficulty/target; exact spacing requires making gap part of the per-difficulty metric or deriving it.
4. **Unused `BoardMargin` constants** suggest incomplete art-direction integration.
5. **Board area depends on header/nav/footer heights** — geometry fixes must coordinate with section 3.

### Proposed engineering steps (implementation-ready, recommended D-B)
1. **Metrics contract** (data-driven): define per-difficulty visual metrics keyed by `DifficultyConfig.Id` (or grid size): `TargetCellPx`, `MinCellPx`, `MaxCellPx`, `GapPx`, `BoardPaddingPx?`, `FontMin/Ratio?`. Seed values to measure against reference: Fácil 8→(36,?), gap ?; Medio 10→(30,?); Difícil 12→(24,?); Maestro 15→(20,20) — **gap may differ from 6 to hit 1:1; confirm from `wathersoup.jpg`**.
2. **Layout struct**: extend `WordGridLayout` (or introduce `ResolvedWordGridLayout`) to carry `CellSize`, `Gap`, `Origin`, `Rows/Cols` as needed. `WordGridFactory.ComputeLayout(rows, cols, available, metrics)`:
   - ideal = `targetCell*n + gap*(n-1)`; if fits, use target (within max).
   - else scale fit: `cell = floor(max(minCell, fitCell))` with gap policy explicit (keep gap fixed and fit cell, or scale gap+cell proportionally — choose one and test hit areas). Guarantee `cell*n + gap*(n-1) <= available` on both axes; center with `origin`.
   - return zero/guard only for invalid input.
3. **`WordGridContainer` alignment**: store resolved `Gap` (and `CellSize`); replace every `WordSearchVisuals.CellGap` usage in `TryGetCell/GetCellRect/GetCellCenter/ShowPreview` step math with resolved gap. Update font sizing using resolved cell; consider per-difficulty font min for Maestro.
4. **Constants cleanup**: deprecate/repurpose global `MinCellSize=24` if Maestro allows 20; keep a safe global floor only where appropriate. Wire `BoardMargin`/padding if reference uses it.
5. **Coordinate with section 3**: after footer min 70px, re-verify `BoardArea` gives enough space; if Medio/Maestro cannot reasonably approach targets, reclaim chrome (nav/header/separators/padding) per reference — but never clip.
6. **Regression safety**: selection path uses `TryGetCell`/centers — update together; `WordTraceOverlay` uses centers from container (OK if container methods updated). Zoom/pan interactions must still use resolved geometry.

**Feasibility note (360×640, est. board 328×342 after 70px footer, gap=6):** Fácil 330×330 borderline width; Medio 354×354 no; Difícil 330×330 borderline; Maestro 384×384 no and below old 24px floor. Implementation must use D-B (target-then-fit + layout budget + reference gap) rather than forcing impossible fixed sizes.

### Risks / decisions needed
- **Maestro 20px vs MinCellSize 24px vs plan note** (“cells not below 24px visible in 15×15 without zoom”) is a direct conflict with the requested target; requires product/design decision and doc update.
- Any gap change affects `WordTraceOverlay`/selection visuals and perceived density — verify against reference.

---

## Cross-cutting implementation notes (for when coding is approved)
- Keep changes localized to `minigames/word_search/` and shared style/theme primitives only where necessary; follow existing SRP/factory patterns (`WordGridFactory`, `WordCellFactory`, visuals constants).
- Preserve accessibility: touch targets ≥56×56 where applicable, focus visibility, non-color-only states, safe contrast. Pixel relief/borders must not reduce legibility.
- Update/add tests after implementation (GdUnit4Net patterns already used): layout/hit-test math for per-difficulty metrics, footer min visibility constraints where feasible, and visual/style snapshot/manual checklist vs `docs/word_search/wathersoup.jpg`.
- Update `docs/word_search/plan.md`/`spec.md`/`art_direction.md` only where decisions (radius policy, Maestro 20px/cell floor, footer height, per-difficulty metrics) are finalized — flag as follow-up in implementation PR/commit.

## Validation checklist (post-implementation, manual/editor)
- [ ] Buttons and cells show ~2–4px subtle rounded corners with continuous 3D relief matching `wathersoup.jpg`.
- [ ] Grid/UI letters are bold/thick and arcade-legible at all difficulties/states.
- [ ] `WordPanel`/footer fully visible at 360×640 for 5/8/12/20 words; min height ~70px effective; no grid/footer clipping.
- [ ] Fácil/Medio/Difícil/Maestro use intended per-difficulty metrics (or approved fitted equivalents), exact spacing, and hit-testing aligned; zero clipping.
- [ ] No regressions in selection, found/hint states, theme application, or safe-area behavior.
