# Word Search V1 — Architecture & Aesthetic Audit Plan

**Version:** v0.4.5  
**Status:** Phase 2 complete (nav bar polish)  
**Protocol:** Execute exactly one phase at a time; mark `[x]` and bump patch version after each phase; STOP and wait for confirmation before starting the next phase.

## SemVer Changelog

| Version | Date | Phase | File | Change |
|---------|------|-------|------|--------|
| v0.4.0 | 2026-11-13 | — | — | Initial 9-phase audit micro-plan |
| v0.4.1 | 2026-11-13 | 1 | `core/ui/PixelStyles.cs` | Canonical palette alignment (`#EFEFEE`, `#A0BFC7`, `#477987`, `#101010`); AA=false, CornerRadius=0, Alpha=1.0 |
| v0.4.2 | 2026-11-15 | 2 | `minigames/word_search/WordSearchMain.tscn`, `core/ui/PixelStyles.cs`, `core/BaseMinigame.cs` | Background `#EFEFEE` (`Placa`), TitlePanel `#A0BFC7` + `#101010` (sin contorno), Difficulty banner `#477987` + white text (`VCR OSD Mono` 14 px); removed runtime overrides that overwrote the scene |
| v0.4.3 | 2026-11-15 | 2 (fix) | `minigames/word_search/WordSearchMain.cs`, `minigames/word_search/WordSearchMain.tscn`, `core/ui/PixelStyles.cs` | Word counter sub-label forced to `TextDark` `#101010`; difficulty label uses `TextLight`; `SubtitlePanel` tokenized (`Tinta`/`Borde`); dead `#6C929D` header tokens removed; `AntiAliasing = false` + `Alpha = 1.0` enforced |
| v0.4.4 | 2026-11-15 | 2 (warm banner) | `core/ui/PixelStyles.cs`, `core/ui/PixelButtonStyle.cs`, `minigames/word_search/WordSearchMain.cs`, `minigames/word_search/WordSearchMain.tscn` | Additive token `CreamYellow #EDE19E`; difficulty banner now warm cream + `Borde` `#477987` with `TextDark` `#101010` (14.4:1); new `%TimeBox` `PanelContainer` (`Placa` + 2 px border, padding 12/4) wrapping timer + counter; `Plate()` gets content margins; `ModalBackdrop()` uses `TextDark` (no ad-hoc scrim); button `Focus` state no longer alpha-0 |
| v0.4.5 | 2026-11-15 | 2 (nav bar polish) | `minigames/word_search/WordSearchMain.tscn`, `minigames/word_search/WordSearchMain.cs`, `tests/integration/WordSearchZoomConfettiTest.cs`, `docs/word_search/art_direction.md` | `%BtnBack` now renders `← Atrás` (icon + text, 96×56, Hud variant padding); `%BtnZoomOut`/`%BtnZoomIn` live inline in `%NavRow` beside `%BtnPause` (the floating `%ZoomOverlay` was dropped because it covered top-right grid cells on 12×12/15×15 boards); `NavBalance` spacer reduced 36 → 0 px; `SafeAreaManager` pins 8 px side margins (344 px usable width at 360 px); HUD button outline stays `#101010` — `#477987` on `#A0BFC7` is 2.47:1 and fails the 3:1 guard (`PixelStyles.BorderColor`), documented in art_direction §2.1 |

## Phases

- [x] **Phase 1** — `core/ui/PixelStyles.cs`: Canonical palette (`#EFEFEE`, `#099C92`, `#A0BFC7`, `#477987`, `#101010`), `AntiAliasing = false`, `CornerRadius = 0`, `Alpha = 1.0`.
- [x] **Phase 2** — `minigames/word_search/WordSearchMain.tscn`: Background `#EFEFEE`, TitlePanel `#A0BFC7` + `#101010`, Difficulty banner `#477987` + white text.
- [ ] **Phase 3** — `minigames/word_search/WordCell.cs`: Replace `Colors.White` with canonical light token, `Alpha = 1.0`.
- [ ] **Phase 4** — `core/ui/CompletionModal.cs`: Replace defeat color with `#B45252` (`CelebrationRed`) and simplify signature (KISS).
- [ ] **Phase 5** — `core/ui/PixelButton.cs`: Remove parallel arrays (`_styles[5]`, `_textColors[5]`) in favor of clean state mapping (YAGNI/KISS).
- [ ] **Phase 6** — `minigames/word_search/WordGridContainer.cs`: Delegate `_GuiInput` 100% to `BoardInputHandler` (DRY/SRP).
- [ ] **Phase 7** — `minigames/word_search/WordSearchGenerator.cs`: Reuse `WordSearchTheme.NormalizeWord()` instead of duplicated `PrepareWords()` (DRY).
- [ ] **Phase 8** — `core/BaseMinigame.cs`: Add dual-mode clock (count-up Chronometer + countdown Timer with timeout signal).
- [ ] **Phase 9** — `minigames/word_search/WordSearchMain.cs`: Use `BaseMinigame` dual clock and run final `dotnet build` check.

## Strict Protocol

1. Work on exactly one phase at a time.
2. After completing a phase, mark it `[x]` in this file, bump the patch version (e.g. v0.4.1 → v0.4.2), and stop.
3. Do not start the next phase until the user confirms.
