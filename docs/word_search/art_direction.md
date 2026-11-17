# Dirección de Arte — Sopa de Letras (Word Search)

> **Documento SDD de dirección de arte (minijuego)**.  
> **Versión:** 1.1 · **Estado:** Vigente · **Última actualización:** 2026-11-17  
> **Hereda de:** [Dirección de Arte Global](../art_direction.md) — **paleta global (§4), tokens tipográficos (§6), relieve/estados base de botones y componentes (§5, §7)**. Este documento **no redefine** fuentes ni estilos base globales; especifica layout, métricas, comportamiento y el **mapeo de tokens de Word Search** (`WS-*`, §2.2).  
> **Referencia visual:** `wathersoup.jpeg` (misma carpeta). Todos los valores de “fidelidad 1:1” se verifican contra esta imagen en editor/dispositivo.

---

## 1. Alcance y herencia

- **Aplica a:** `minigames/word_search/` (escena principal `WordSearchMain.tscn`, HUD, tablero, footer/WordsPanel, modales de pausa/victoria).
- **Fuente de verdad de color/estados:** tokens de la [matriz global](../art_direction.md#4-matriz-inmutable-de-paleta-global-autoridad-canónica). Desde **v1.1** el minijuego mapea los tokens a una **sub-paleta cálida madera/tierra** (§2.2, `WS-*`); las superficies frías heredadas (`Placa`, `HeaderBannerColor`, `Turquesa`) quedan **deprecadas** para Word Search. Usar **nombres de token**, no valores hex ad hoc.
- **Tipografía y relieve:** roles heredados de global; este documento solo indica **qué rol se aplica a cada elemento** de Word Search.
- **Overrides locales permitidos:** métricas (tamaños, márgenes, alturas, gaps), composición de layout y comportamiento (zoom, partículas, feedback). Cualquier ajuste específico para igualar la referencia queda documentado aquí como excepción de alcance local, sin alterar la semántica global de estados.

---

## 2. Regiones de layout y safe area

| Región | Nodo/área (referencia) | Rol |
|---|---|---|
| Fondo | `Background` | Superficie madera/tierra (`WS-Fondo`, §2.2) |
| Cabecera | `%TitlePanel`, `%SubtitlePanel` (`Header`) | Título “SOPA DE LETRAS” y etiqueta de dificultad/nivel |
| Navegación/HUD | `NavRow` (`%BtnBack` `← Atrás`, `%TimeBox` → `%TimeLabel` + `%WordsCounter`, `%BtnPause`, `%BtnZoomOut` `−`, `%BtnZoomIn` `+`, `NavBalance` 0 px) | Volver, cronómetro no punitivo, progreso `X/Y Palabras`, pausa, zoom |
| Tablero | `%BoardArea` (flex) + `WordGridContainer` (+ `BoardFrame`) | Grilla de letras, selección por arrastre y trazo |
| Footer / WordsPanel | `%WordsPanel` (script `WordsPanel.cs`) → `%WordList` (runtime) | Lista de palabras pendientes/encontradas (flujo con separador) |
| Modales | `%PauseModal`, `%WinModal` | Pausa (continuar/pista/reiniciar/salir) y victoria (puntaje/tiempo/acciones) |

- **Safe area:** hereda reglas globales. `SafeAreaManager` aplica insets del display con mínimos por panel (Word Search: top ≥ 32, resto ≥ 8) sobre el `MarginContainer` de la escena (`margin_*` base de escena 48/16/16/16).
- **Viewport de referencia:** 360×640 (`canvas_items`); validar también en anchos altos (20:9) y tablet.

### 2.1 Estilos de cabecera (fidelidad con `wathersoup.jpeg`)

En la referencia la cabecera **no usa placas**: el título se escribe directamente sobre el fondo madera y la banda de dificultad es una placa crema. Relieve pixel art: 2 px de borde, `AntiAliasing = false`, `CornerRadius = 0` (radio 0 en placas, celdas y footer; solo el botón Pausa muestra esquinas redondeadas ≈ 4 px, §2.2).

| Elemento | Fondo | Texto | Fuente | Rol / contraste |
|---|---|---|---|---|
| `%TitlePanel` | `WS-Fondo` (`#A17A5B`) — sin placa | `WS-CianTitulo` `#65B9C4` | `PressStart2P.ttf` 24 px | ~1.7:1 sobre madera — **insuficiente**: el título debe llevar un contorno duro `WS-MaderaSombra` `#3A2A1E` (4.19:1 local) o placa clara para cumplir ≥ 3:1 en display |
| `%SubtitlePanel` | `WS-Crema` `#EDE19E` + borde `WS-CremaBorde` `#C4B476` | `TextDark` `#101010` | `VCR_OSD_Mono.ttf` 14 px | ~14.4:1 |
| `%TimeBox` | `WS-CremaSuave` `#F1EFE2` | `TextDark` `#101010` | `VCR_OSD_Mono.ttf` 14/12 px | ~16:1 |
| Botones `NavRow` (`← Atrás`, `Pausa`) | `WS-Madera` `#A17A5B` + borde `WS-MaderaBorde` `#3A2A1E` | `TextDark` `#101010` | `VCR_OSD_Mono.ttf` 16 px | 4.94:1 (texto) / 3.56:1 (borde ≥ 3:1) |

`%TimeBox` es un `PanelContainer` con padding interno 12 px horizontal / 4 px vertical que agrupa el cronómetro (`%TimeLabel`, 14 px) y su sub-etiqueta (`%WordsCounter`, 12 px, `0/5 Palabras`), ambos en `TextDark`, `mouse_filter = Ignore` y **borderless** en la referencia (la placa crema puede llevar borde `WS-CremaBorde` como refuerzo de forma).

Los botones de navegación toman el **mismo relleno madera que el fondo** y se distinguen por borde `WS-MaderaBorde` (contraste de forma/borde, no de color). Un borde oscuro (`WS-MaderaBorde` `#3A2A1E` = 3.56:1 contra el relleno madera) mantiene la jerarquía de controles accionables sobre placas informativas heredada de v0.4.5, sin introducir rellenos fríos. `%BtnZoomOut`/`%BtnZoomIn` (integrados en el modal de pausa desde v0.6.0) heredan el botón `WS-Acento` para mayor prominencia.

### 2.2 Sub-paleta de Word Search `WS-*` (rev. 1.1 — madera/tierra)

**Decisión (v1.1):** la referencia `wathersoup.jpeg` usa una dirección **cálida madera/tierra** (fondo marrón, placas crema, acento coral). Esto **supersede** la restricción previa de “neutros fríos estrictos” (`Placa` `#EFEFEE`, `HeaderBannerColor` `#A0BFC7`, `SubheaderBannerColor` `#477987`, `Turquesa` `#099C92`) **exclusivamente en el alcance de Word Search**.

> **Nota de gobernanza:** `WS-*` es una **extensión aditiva de alcance local** (MINOR), igual que `CreamYellow` (v0.5.1). No renombra ni sustituye tokens globales de la [§4 global](../art_direction.md#4-matriz-inmutable-de-paleta-global-autoridad-canónica); documenta el mapeo que Word Search usa para el mockup. Valores muestreados de la imagen (con tolerancia ±2 por compresión JPEG) y **pendientes de formalizar en `ThemeData`/`PixelStyles`**.

| Token `WS-*` | Hex | Rol | Contraste sobre su texto | Sustituye (deprecado para WS) |
|---|---|---|---|---|
| `WS-Fondo` / `WS-Madera` | `#A17A5B` | Fondo de escena y relleno de botones HUD | — | `Placa` `#EFEFEE` como fondo |
| `WS-MaderaBorde` | `#3A2A1E` | Borde visible de botones, marco del tablero y del footer | 3.56:1 contra `WS-Madera` (≥ 3:1) | `Borde` `#477987` / `TextDark` |
| `WS-MaderaSombra` | `#2E1D12` | Contorno/sombra dura del título y separadores | 4.19:1 contra `WS-Madera` | `Sombra` `#8A9AAA` |
| `WS-Crema` | `#EDE19E` | Placa de dificultad (banda de nivel) | 14.4:1 (`TextDark`) | `CreamYellow` (se conserva su rol) |
| `WS-CremaBorde` | `#C4B476` | Borde de la placa de dificultad | ≈ 2.5:1 (decorativo; ≥ 3:1 con `TextDark` outline) | `Borde` `#477987` |
| `WS-CremaSuave` | `#F1EFE2` | `%TimeBox` y tarjeta del footer | 16:1 (`TextDark`) | `Placa` `#EFEFEE` en placas informativas |
| `WS-Celda` | `#F1EDE1` | Relleno de celda idle | 16:1 (`TextDark`) | `CellIdleBackgroundColor` `#EBF2F5` |
| `WS-CeldaBorde` | `#A17A5B` | Borde interior de celda idle (discontinuo con el fondo) | — | `CellIdleBorderColor` `#A0BFC7` |
| `WS-Acento` / `WS-Encontrado` | `#FD6D52` | **Acierto/selección/encontrado** (coral sólido, alpha 1.0) | 6.78:1 (`TextDark`) / 2.43:1 contra crema (requiere borde) | `Turquesa` `#099C92`, `CeldaSeleccion` |
| `WS-EncontradoBorde` | `#8E3B33` | Borde/contorno de celda encontrada (refuerzo de forma) | 6.44:1 contra `WS-CremaSuave`; texto `TextDark` 4.79:1 | `EncontradaBorde` `#06706A` |
| `WS-CeldaAviso` | `#B07259` | Variante de celda en foco/hover/hint suave | 4.89:1 (`TextDark`) | `PistaAcento` (solo en pista explícita) |
| `WS-CianTitulo` | `#65B9C4` | Título sobre fondo madera (siempre con contorno `WS-MaderaSombra`) | 1.7:1 crudo / 4.19:1 con contorno oscuro | `HeaderBannerColor` `#A0BFC7` |
| `WS-Tinta` | `#101010` | Texto de grilla, HUD, footer y tachado base | 16:1 | `TextDark` (se conserva) |
| `WS-Tachado` | `#9B4E48` | Tachado + texto de palabra encontrada en el footer | 5.08:1 sobre `WS-CremaSuave` | `FooterStrikethroughColor` `#099C92` |

**Reglas de paleta (obligatorias):**
1. Prohibido reintroducir superficies frías (`#EFEFEE`, `#A0BFC7`, `#477987`, `#099C92`) en Word Search; el único frío admitido es el cian del título `WS-CianTitulo`.
2. El **acento de encontrado/selección es coral** (`WS-Acento`), no turquesa; el estado debe ser **sólido** (alpha 1.0, sin ghosting) y distinguirse además por borde (`WS-EncontradoBorde`).
3. Todo texto sobre placa/celda crema usa `TextDark` `#101010`; no se admite texto claro sobre crema (falla 4.5:1).
4. Bordes de controles accionables ≥ 3:1; bordes decorativos (placa de dificultad, celda idle) pueden ser de bajo contraste si la forma ya separa la superficie.
5. `CelebrationRed` `#B45252` (global, v0.5.0) **sigue vigente** y es coherente con la familia cálida; se mantiene para defeat/celebración.

> **Implementación:** el mapeo `WS-*` se concentra en `core/ThemeData.cs` + `core/ui/PixelStyles.cs`/`PixelCellStyle.cs` (Flyweight por `ThemeId`, §Phase 11 de `plan_audit.md`). Esta tabla es el contrato de valores a replicar.

---

## 3. Reglas de grilla y métricas del tablero

### 3.1 Baseline estático de referencia (fidelidad visual)
**Estándar estático de grilla (referencia/baseline):** para alineación 1:1 con `wathersoup.jpeg`, la grilla visible de referencia se documenta como:
- **Matriz visible:** $8 \times 8$ letras.
- **Celda:** $30 \times 30$ px (esquinas redondeadas ≈ 3–4 px, radio local admitido por §1).
- **Separación (gap):** $6$ px entre celdas.
- **Marco:** `BoardFrame` sobre `WS-Fondo` con borde `WS-MaderaBorde` 3 px (decorativo), grilla aireada en madera.

> **Nota de autoridad métrica:** este baseline es el objetivo visual de referencia (dificultad Fácil). La implementación debe evitar clipping y mantener legibilidad/alineación de hit-testing en el `BoardArea` disponible (política **target→fit** cuando el área útil no permita el tamaño objetivo exacto).

### 3.2 Variantes por dificultad (datos) y comportamiento de ajuste
| Dificultad | Grilla (datos) | Palabras objetivo | Direcciones / categorías | Estado métrico |
|---|---|---:|---|---|
| Fácil | $8 \times 8$ | 5 | H/V directas · 1 categoría | **Baseline:** $30\times30$ px, gap $6$ px; **ajuste al área disponible si es necesario (target→fit, sin clipping)** |
| Medio | $10 \times 10$ | 8 | + diagonales descendentes · 1 | Objetivo de referencia por definir vs `wathersoup.jpeg`; **runtime actual: fit-to-area** |
| Difícil | $12 \times 12$ | 12 | 8 direcciones · 2 | Objetivo de referencia por definir vs `wathersoup.jpeg`; **runtime actual: fit-to-area** |
| Maestro | $15 \times 15$ | 20 | 8 direcciones · 4 | Objetivo de referencia por definir vs `wathersoup.jpeg`; **runtime actual: fit-to-area** |

- **Fuente de datos:** `resources/difficulties/{facil,medio,dificil,maestro}.tres`, `DifficultyConfig.cs`/`DifficultyCatalog.cs`.
- **Runtime actual (implementación):** `WordGridFactory.ComputeLayout` calcula `cellSize = max(MinCellSize, floor(fit))` con `gap = WordSearchVisuals.CellGap` (6.0) centrando la grilla en `BoardArea`. **No** existe hoy una métrica fija $30\times30$ por dificultad en código; cualquier discrepancia con el baseline debe resolverse como trabajo de alineación visual explícito (ver `minigames/word_search/WordSearchVisuals.cs`).
- **Criterios de aceptación visual:** cero clipping horizontal/vertical, celdas centradas, espaciado uniforme, y **hit-testing/centros de trazo exactamente alineados** a la geometría renderizada.

---

## 4. Controles de zoom y accesibilidad

- **Objetivo de interacción:** zoom de tablero accesible y no dependiente de un único gesto.
- **Controles requeridos (UX):** botones **`+`** / **`−`** para ampliar/reducir y control de **centrar/restablecer** vista (alternativa sin gestos finos), además de soporte de pellizco (pinch) cuando esté disponible.
- **Parámetros de referencia:** rango $1.0\times$–$2.5\times$, paso incremental acorde a implementación, y reposición al iniciar/reiniciar partida.
- **Ubicación (v0.6.0, alineado con `wathersoup.jpeg`):** el mockup muestra una `NavRow` de una sola fila (`Atrás` · `TIEMPO` · `Pausa`); los botones `−` / `+` viven en el `%PauseModal` para no saturar el HUD (regla R1 de `plan_audit.md` v0.6.0). No existe `%ZoomOverlay` flotante.
- **Pendiente:** el control de **centrar/restablecer** vista sigue sin UI propia (solo existe `WordGridContainer.ResetZoom()`).
- **Regla:** el zoom no debe romper la selección por arrastre ni perder el tablero fuera de límites visibles.

---

## 5. Footer / WordsPanel (dimensiones y visibilidad)

- **Componente:** `%WordPanel` (PanelContainer) con `%WordList` (`RichTextLabel`, BBCode) — presentación orquestada por `WordSearchHud` y estilos de `PixelStyles.WordPanel()` aplicados en `WordSearchMain.ApplyTheme`.
- **Estilo (v0.6.0 / rev. 1.1):** tarjeta `WS-CremaSuave` `#F1EFE2` sobre `WS-Fondo`, borde `WS-MaderaBorde` 3 px, esquinas 0, texto `TextDark`. Debe leerse como una tarjeta crema independiente del tablero (contraste de superficie, no de matiz frío).
- **Altura mínima objetivo (requerida):** **~70 px mínimos efectivos** para garantizar visibilidad estable del panel en 360×640 y evitar que el `BoardArea` lo comprima o desplace fuera de pantalla. El panel se comporta como banda inferior **no colapsable** y compatible con safe area.
- **Contenido:** palabras pendientes en flujo continuo separadas por ` | ` (`TextDark`); palabras encontradas **tachadas** y coloreadas con `WS-Tachado` `#9B4E48` (5.08:1 sobre crema). Legible con 5, 8, 12 y 20 palabras.
- **Restricción:** el footer no debe solaparse con el tablero ni quedar oculto por gestos/notch; `scroll` solo si lo exige la referencia.

---

## 6. Mapeo tipográfico (roles globales → Word Search)

| Elemento | Rol global | Fuente (token de rol) | Notas |
|---|---|---|---|
| Título principal / victoria | Display / Título | `PressStart2P.ttf` | `WS-CianTitulo` `#65B9C4` sobre `WS-Fondo` madera |
| Dificultad/subtítulo | HUD / Mono / UI técnica | `VCR_OSD_Mono.ttf` 14 px | `TextDark` `#101010` sobre `WS-Crema` `#EDE19E` |
| Timer (cronómetro) | HUD / Mono / UI técnica | `VCR_OSD_Mono.ttf` 14 px | Dentro de `%TimeBox` con fondo `WS-CremaSuave` `#F1EFE2`; texto `TextDark` |
| Contador | HUD / Mono / UI técnica | `VCR_OSD_Mono.ttf` | Claridad numérica y de controles |
| Botones HUD/modales | HUD / Mono / UI técnica | `VCR_OSD_Mono.ttf` / `AtkinsonHyperlegible` | Etiquetas de pausa/atrás en `TextDark` sobre botón madera |
| Letras de grilla y lista de palabras | Contenido de grilla / listado | `PixelOperator.ttf` | Máxima legibilidad en celdas; peso/refuerzo visual según referencia |
| Ajustes/accesibilidad global | UI accesible | `AtkinsonHyperlegible` | Cuando aplique fuera del tablero/juego |

---

## 7. Estados visuales y feedback (mapeados a §2.2)

| Estado | Relleno | Borde | Texto |
|---|---|---|---|
| Idle | `WS-Celda` `#F1EDE1` | `WS-CeldaBorde` `#A17A5B` (1 px) | `TextDark` `#101010` |
| Hover/Foco | `WS-CeldaAviso` `#B07259` | `WS-MaderaBorde` (2 px) | `TextDark` |
| Encontrado / seleccionado | `WS-Acento` `#FD6D52` (sólido, alpha 1.0) | `WS-EncontradoBorde` `#8E3B33` (2 px) | `TextDark` |
| Pista (explícita) | `WS-Celda` + `PistaAcento` `#F9881E` (global) | `PistaBorde` `#8A4600` | `TextDark` |

- Los estados encontrados/activos son **sólidos** (sin “ghosting”) y distinguibles por forma/borde además de color.
- **Trazo de selección:** grosor/opacidad según implementación coherente con la referencia; color `WS-Acento`. `Trazo` (#2B56B8) queda **no usado** en Word Search por incompatibilidad con la familia cálida.
- **Footer:** texto `TextDark`, tachado `WS-Tachado`.

---

## 8. Partículas y celebración (confeti)

- **Disparador:** al completar la última palabra del tablero (victoria de partida).
- **Comportamiento de referencia (spec):** detener cronómetro al completar; activar lluvia de confeti pixel art y, tras **~300 ms**, mostrar modal de victoria (`%WinModal`) con tiempo, puntaje y acciones.
- **Estilo:** partículas **cuadradas tipo pixel** (4×4 o 6×6 px lógicos, sin suavizado) con colores derivados **exclusivamente** de los tokens de §8.1; rendimiento adecuado en gama baja.
- **Estado actual:** la lógica de finalización y modal existe; el emisor de confeti dedicado debe verificarse/implementarse según esta especificación.

### 8.1 Paleta de confeti (rev. 1.1 — familia cálida)

El mockup muestra confeti en la misma familia cálida del tablero. La paleta se reescribe para eliminar acentos fríos (`Turquesa`, `Cian`):

| Token | Hex | Rol en el confeti |
|---|---|---|
| `WS-Acento` / `WS-Encontrado` | `#FD6D52` | Color principal del estallido (coral, coherente con celdas encontradas) |
| `CelebrationRed` | `#B45252` | Acento cálido profundo (global, v0.5.0) |
| `WS-Crema` | `#EDE19E` | Chispa crema/dorada (placa de dificultad) |
| `Amarillo` | `#FDC109` | Acento dorado brillante (global) |

**Restricción:** los colores del confeti **proceden exclusivamente de esta tabla**. No se admiten `Turquesa`/`Cian` ni valores hex ad hoc. El emisor vive en `core/ui/PixelConfettiEmitter.cs` y consume los tokens vía `ThemeData`/`PixelStyles`.

**Regla de disparo:** confeti **solo en victoria**. Al encontrar una palabra individual no se dispara confeti: esa palabra recibe animación sutil de celda (`WordCell` pop) y tachado en el footer (`~~PALABRA~~` con `WS-Tachado`).

### 8.2 Revisión de paleta local (historial de decisiones)

| Propuesta | Decisión | Estado actual (rev. 1.1) |
|---|---|---|
| v0.4.x — neutros fríos (`#EFEFEE`, `#A0BFC7`, `#477987`, `#099C92`) | Aceptada entonces como “matriz fría estricta” | **Supersedida** por la dirección cálida del mockup |
| Matriz alternativa V1 (`#68c2d3` título / `#ede19e` subtítulo / `#a77b5b` botones / `#f2f0e5` panel) | Rechazada en v0.5.0 por contradecir la herencia fría | **Revalidada y adoptada** en v1.1 vía tokens `WS-*` (§2.2): la referencia mockup es la autoridad visual |
| `CreamYellow` `#EDE19E` (global, v0.5.1) | Aceptada como extensión aditiva | Vigente, ahora como `WS-Crema` |

`CelebrationRed` (#B45252, v0.5.0) y `CreamYellow` (#EDE19E, v0.5.1) siguen siendo las extensiones globales aprobadas; la sub-paleta `WS-*` es una extensión **de alcance local** documentada aquí.

---

## 9. Referencias

- **Global:** [`../art_direction.md`](../art_direction.md)
- **Spec/plan del minijuego:** [`spec.md`](spec.md), [`plan.md`](plan.md)
- **Auditoría/roadmap:** [`plan_audit.md`](plan_audit.md)
- **Referencia visual:** [`wathersoup.jpeg`](wathersoup.jpeg)
- **Implementación:** `minigames/word_search/WordSearchMain.tscn`, `WordSearchMain.cs`, `WordGridContainer.cs`, `WordGridFactory.cs`, `WordSearchVisuals.cs`, `WordSearchHud.cs`, `WordCell.cs`, `core/SafeAreaManager.cs`, `core/ui/PixelStyles.cs`, `core/ThemeData.cs`

## 10. Verificación

- [x] Comparar layout, radios/relieve, tipografía y colores contra `wathersoup.jpeg`.
- [x] **rev. 1.1:** muestra de color del mockup (fondo `#A17A5B`, título `#65B9C4` + contorno `#3A2A1E`, dificultad `#EDE19E`, celdas `#F1EDE1`, encontrado `#FD6D52`/borde `#8E3B33`, footer `#F1EFE2`, tachado `#9B4E48`) registrada en §2.2 y aplicada a §2.1/§5/§7/§8.
- [ ] Implementar tokens `WS-*` en `ThemeData`/`PixelStyles` y retirar superficies frías en las escenas de Word Search.
- [ ] Confirmar baseline $8\times8$ / $30\times30$ px / $6$ px gap y política target→fit para otras dificultades sin clipping.
- [ ] Confirmar el control de **centrar/restablecer** vista (hoy solo `ResetZoom()`, sin botón) y que el zoom del `PauseModal` no interfiera con el arrastre en 15×15.
- [ ] Confirmar `WordsPanel` con altura mínima efectiva ~70 px visible en 360×640 (5–20 palabras) y con safe area.
- [ ] Confirmar confeti pixel cálido (tamaño/tiempo ~300 ms antes de modal) sin `Turquesa`/`Cian`.
