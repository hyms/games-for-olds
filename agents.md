# Agent Guidelines — Clásicos Mente Activa 2D

> Concise instructions for AI agents working in this repository. Follow these before reading or modifying code/docs.

## Purpose & scope
- Keep changes minimal, verifiable, and aligned with the project’s Spec-Driven Development (SDD) workflow.
- **Documentation/governance changes:** prefer updating the authoritative docs below over ad-hoc decisions.
- **Visual fidelity:** validate UI/layout claims against `docs/word_search/wathersoup.jpg` (and future minigame references) in the Godot editor/device when possible.

## Hierarchy of Sources of Truth (SoT)
1. **Global product/spec:** [`docs/spec.md`](docs/spec.md) — vision, architecture, transversal requirements.
2. **Global art direction:** [`docs/art_direction.md`](docs/art_direction.md) — global UI/UX standards, universal base components, **immutable global color palette matrix**, typography tokens, and **visual inheritance** (Master Index of minigame art directions).
3. **Minigame-specific (per game):** `docs/<minigame>/spec.md` and `docs/<minigame>/art_direction.md` (e.g. [`docs/word_search/spec.md`](docs/word_search/spec.md), [`docs/word_search/art_direction.md`](docs/word_search/art_direction.md)) — rules, layout, metrics, and behavior for that minigame.

**Precedence:** do not contradict a higher-level SoT. Minigame docs may define local metrics/layout/behavior but must not redefine global palette tokens, base typography roles, or inherited button-relief/base style semantics.

## Engineering guardrails
- **Stack:** Godot **4.x / C# (.NET)**.
- **Architecture:** Clean Architecture & DDD principles; clear separation of domain/game logic and UI/presentation; composition over tight coupling (Godot nodes + C#).
- **Versioning:** strict **SemVer 2.0** (`MAJOR.MINOR.PATCH`) for releases and compatibility decisions.
- **Visual system:** adhere to the **immutable global color palette matrix** and visual inheritance in [`docs/art_direction.md`](docs/art_direction.md). All minigames inherit **base colors, button relief styles, and font tokens** from global; overrides are limited to documented, local metrics/layout/behavior.
- **Viewport:** mobile portrait baseline **360×640** (`canvas_items`), with safe-area-aware layouts.
- **Static grid standard (Word Search reference):** baseline **$8\times8$** visible letters, **$30\times30$ px** cells, **$6$ px** gaps (reference fidelity, e.g. Fácil / `wathersoup.jpg`). Difficulty variants (10×10/12×12/15×15) follow per-spec rules and must **fit without clipping** with hit-testing aligned to rendered geometry; treat as standards, not ad-hoc gameplay changes.

## Doc/code hygiene
- Update relevant docs when behavior or visual decisions change; avoid duplicating SoT across files.
- Prefer small, reviewable changes; do not invent metrics, CI status, or assets—mark unverifiable details for editor/device confirmation.
- Do not modify gameplay logic unless explicitly requested; keep `.md`/governance tasks documentation-only when scoped as such.
