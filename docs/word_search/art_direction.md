# Dirección de Arte — Sopa de Letras (Word Search)

> **Documento SDD de dirección de arte (minijuego)**.  
> **Versión:** 1.0 · **Estado:** Vigente · **Última actualización:** 2026-11-10  
> **Hereda de:** [Dirección de Arte Global](../art_direction.md) — **paleta global inmutable (§4), tokens tipográficos (§6), relieve/estados base de botones y componentes (§5, §7)**. Este documento **no redefine** colores, fuentes ni estilos base globales; solo especifica layout, métricas, comportamiento y mapeos del minijuego.  
> **Referencia visual:** `wathersoup.jpg` (misma carpeta). Los valores de “fidelidad 1:1” deben verificarse contra esta imagen en editor/dispositivo.

---

## 1. Alcance y herencia

- **Aplica a:** `minigames/word_search/` (escena principal `WordSearchMain.tscn`, HUD, tablero, footer/WordsPanel, modales de pausa/victoria).
- **Fuente de verdad de color/estados:** tokens de la [matriz global](../art_direction.md#4-matriz-inmutable-de-paleta-global-autoridad-canónica) (`BackgroundColor`, `Cell*`, `Footer*`, `Turquesa/CellFound*`, `PistaAcento/PistaBorde`, `Trazo/CeldaSeleccion`, `Tinta/Borde/Sombra`, etc.). Usar **nombres de token**, no valores hex ad hoc.
- **Tipografía y relieve:** roles y comportamiento heredados de global; este documento solo indica **qué rol se aplica a cada elemento** de Word Search.
- **Overrides locales permitidos:** métricas (tamaños, márgenes, alturas, gaps), composición de layout y comportamiento (zoom, partículas, feedback). Cualquier ajuste de geometría/relieve específico para igualar la referencia debe quedar documentado aquí como excepción de alcance local, sin alterar la paleta global.

---

## 2. Regiones de layout y safe area

| Región | Nodo/área (referencia) | Rol |
|---|---|---|
| Cabecera | `%TitlePanel`, `%SubtitlePanel` (`Header`) | Título “SOPA DE LETRAS” y etiqueta de dificultad/nivel |
| Navegación/HUD | `NavRow` (`%BtnBack`, `%TimeLabel`, `%WordsCounter`, `%BtnPause`) | Volver, cronómetro no punitivo, progreso `X/Y Palabras`, pausa |
| Tablero | `%BoardArea` (flex) + `WordGridContainer` | Grilla de letras, selección por arrastre y trazo |
| Footer / WordsPanel | `%WordPanel` → `%WordMargin` → `%WordList` | Lista de palabras pendientes/encontradas (flujo con separador) |
| Modales | `%PauseModal`, `%WinModal` | Pausa (continuar/pista/reiniciar/salir) y victoria (puntaje/tiempo/acciones) |

- **Safe area:** hereda reglas globales. `SafeAreaManager` aplica insets del display con mínimos por panel (Word Search: top ≥ 32, resto ≥ 8) sobre el `MarginContainer` de la escena (`margin_*` base de escena 48/16/16/16). El contenido crítico (HUD, tablero, WordsPanel) debe permanecer visible y no solaparse con notch/gestos del sistema.
- **Viewport de referencia:** 360×640 (`canvas_items`); validar también en anchos altos (20:9) y tablet.

---

## 3. Reglas de grilla y métricas del tablero

### 3.1 Baseline estático de referencia (fidelidad visual)
**Estándar estático de grilla (referencia/baseline):** para alineación 1:1 con `wathersoup.jpg`, la grilla visible de referencia se documenta como:
- **Matriz visible:** $8 \times 8$ letras.
- **Celda:** $30 \times 30$ px.
- **Separación (gap):** $6$ px entre celdas.

> **Nota de autoridad métrica:** este baseline es el objetivo visual de referencia (p. ej. dificultad Fácil). La implementación debe evitar clipping y mantener legibilidad/alineación de hit-testing en el `BoardArea` disponible (política **target→fit** cuando el área útil no permita el tamaño objetivo exacto). Verificar medidas finales contra `wathersoup.jpg` en editor/dispositivo.

### 3.2 Variantes por dificultad (datos) y comportamiento de ajuste
| Dificultad | Grilla (datos) | Palabras objetivo | Direcciones / categorías | Estado métrico |
|---|---|---:|---|---|
| Fácil | $8 \times 8$ | 5 | H/V directas · 1 categoría | **Baseline:** $30\times30$ px, gap $6$ px; **ajuste al área disponible si es necesario (target→fit, sin clipping)** |
| Medio | $10 \times 10$ | 8 | + diagonales descendentes · 1 | Objetivo de referencia por definir vs `wathersoup.jpg`; **runtime actual: fit-to-area** |
| Difícil | $12 \times 12$ | 12 | 8 direcciones · 2 | Objetivo de referencia por definir vs `wathersoup.jpg`; **runtime actual: fit-to-area** |
| Maestro | $15 \times 15$ | 20 | 8 direcciones · 4 | Objetivo de referencia por definir vs `wathersoup.jpg`; **runtime actual: fit-to-area** |

- **Fuente de datos:** `resources/difficulties/{facil,medio,dificil,maestro}.tres`, `DifficultyConfig.cs`/`DifficultyCatalog.cs`.
- **Runtime actual (implementación):** `WordGridFactory.ComputeLayout` calcula `cellSize = max(MinCellSize, floor(fit))` con `gap = WordSearchVisuals.CellGap` (6.0) centrando la grilla en `BoardArea` (`WordGridContainer` aplica posición/tamaño y deriva tamaño de fuente). **No** existe hoy una métrica fija $30\times30$ por dificultad en código; cualquier discrepancia con el baseline debe resolverse como trabajo de alineación visual explícito (ver `minigames/word_search/WordSearchVisuals.cs`).
- **Criterios de aceptación visual:** cero clipping horizontal/vertical, celdas centradas, espaciado uniforme, y **hit-testing/centros de trazo exactamente alineados** a la geometría renderizada.

---

## 4. Controles de zoom y accesibilidad

- **Objetivo de interacción:** zoom de tablero accesible y no dependiente de un único gesto.
- **Controles requeridos (UX):** botones **`+`** / **`−`** para ampliar/reducir y control de **centrar/restablecer** vista (alternativa sin gestos finos), además de soporte de pellizco (pinch) cuando esté disponible.
- **Parámetros de referencia (visual/comportamiento):** rango $1.0\times$–$2.5\times$, paso incremental acorde a referencia/implementación, y reposición al iniciar/reiniciar partida si así lo define la UX del minijuego.
- **Estado actual:** los límites/paso de zoom están definidos en `WordSearchVisuals` (`MinZoom/MaxZoom/ZoomStep`); la presencia y disposición final de los controles `+ / −` y centrar en la UI deben verificarse contra escena/implementación y `wathersoup.jpg` durante la alineación.
- **Regla:** el zoom no debe romper la selección por arrastre ni perder el tablero fuera de límites visibles.

---

## 5. Footer / WordsPanel (dimensiones y visibilidad)

- **Componente:** `%WordPanel` (PanelContainer) con `%WordList` (`RichTextLabel`, BBCode) — presentación orquestada por `WordSearchHud` y estilos de `PixelStyles.WordPanel()` aplicados en `WordSearchMain.ApplyTheme`.
- **Altura mínima objetivo (requerida):** **~70 px mínimos efectivos** para garantizar visibilidad estable del panel en 360×640 y evitar que el `BoardArea` lo comprima o desplace fuera de pantalla. El panel debe comportarse como banda inferior **no colapsable** y compatible con safe area.
- **Estado actual (a alinear):** `custom_minimum_size.y` de `WordPanel` es 48 y el layout depende de `fit_content`/márgenes internos; puede ser insuficiente con 12–20 palabras o mayor inset superior.
- **Contenido:** palabras en flujo continuo separadas por ` | `; pendientes visibles y encontradas tachadas/coloreadas mediante **tokens globales** (`FooterTextColor`, `FooterStrikethroughColor` / acento de encontrado según tema). Legible con 5, 8, 12 y 20 palabras.
- **Restricción:** el footer no debe solaparse con el tablero ni quedar oculto por gestos/notch; `scroll` solo si lo exige la referencia (preferir ajuste de altura/padding/flujo).

---

## 6. Mapeo tipográfico (roles globales → Word Search)

| Elemento | Rol global | Fuente (token de rol) | Notas |
|---|---|---|---|
| Título principal / victoria | Display / Título | `PressStart2P.ttf` | Jerarquía alta, legible sobre banner |
| Dificultad/subtítulo, timer, contador, botones HUD/modales | HUD / Mono / UI técnica | `VCR_OSD_Mono.ttf` | Claridad numérica y de controles |
| Letras de grilla y lista de palabras | Contenido de grilla / listado | `PixelOperator.ttf` | Máxima legibilidad en celdas; peso/refuerzo visual según referencia (sin alterar token) |
| Ajustes/accesibilidad global | UI accesible | `AtkinsonHyperlegible` | Cuando aplique fuera del tablero/juego |

---

## 7. Estados visuales y feedback (referenciando paleta global)

- **Celda idle / selected / found / hint:** usar tokens `CellIdle*`, `CeldaSeleccion`/`Trazo` (selección/trazo), `CellFound*`/`Turquesa`/`EncontradaBorde`, `PistaAcento`/`PistaBorde`. Estados encontrados/activos deben ser sólidos (sin “ghosting”) y distinguibles por forma/borde además de color.
- **Trazo de selección:** grosor/opacidad definidos por implementación visual coherente con referencia; color basado en tokens de trazo/selección globales.
- **Footer:** texto y tachado por `FooterTextColor` y `FooterStrikethroughColor`.

---

## 8. Partículas y celebración (confeti)

- **Disparador:** al completar la última palabra del tablero (victoria de partida).
- **Comportamiento de referencia (spec):** detener cronómetro al completar; activar lluvia de confeti pixel art y, tras **~300 ms**, mostrar modal de victoria (`%WinModal`) con tiempo, puntaje y acciones.
- **Estilo:** partículas **cuadradas tipo pixel** (p. ej. 4×4 o 6×6 px lógicos, sin suavizado) con colores derivados **exclusivamente de tokens de la paleta global aprobada** (p. ej. acentos/estados como `Turquesa`/`CellFound*`, `Amarillo`, `Cian`, `CeldaSeleccion`, `TextLight` según composición), con rendimiento adecuado en gama baja. **No** introducir colores fuera de la matriz global.
- **Estado actual:** la lógica de finalización y modal existe; el emisor de confeti dedicado debe verificarse/implementarse según esta especificación y `wathersoup.jpg` (si no está presente, tratar como brecha de alineación visual).

### 8.1 Paleta de confeti (v0.5.0)

El estallido de victoria usa **cuatro** tokens de la paleta global, combinados en una `ColorRamp` del `CpuParticles2D`:

| Token | Hex | Rol en el confeti |
|---|---|---|
| `Turquesa` / `CellFoundBackgroundColor` | `#099C92` | Color principal del estallido (acentos fríos) |
| `Amarillo` | `#FDC109` | Acento cálido intermedio |
| `Cian` | `#08D0DF` | Acento frío claro |
| `CelebrationRed` | `#B45252` | Acento cálido coral (**token agregado v0.5.0**, ver [`../art_direction.md`](../art_direction.md) §4) |

**Restricción:** los colores del confeti **proceden exclusivamente de esta tabla**. No se permiten valores hex ad hoc. El emisor vive en `core/ui/PixelConfettiEmitter.cs` y consume los tokens vía `ThemeData`/`PixelStyles`.

**Regla de disparo:** confeti **solo en victoria** (al completar el tablero). Al encontrar una palabra individual no se dispara confeti: esa palabra recibe animación sutil de celda (`WordCell` pop) y tachado en el footer (`~~PALABRA~~` con `FooterStrikethroughColor`), tal como hace el sistema actual.

### 8.2 Revisión de paleta local (v0.5.0, Opción 1)

Durante la alineación V1 se evaluó una propuesta de matriz de colores alternativa (`#68c2d3` título / `#ede19e` subtítulo / `#a77b5b` botones / `#f2f0e5` panel / confeti `#ede19e`+`#a2dcc7`+`#b45252`). **Se rechazó como desviación** por contradecir la herencia visual obligatoria (`agents.md` y [`../art_direction.md`](../art_direction.md) §7): los minijuegos **no pueden redefinir** los tokens globales de paleta, tipografía ni relieve base.

| Elemento propuesto | Decisión final (Opción 1) | Token canónico usado |
|---|---|---|
| Título | `#6C929D` (override local en `PixelStyles.TitlePanel()`) | `HeaderBannerColor` #A0BFC7 no se toca |
| Subtítulo | `SubheaderBannerColor` #477987 | canonical |
| Botones | `HeaderBannerColor` #A0BFC7 (normal) / #477987 (hover) | `PixelButtonStyle.FillFor` |
| Panel de palabras | `Placa` #EFEFEE | `PixelStyles.WordPanel()` |
| Confeti | `Turquesa` #099C92 + `Amarillo` #FDC109 + `Cian` #08D0DF + `CelebrationRed` #B45252 | tokens globales + 1 agregado |

La única extensión aprobada a la matriz global es `CelebrationRed` (#B45252), agregada de forma **aditiva** en v0.5.0 (SemVer MINOR) sin alterar ningún token existente.

---

## 9. Referencias

- **Global:** [`../art_direction.md`](../art_direction.md)
- **Spec/plan del minijuego:** [`spec.md`](spec.md), [`plan.md`](plan.md)
- **Referencia visual:** [`wathersoup.jpg`](wathersoup.jpg)
- **Implementación:** `minigames/word_search/WordSearchMain.tscn`, `WordSearchMain.cs`, `WordGridContainer.cs`, `WordGridFactory.cs`, `WordSearchVisuals.cs`, `WordSearchHud.cs`, `WordCell.cs`, `core/SafeAreaManager.cs`, `core/ui/PixelStyles.cs`, `core/ThemeData.cs`

## 10. Verificación

- [ ] Comparar layout, radios/relieve, tipografía y colores contra `wathersoup.jpg` en editor/dispositivo.
- [ ] Confirmar baseline $8\times8$ / $30\times30$ px / $6$ px gap y política target→fit para otras dificultades sin clipping.
- [ ] Confirmar controles de zoom `+ / −` y centrado, y que no interfieren con el arrastre.
- [ ] Confirmar `WordsPanel` con altura mínima efectiva ~70 px visible en 360×640 (5–20 palabras) y con safe area.
- [ ] Confirmar confeti pixel (tamaño/tiempo ~300 ms antes de modal) y ausencia de redefiniciones de paleta global.
