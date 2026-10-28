# Fuentes

Fuentes incluidas en el proyecto (todas con licencia **SIL Open Font License 1.1**,
texto original en cada `OFL-*.txt`):

| Archivo | Uso | Tamaño | Origen |
|---------|-----|--------|--------|
| `PressStart2P.ttf` | **Letras del tablero** de la Sopa (A–Z) | 24 px lógicos (múltiplo de 8 para que los píxeles queden nítidos) | [Google Fonts](https://fonts.google.com/specimen/Press+Start+2P) |
| `AtkinsonHyperlegible-Regular.ttf` | **Interfaz** (títulos, botones, listas) | 20 / 24 / 28 px lógicos | [Google Fonts](https://fonts.google.com/specimen/Atkinson+Hyperlegible) |
| `AtkinsonHyperlegible-Bold.ttf` | Interfaz en negrita (contador, resultados) | Igual que la regular | Google Fonts |

## Decisiones

- **Interfaz:** Atkinson Hyperlegible, diseñada por el Braille Institute para
  maximizar la legibilidad (perfil 40+/60+). Sustituye al `SystemFont` de
  `core/AccessibleTheme.tres`.
- **Letras del tablero:** Press Start 2P, fuente pixel de rejilla 8×8, acorde
  con la plantilla `docs/word_search/wathersoup.jpg`. Como el juego normaliza
  a A–Z sin tildes ni Ñ, el juego cubre todo lo necesario.

## Uso en Godot

- El tema global (`core/AccessibleTheme.tres`) referencia
  `AtkinsonHyperlegible-Regular.ttf` como `default_font`.
- Las letras del tablero se asignarán en la Fase 3/4 con un `Label` por celda
  usando `PressStart2P.ttf` a 24 px; comprobar en editor que el glifo cabe en
  la celda (41 px) y en 15×15 con zoom.

## Cambiar de fuente

1. Colocar el `.ttf/.otf` aquí con su licencia.
2. Actualizar `core/AccessibleTheme.tres` (interfaz) o el código de celdas (letras).
3. Registrar el cambio en `docs/art_direction.md` §5 y en `assets/asset-manifest.json`.
