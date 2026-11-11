# Dirección de Arte Global — Clásicos Mente Activa 2D

> **Documento SDD maestro (global)** de UI/UX, componentes base, tipografía, paleta y herencia visual.  
> **Versión:** 1.0 · **Estado:** Vigente · **Última actualización:** 2026-11-10  
> **Alcance:** normas universales de la aplicación (estándares UI globales, componentes base universales, **matriz inmutable de paleta global** y **Master Index** de direcciones de arte por minijuego). Las métricas, layouts y comportamientos específicos de cada minijuego se definen en su documento de dirección de arte y **heredan** de este maestro.

---

## 1. Propósito, alcance y autoridad

- Este documento es la **fuente de verdad global** para: reglas de presentación retro/pixel, accesibilidad visual base, componentes universales, **tokens tipográficos**, **matriz inmutable de paleta global** y política de **herencia visual**.
- **No** define métricas de grilla, layouts de escena, reglas de puntuación ni comportamientos exclusivos de un minijuego (p. ej. tablero, zoom, footer o partículas concretas). Esos detalles viven en los documentos indexados en la [§8 Master Index](#8-master-index--direcciones-de-arte-por-minijuego).
- En caso de conflicto, la precedencia es: **Matriz de paleta global (§4) y reglas de herencia (§7)** > este documento global > dirección de arte del minijuego > implementación puntual. Las desviaciones temporales deben documentarse explícitamente en el minijuego y resolverse sin redefinir tokens globales.

---

## 2. Reglas globales de pantalla y presentación (pixel art)

- **Renderizado nítido:** sprites y UI con `Texture Filter = Nearest`, sin suavizado de bordes (*anti-aliasing*) ni mipmaps donde aplique. Para `StyleBoxFlat`/bordes, usar `AntiAliasing = false` salvo justificación documentada.
- **Estilo retro consistente:** preferir geometría y sombreado “duros” (bordes/sombras en píxeles enteros, sin transparencias que generen efecto fantasma en estados activos/encontrados). Los estados críticos deben distinguirse por **forma/borde/icono además de color**.
- **Fondo base de aplicación:** claro y adaptable por tema (token `BackgroundColor`, por defecto `#FFFFFF`).
- **Safe Area / notch:** los layouts deben respetar el área segura del dispositivo. Patrón base recomendado: contenedor con márgenes seguros (p. ej. `MarginContainer` con top/bottom/left/right acordes al dispositivo; referencia histórica 48/16/16/16) y/o gestor de safe area (`core/SafeAreaManager.cs`) aplicando `max(mínimo configurado, inset del display)`.
- **Accesibilidad base:** objetivos táctiles mínimos **≥ 56×56 px lógicos** para controles principales; contraste suficiente (WCAG: texto ≥ 4.5:1, indicadores/bordes ≥ 3:1 cuando aplique); no depender solo del color para comunicar estado.

> **Nota de evolución:** si un minijuego requiere radios de esquina o relieve específicos para alinearse a una referencia visual, debe declararlo como **override métrico/estilístico de alcance local** en su propia dirección de arte, manteniendo intactos los valores de la paleta global y la legibilidad/accesibilidad.

---

## 3. Sistema de temas y aplicación de tokens

- **`ThemeData` (`core/ThemeData.cs`)** define tokens de color consumibles por UI, celdas, banners y paneles. **`ThemeManager` (`core/ThemeManager.cs`)** es el punto central (autoload/singleton) que expone el tema activo y notifica cambios (`OnThemeChanged` / señales) para que los componentes actualicen estilos en tiempo real.
- **`PixelStyles` (`core/ui/PixelStyles.cs`)** centraliza fábrica/utilidades de `StyleBoxFlat` y helpers de contraste para la línea visual pixel/retro del proyecto.
- Los temas alternativos pueden **mapear** los tokens globales a variantes (p. ej. alto contraste/oscuro), pero **no deben contradecir el significado semántico** de los tokens (idle, found, hint, texto, etc.) ni redefinir la matriz canónica base fuera de un contrato de tema documentado.

---

## 4. Matriz inmutable de paleta global (autoridad canónica)

**Inmutable:** los nombres, roles y valores hex de esta matriz son la referencia global. Los minijuegos **referencian** estos tokens; no los redefinen.

| Token | Hex | Rol / uso | Fuente canónica |
|---|---|---|---|
| `BackgroundColor` / `Fondo` | `#FFFFFF` | Fondo principal de app/escenas (base clara) | `ThemeData`, `PixelStyles` |
| `HeaderBannerColor` / `PlacaHeader` | `#A0BFC7` | Banner/cabecera, acentos de placa | `ThemeData`, `PixelStyles` |
| `SubheaderBannerColor` | `#477987` | Sub-banners, texto secundario/footer en tema | `ThemeData` |
| `CellIdleBackgroundColor` | `#EBF2F5` | Fondo de celda/grilla en reposo | `ThemeData` |
| `CellIdleBorderColor` / `Arena` | `#A0BFC7` / `#B0C2CC` | Borde fino de celda/reposo y superficies suaves (*nota: variantes cercanas; usar el token del contexto UI/tema*) | `ThemeData`, `PixelStyles` |
| `CellIdleTextColor` / `TextDark` | `#101010` | Texto oscuro base sobre claros / celda idle | `ThemeData`, `PixelStyles` |
| `CellFoundBackgroundColor` / `Turquesa` | `#099C92` | Estado encontrado/selección-acierto (acento principal) | `ThemeData`, `PixelStyles` |
| `CellFoundTextColor` / `TextLight` | `#FFFFFF` | Texto sobre acento/encontrado (alto contraste) | `ThemeData`, `PixelStyles` |
| `FooterTextColor` | `#477987` | Texto del panel/lista inferior | `ThemeData` |
| `FooterStrikethroughColor` | `#099C92` | Tachado/marca de palabra encontrada en footer | `ThemeData` |
| `Tinta` | `#4E6077` | Texto/bordes principales sobre superficies claras | `PixelStyles` |
| `Borde` | `#4E6077` | Borde visible de placas/celdas/botones | `PixelStyles` |
| `Sombra` | `#8A9AAA` | Sombra dura decorativa (relieve) | `PixelStyles` |
| `Celda` | `#FFFCFB` | Relleno claro de celdas/paneles claros | `PixelStyles` |
| `Placa` | `#EFEFEE` | Interior de placas/modales | `PixelStyles` |
| `CeldaSeleccion` | `#6095FB` | Selección/trazo en curso (según componente) | `PixelStyles` |
| `Trazo` | `#2B56B8` | Línea/indicador de trazo | `PixelStyles` |
| `EncontradaBorde` | `#06706A` | Borde de estado encontrado (contraste reforzado) | `PixelStyles` |
| `PistaAcento` | `#F9881E` | Resaltado de pista/ayuda | `PixelStyles` |
| `PistaBorde` | `#8A4600` | Borde de pista/ayuda | `PixelStyles` |
| `Amarillo` | `#FDC109` | Acento secundario | `PixelStyles` |
| `Cian` | `#08D0DF` | Acento secundario | `PixelStyles` |
| `CelebrationRed` | `#B45252` | Color cálido de confeti/celebración (coral). Usado en el estallido de partículas de victoria de Word Search (§8). **Agregado v0.5.0** como extensión aditiva; no altera ningún token existente. | `ThemeData`, `PixelStyles`, `PixelConfettiEmitter` |

> **Deriva conocida:** existen diferencias históricas entre valores de tema y utilidades de estilo (p. ej. bordes de celda idle). Al implementar o refinar, consolidar contra la fuente canónica indicado y actualizar esta matriz si se aprueba un cambio global; los minijuegos deben seguir los **tokens**, no valores ad hoc.

> **Additive extensions (v0.5.0, MINOR):** `CelebrationRed` (#B45252) se incorporó como token de celebración para completar la paleta de partículas de Word Search. Es una **extensión aditiva**: no redefine, renombra ni sustituye ningún token de la matriz canónica. Los minijuegos que la usan deben documentarlo en su dirección de arte local (ver [`word_search/art_direction.md`](word_search/art_direction.md) §8).

---

## 5. Componentes base universales

Componentes reutilizables (rutas orientativas en `res://scenes/ui/components/` y equivalentes en código):

| Componente | Propósito | Guía visual global |
|---|---|---|
| **Botón (pixel/BigButton)** | Acción táctil principal/secundaria | ≥ 56×56 px, estados normal/hover/pressed/disabled/focus claros, relieve/press “pixel” consistente; foco visible por forma/borde |
| **Banner/Placa (`PixelBanner`, placas)** | Títulos, cabeceras, tarjetas | Superficies con borde/relieve legible usando tokens de placa/borde/sombra |
| **Celda (`PixelCell`)** | Unidad de grilla interactiva | Estados idle/selected/found/hint derivados de tokens; legibilidad y contraste prioritarios |
| **HUD/Header (`PixelHeaderHUD`)** | Navegación, estado, progreso, tiempo | Jerarquía clara, controles accesibles, sin saturar el área segura |
| **Footer/Panel (`PixelFooterPanel`)** | Listas/objetivos/palabras | Contenido legible, separadores claros, respeto de safe area inferior |
| **Modales (backdrop + card)** | Pausa, confirmación, victoria | Bloqueo de fondo sólido, tarjeta legible, acciones con targets accesibles, foco contenido |

Los minijuegos pueden **componer** estos patrones y ajustar métricas locales, pero deben respetar la semántica de estados y la paleta/tipografía heredadas.

---

## 6. Reglas tipográficas globales (tokens de rol)

| Rol / token | Fuente principal | Uso |
|---|---|---|
| **Display / Título** | `PressStart2P.ttf` | Títulos, banners principales, modales destacados |
| **HUD / Mono / UI técnica** | `VCR_OSD_Mono.ttf` | Tiempo, contadores, botones de interfaz, mensajes estilo retro |
| **Contenido de grilla / listado de juego** | `PixelOperator.ttf` (u operador mono equivalente aprobado) | Letras de celdas, matrices, listas/palabras en juego |
| **UI accesible / lectura prolongada** | `AtkinsonHyperlegible-Regular.ttf` / Bold | Menús, ajustes, textos de ayuda o donde la legibilidad prime |

**Reglas:** usar `Nearest` para fuentes pixel; mantener jerarquía y tamaños legibles por dispositivo; cualquier tratamiento de peso (bold/faux-bold) debe preservar nitidez y documentarse en el minijuego si es específico.

---

## 7. Regla de herencia visual (obligatoria)

1. **Heredan de este global (inmutable por minijuego):** **colores base y valores de la matriz de paleta global (§4)**, semántica de estados de color, **estilos/relieve base de botones y componentes**, y **tokens tipográficos y sus roles (§6)** (fuentes heredadas). Ningún minijuego redefine estos tokens globales.
2. **Puede definir cada minijuego:** layout, métricas, espaciados, tamaños de grilla/celdas, comportamiento de controles (p. ej. zoom), contenido de HUD/footer, animaciones/partículas y copy visual, siempre dentro de las reglas globales de accesibilidad y presentación.
3. **Overrides permitidos (acotados):** ajustes métricos o de composición necesarios para fidelidad a la referencia del minijuego (p. ej. radio/relieve local, alturas de panel), **sin** sustituir ni redefinir tokens globales. Toda excepción debe indicarse en el documento del minijuego con justificación y verificación visual.

---

## 8. Master Index — Direcciones de arte por minijuego

| Minijuego | Documento | Alcance |
|---|---|---|
| **Sopa de Letras (Word Search)** | [`word_search/art_direction.md`](word_search/art_direction.md) | Layout y HUD del juego, reglas/métricas de grilla, zoom, footer/WordsPanel, confeti/celebración; hereda paleta, tipografía y relieve globales. |
| *(Futuros minijuegos)* | `docs/<minijuego>/art_direction.md` | Añadir una fila por minijuego siguiendo la misma convención de herencia. |

---

## 9. Micro-animaciones y feedback global (“game juice”)

Patrones reutilizables (la implementación concreta puede variar por minijuego):
- **Selección/press:** respuesta inmediata (escala/tween sutil o desplazamiento de 1–2 px) que refuerce el estado sin perder claridad.
- **Acierto/confirmación:** realce breve con tokens de acento/encontrado y feedback en lista/objetivo cuando aplique.
- **Transiciones/modales:** animaciones cortas y legibles; evitar bloqueos prolongados de interacción.

Los efectos específicos (p. ej. confeti de victoria de Word Search) se especifican en la dirección de arte del minijuego.

---

## 10. Referencias de implementación

- `core/ThemeData.cs`, `core/ThemeManager.cs` — tokens y propagación de tema
- `core/ui/PixelStyles.cs` — estilos/utilidades visuales
- `core/SafeAreaManager.cs`, `core/AccessibleTheme.tres` — safe area y tema accesible
- `docs/word_search/art_direction.md` — ejemplo de dirección específica heredada
