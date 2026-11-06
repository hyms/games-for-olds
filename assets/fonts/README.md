# Fuentes

Fuentes incluidas en el proyecto para la Sopa de Letras según `docs/word_search/spec.md` §1:

| Archivo | Licencia | Archivo de licencia | Uso (spec §1) |
|---------|----------|---------------------|---------------|
| `PressStart2P.ttf` | SIL OFL 1.1 | `OFL-PressStart2P.txt` | §1.1 Títulos principales, banners (`PixelBanner`), modales de victoria y números destacados del temporizador |
| `VCR_OSD_Mono.ttf` | Freeware | `VCR-OSD-Mono-LICENSE.txt` | §1.2 HUD de control, cronómetro (`00:50`), contador de progreso (`1/5 Palabras`), botones (`PixelButton`) y mensajes estilo CRT/arcade |
| `PixelOperator.ttf` | CC0 1.0 | `CC0-PixelOperator.txt` | §1.3 Celdas del tablero (`PixelCell`), matriz de letras y lista de palabras pendientes del panel inferior |
| `AtkinsonHyperlegible-Regular.ttf` | SIL OFL 1.1 | `OFL-AtkinsonHyperlegible.txt` | Interfaz accesible global (`core/AccessibleTheme.tres`) |
| `AtkinsonHyperlegible-Bold.ttf` | SIL OFL 1.1 | `OFL-AtkinsonHyperlegible.txt` | Interfaz en negrita (usos puntuales) |

## Decisiones (spec §1)

- **Letras del tablero y lista de palabras:** `PixelOperator.ttf`, fuente pixel de máxima
  legibilidad en píxel art. Sustituye a `PressStart2P.ttf` en las celdas (spec §1.3).
- **HUD, cronómetro, contadores y botones:** `VCR_OSD_Mono.ttf`, estilo CRT/arcade (spec §1.2).
- **Títulos y modales:** `PressStart2P.ttf`, rejilla 8×8 acorde con la plantilla
  `docs/word_search/wathersoup.jpg` (spec §1.1). Como el juego normaliza a A–Z sin tildes
  ni Ñ, cubre todo lo necesario.
- **Interfaz accesible global:** Atkinson Hyperlegible (Braille Institute, perfil 40+/60+),
  fuente por defecto de `core/AccessibleTheme.tres` para el resto de la app.

## Uso en Godot

- El tema global (`core/AccessibleTheme.tres`) referencia
  `AtkinsonHyperlegible-Regular.ttf` como `default_font`.
- Celdas: `core/Constants.cs` → `FontCellPath` (PixelOperator), aplicado en
  `minigames/word_search/WordCell.cs`.
- HUD/botones de la Sopa: `VCR_OSD_Mono.ttf` en `WordSearchMain.tscn`
  (`TimeLabel`, `WordsCounter`, botones, ventanas de pausa/victoria).

## Cambiar de fuente

1. Colocar el `.ttf/.otf` aquí con su licencia.
2. Actualizar `core/Constants.cs` (rutas) y las escenas que referencian la tipografía.
3. Registrar el cambio en `docs/art_direction.md` §4 y en `assets/asset-manifest.json`.

> Nota: `VCR_OSD_Mono.ttf` (renombrado desde `VCR OSD Mono.ttf` para evitar espacios en la
> ruta del recurso) no es OFL (freeware de dafont.com); `PixelOperator.ttf` es CC0 1.0.
> Ambas licencias quedan documentadas en los archivos `VCR-OSD-Mono-LICENSE.txt` y
> `CC0-PixelOperator.txt`.