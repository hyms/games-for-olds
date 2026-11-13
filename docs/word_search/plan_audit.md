# Word Search V1 — Architecture & Aesthetic Audit Plan

**Version:** v0.4.2  
**Status:** Phase 2 complete  
**Protocol:** Execute exactly one phase at a time; mark `[x]` and bump patch version after each phase; STOP and wait for confirmation before starting the next phase.

## SemVer Changelog

| Version | Date | Phase | File | Change |
|---------|------|-------|------|--------|
| v0.4.0 | 2026-11-13 | — | — | Initial 9-phase audit micro-plan |
| v0.4.1 | 2026-11-13 | 1 | `core/ui/PixelStyles.cs` | Canonical palette alignment (`#EFEFEE`, `#A0BFC7`, `#477987`, `#101010`); AA=false, CornerRadius=0, Alpha=1.0 |
| v0.4.2 | 2026-11-13 | 2 | `minigames/word_search/WordSearchMain.tscn` | Background `#EFEFEE`, TitlePanel `#A0BFC7` + `#101010`, Difficulty banner `#477987` + white text (`VCR OSD Mono`) |

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
2. After completing a phase, mark it `[x]` in this file, bump the version (e.g. v1.0.1 → v1.0.2), and stop.
3. Do not start the next phase until the user confirms.
