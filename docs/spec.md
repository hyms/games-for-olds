# Especificación General — Clásicos Mente Activa 2D (v0.3)

> **Documento SDD global del producto.**
> Define la visión, los objetivos, la arquitectura, la accesibilidad, las
> estadísticas y la publicidad transversal. Las reglas de cada juego viven en su
> propio `spec.md`.
>
> **Versión:** 0.3 · **Fecha:** 26/10/2026 · **Estado:** MVP en definición
> **Documentación relacionada:** [Plan global](plan.md) · [Pruebas globales](test.md) ·
> [Sopa de Letras](word_search/spec.md)

---

## 1. Visión del producto

**Clásicos Mente Activa 2D** es una colección de minijuegos clásicos de agilidad
mental para adultos de 40 años en adelante, con foco prioritario en personas de
60+ y en quienes tienen baja familiaridad con la tecnología.

**Propuesta de valor:** juegos conocidos, legibles, sin frustración y sin castigos
por tiempo. El jugador debe poder abrir la app, ajustar el tamaño del texto si lo
necesita, elegir un juego y empezar en menos de 30 segundos, sin registro y sin
conexión obligatoria.

### Principios del producto

1. **Accesibilidad primero:** texto escalable, objetivos táctiles de al menos
   56×56 px, alto contraste, estados que nunca dependen solo del color y ausencia
   de gestos finos obligatorios.
2. **Sin presión:** cero cronómetros punitivos y cero derrotas agresivas. El
   tiempo se mide para estadísticas, no para castigar.
3. **Publicidad no invasiva:** la publicidad entra al final del proyecto y solo
   como banner en la parte superior del menú principal y de la pantalla de
   estadísticas. Nunca dentro de una partida.
4. **KISS:** navegación plana, pocas opciones por pantalla y lenguaje cotidiano.
5. **DRY y orientado a datos:** temáticas, niveles, dificultades y temas de color
   son recursos editables; la lógica no contiene contenido hardcodeado.
6. **Un solo punto de entrada global:** `SceneManager` es el único autoload. Los
   servicios compartidos viven en `core/` y son creados por él.

---

## 2. Objetivos generales y métricas

### 2.1 Objetivos específicos

| # | Objetivo | Criterio de logro |
|---|----------|-------------------|
| O1 | Autonomía desde el primer uso | Un adulto 40+ **inicia** una partida sin ayuda externa en ≤ 30 s |
| O2 | Accesibilidad real | Texto escalable en 3 tamaños (solo tipografía), objetivos táctiles ≥ 56×56 px, contraste alto y estados no solo por color |
| O3 | Progreso visible | Pantalla global de estadísticas por juego con nivel actual, mejor tiempo del nivel actual y mejor puntaje |
| O4 | Rendimiento | 60 fps estables en Android de gama baja; arranque < 3 s; memoria < 150 MB; carga de nivel < 0,5 s |

### 2.2 Métricas de éxito

- ≥ 80 % de los usuarios de prueba terminan una partida en la primera sesión.
- ≤ 10 % abandona en los primeros 60 segundos.
- El jugador entiende la mecánica sin leer instrucciones largas.
- Comentarios cualitativos: "se ve bien", "se entiende", "no me apura".

---

## 3. Público objetivo

| Perfil | Características | Necesidad principal |
|--------|-----------------|---------------------|
| Adulto 40–59 | Juega ocasionalmente en el móvil | Rapidez y claridad |
| Adulto 60+ (prioritario) | Vista cansada, motricidad fina reducida, poca familiaridad con gestos | Letra grande, toques simples, tolerancia al error |
| Cuidador o familiar | Instala y configura la app para otra persona | Sin compras, sin registros y publicidad discreta |

Necesidades transversales: ver bien, tocar fácil, entender sin jerga y recordar
pocas opciones por pantalla.

---

## 4. Alcance del MVP

### 4.1 Juegos

| Juego | Estado | Especificación |
|-------|--------|----------------|
| Sopa de Letras | Base funcional; en cierre de jugabilidad | [Ver spec](word_search/spec.md) |
| Tangram / Puzles de encaje | Pendiente | Se creará con su implementación |
| Lógica de Bloques (estilo *Dr. Mario*) | Pendiente | Se creará con su implementación |

### 4.2 Fuera de alcance del MVP

- Multijugador, ranking online o cuentas de usuario.
- Compras, suscripciones y anuncios bonificados (Rewarded Ads).
- Anuncios dentro de las partidas.
- iOS (se evalúa después).
- Traducciones a otros idiomas.
- Reconocimiento de voz o gestos complejos.
- Juegos fuera de los tres definidos.

---

## 5. Experiencia de usuario y navegación

### 5.1 Menú principal

- Título de la app.
- Tres botones grandes: **Sopa de Letras**, **Tangram**, **Lógica de Bloques**.
- Botones **Estadísticas**, **Ajustes** y **Salir**.
- Banner publicitario en la parte superior cuando la publicidad esté activa
  (Fase 8); espacio reservado para que no salte el layout.
- Fondo animado pixel art con opción de movimiento reducido.

### 5.2 Pantalla de estadísticas

- Accesible desde el menú principal.
- Una tarjeta por juego con:
  - **Nivel actual**.
  - **Mejor tiempo** para completar el nivel actual.
  - **Mejor puntaje** del nivel actual.
- Botón **Restablecer estadísticas** con confirmación.
- Banner publicitario en la parte superior cuando la publicidad esté activa.
- Nunca muestra anuncios durante una partida.

### 5.3 Ajustes

Accesibles desde el menú y desde la pausa de cada juego:

1. **Audio y música:** on/off y volumen general.
2. **Tamaño del texto:** Normal / Grande / Muy grande. **Solo cambia la
   tipografía**; no cambia la resolución ni reduce los objetivos táctiles.
3. **Tema de color:** Papel Cálido, Oscuro Alto Contraste, Azul Calma.
4. **Movimiento reducido:** desactiva animaciones de fondo.
5. **Estadísticas:** acceso y reinicio con confirmación.

Cada cambio se guarda de inmediato.

### 5.4 Pantalla de partida

- Botón **Volver** siempre visible.
- Botón **Reiniciar**.
- Ayudas propias del juego (si existen), definidas en su `spec.md`.
- Estado visible: nivel, progreso y objetivo.
- Sin anuncios y sin cronómetros punitivos.

### 5.5 Fin de partida

- Mensaje claro de victoria o de finalización.
- Tiempo y puntaje de la partida.
- Opciones **Repetir** y **Volver**.
- El resultado se registra en estadísticas.

### 5.6 Reglas de navegación

- Volver al menú principal en un máximo de dos toques.
- Cambiar de juego no requiere reiniciar la app.
- Ninguna pantalla queda bloqueada sin salida visible.
- Un único mecanismo de navegación: `SceneManager`.

---

## 6. Accesibilidad transversal

1. **Texto escalable:** tres tamaños configurables. Normal usa 20 px como base;
   Grande 24 px; Muy grande 28 px. Solo se modifica la tipografía. Los controles
   pueden crecer en altura para que el texto no se corte, pero nunca por debajo
   de 56×56 px.
2. **Objetivos táctiles:** mínimo **56×56 px** en todos los modos. No se exige
   un tamaño distinto al agrandar el texto.
3. **Contraste:** ≥ 4,5:1 para texto y ≥ 3:1 para bordes, iconos y estados.
4. **Nunca solo color:** los estados se comunican también con forma, borde, icono
   o texto.
5. **Feedback inmediato:** respuesta visual en < 0,2 s.
6. **Sin presión de tiempo:** el tiempo se registra para estadísticas, no se
   muestra como cuenta atrás ni provoca derrota.
7. **Tolerancia al error:** selección por toques, deshacer donde aplique y
   feedback suave ante una acción inválida.
8. **Sin gestos finos obligatorios:** toda acción tiene alternativa táctil simple.
   Excepción cerrada para la Sopa de Letras: la selección es solo por arrastre
   (decisión C4), compensada con celdas grandes, bloqueo de dirección y
   tolerancia; se valida con el perfil 60+ antes de su cierre.
9. **Sin desbordes:** los textos largos se ajustan o envuelven; nada se corta en
   360×640, 20:9 o tablet.
10. **Movimiento reducido:** opción para desactivar el fondo animado.
11. **Idioma:** español neutro, con tuteo y vocabulario cotidiano.

---

## 7. Plataforma y requisitos técnicos

- **Plataforma principal:** Android móvil, orientación vertical.
- **Resolución base:** 360×640 lógicos, estirado `canvas_items`, adaptable a
  pantallas 20:9 y tablets.
- **Modo de texto:** Normal / Grande / Muy grande, sin cambiar el viewport.
- **Estilo visual:** pixel art limpio, colores saturados pero no chillones,
  fondos oscuros con elementos claros.
- **Funcionamiento offline:** todos los contenidos incluidos en la app.
- **Persistencia:** preferencias, progreso y estadísticas en local.
- **Motor:** Godot 4.7 **.NET** con **C# 12** (`net8.0`; `net9.0` al exportar a
  Android).
- **Pruebas:** **GdUnit4Net** (`dotnet test`), con ejecución headless para CI.
- **Rendimiento:** 60 fps; arranque < 3 s; memoria < 150 MB; carga de nivel
  < 0,5 s.

---

## 8. Arquitectura de software

El proyecto usa **alta cohesión interna y bajo acoplamiento entre módulos**. La
estructura de carpetas sigue la recomendación del skill del proyecto
(`godot4-mastery`) y la organización real del repositorio:

```text
res://
├── ClasicosMenteActiva2D.csproj  # Proyecto C# (Godot.NET.Sdk 4.7.2)
├── autoloads/
│   └── SceneManager.cs          # ÚNICO autoload: navegación y servicios
├── core/
│   ├── save_manager.cs          # Persistencia JSON
│   ├── theme_manager.cs         # Temas y tamaño de texto
│   ├── audio_manager.cs         # Buses BGM/SFX
│   ├── ad_manager.cs            # Banners (no-op hasta Fase 8)
│   ├── base_minigame.cs         # Base común de minijuegos
│   ├── Constants.cs
│   ├── Utils.cs
│   └── AccessibleTheme.tres
├── ui/
│   ├── main_menu/
│   ├── difficulty_selector/      # Selector de dificultad (antes de la partida)
│   ├── components/
│   ├── settings/
│   └── stats/
├── minigames/
│   └── word_search/             # Un directorio por juego
├── resources/
│   ├── difficulties/            # Los 4 niveles (DifficultyConfig .tres)
│   └── themes/                  # Temáticas y datos editables
├── assets/
│   └── fonts/
├── tests/                       # Pruebas GdUnit4Net
└── docs/                        # Documentación SDD (globales y por juego)
```

### 8.1 Un solo autoload

- `SceneManager` es el **único** autoload del proyecto.
- Centraliza la navegación y expone los servicios compartidos:
  `save`, `theme`, `audio` y `ads`.
- Los servicios son clases de `core/` que `SceneManager` instancia. Así las
  pruebas pueden crear instancias aisladas sin depender del árbol de escenas.
- No existe un `GameManager` separado: la sesión de cada partida vive en su
  minijuego y el resultado se envía a `SaveManager`.

### 8.2 `BaseMinigame` (Template Method)

- Escena/clase base con encabezado, pausa, final de partida, acceso a ajustes y
  gancho de estadísticas.
- Espacio **opcional** para las ayudas propias del juego.
- No incluye anuncios: la publicidad solo vive en el menú principal y en
  estadísticas.
- No impone número de pistas ni dificultades; cada juego lo define.

### 8.3 Comunicación

- **Señales** para eventos hijo → padre (celda → tablero → HUD).
- **Parámetros** para datos padre → hijo (`start_with_theme`, `start_level`).
- Señales globales solo desde `SceneManager` para cambios de escena.
- Sin referencias cruzadas entre minijuegos.

### 8.4 Contratos de servicios

| Servicio | API mínima | Señales |
|----------|------------|---------|
| `SceneManager` | `go_to_main_menu()`, `go_to_stats()`, `go_to_settings()`, `go_to_game(game_id)`; propiedades `save`, `theme`, `audio`, `ads` | `scene_changing(path)`, `scene_changed(path)` |
| `SaveManager` | `load_data()`, `save_data()`, `get_setting(key, default)`, `set_setting(key, value)`, `record_result(game_id, level, time_seconds, score, completed)`, `get_stats(game_id)`, `reset_stats()` | `data_changed` |
| `ThemeManager` | `set_theme(theme_id)`, `set_text_size(size_id)` | `theme_changed(theme)`, `text_size_changed(size_id)` |
| `AudioManager` | `set_bgm_volume(value)`, `set_sfx_volume(value)`, `set_muted(value)`, `play_sfx(name)` | `audio_settings_changed` |
| `AdManager` | `initialize()`, `show_banner(placement)`, `hide_banner()` | `banner_loaded(placement)`, `banner_failed(placement, reason)` |

### 8.5 Datos como recursos

- Temáticas, dificultades y configuraciones son `Resource` con `class_name` y
  `@export`, editables en el Inspector.
- Los textos de UI se centralizan para facilitar futuras traducciones.
- El contenido no se incrusta en scripts.

---

## 9. Publicidad

- **Momento:** entra al final del proyecto (Fase 8). Hasta entonces `AdManager`
  es un proveedor no-op y la UI no muestra anuncios.
- **Formato:** solo **banners**. No hay anuncios bonificados en el MVP.
- **Ubicaciones permitidas:**
  - Parte superior del menú principal.
  - Parte superior de la pantalla de estadísticas.
- **Ubicaciones prohibidas:** partidas, selectores de juego, ajustes y pantallas
  de victoria.
- **Layout:** el espacio del banner se reserva; al ocultarse (offline o fallo) la
  interfaz se reacomoda sin saltos bruscos ni controles tapados.
- **Offline:** el banner se oculta y el juego funciona igual.
- **Editor/escritorio:** proveedor no-op; nunca anuncios reales en desarrollo.
- **Privacidad:** consentimiento (UMP/GDPR) antes de solicitar anuncios y
  política de privacidad publicada.

---

## 10. Persistencia y estadísticas

### 10.1 Archivo local

`SaveManager` usa `user://save_data.json` con escritura segura (archivo temporal
y reemplazo) y respaldo ante corrupción.

```json
{
  "version": 1,
  "settings": {
    "bgm_volume": 0.8,
    "sfx_volume": 1.0,
    "text_size": "normal",
    "theme_id": "warm_paper",
    "reduced_motion": false
  },
  "stats": {
    "word_search": {
      "current_level": 1,
      "levels": {
        "1": { "best_time_seconds": 0, "best_score": 0, "plays": 0, "wins": 0 },
        "2": { "best_time_seconds": 0, "best_score": 0, "plays": 0, "wins": 0 },
        "3": { "best_time_seconds": 0, "best_score": 0, "plays": 0, "wins": 0 }
      }
    },
    "tangram": {
      "current_level": 1,
      "levels": {
        "1": { "best_time_seconds": 0, "best_score": 0, "plays": 0, "wins": 0 }
      }
    },
    "block_logic": {
      "current_level": 1,
      "levels": {
        "1": { "best_time_seconds": 0, "best_score": 0, "plays": 0, "wins": 0 }
      }
    }
  }
}
```

### 10.2 Semántica

- `current_level`: nivel en el que está el jugador en ese juego. Avanza al
  completar el nivel actual; si no hay más niveles, se queda en el último.
- `best_time_seconds`: menor tiempo para completar **el nivel actual**.
- `best_score`: mayor puntaje obtenido en **el nivel actual**.
- `plays` y `wins`: partidas jugadas y completadas en ese nivel.
- El tiempo se mide en segundo plano; la Sopa lo muestra transcurrido y nunca
  como cuenta atrás.
- El puntaje lo define cada juego en su `spec.md`; el global solo lo almacena.
- Las dificultades también las define cada juego; el global no impone nombres ni
  cantidades.
- Reiniciar estadísticas exige confirmación y restaura todos los juegos.
- `version` permite migrar el esquema sin perder datos.

---

## 11. Los minijuegos

### 11.1 Sopa de Letras

Buscar palabras en un tablero de 8×8, 10×10, 12×12 o 15×15 según el nivel, con
diez categorías, cuatro dificultades propias (Fácil, Medio, Difícil y Maestro),
selección por arrastre, 3 pistas por nivel sin anuncios y desbloqueo del nivel
siguiente tras 20 partidas ganadas. Especificación completa en
[docs/word_search/spec.md](word_search/spec.md).

### 11.2 Tangram / Puzles de encaje

Completar siluetas con siete piezas, sin tiempo y con rotación por botones. Sus
niveles, dificultades, ayudas y puntaje se definirán en su propio `spec.md`.

### 11.3 Lógica de Bloques

Cápsulas de dos colores que caen y se eliminan al alinear cuatro o más. Modo
relajado por defecto, pausa accesible y colores distinguibles por forma o
símbolo. Sus niveles, dificultades, ayudas y puntaje se definirán en su propio
`spec.md`.

---

## 12. Requisitos transversales

### 12.1 Audio

- Música de fondo suave y opcional.
- Efectos para aciertos, errores, ayudas y victoria.
- Control de volumen y silencio total desde ajustes.
- Sin sonidos bruscos ni alarmas.

### 12.2 Ajustes

- Audio y música.
- Tamaño de texto (solo tipografía).
- Tema de color.
- Movimiento reducido.
- Estadísticas y reinicio.

### 12.3 Tutorial y ayuda

- Explicación breve la primera vez que se entra a cada juego.
- Recordatorio accesible desde un botón de ayuda.
- Sin textos largos: frases cortas y ejemplos visuales.

### 12.4 Localización

- Español como único idioma del MVP.
- Textos centralizados para futuras traducciones.

### 12.5 Privacidad

- Sin cuentas, sin nube y sin datos personales.
- La publicidad, cuando se active, cumple consentimiento y política de
  privacidad.
- Funcionamiento completo en modo avión.

---

## 13. Criterios de aceptación del MVP

1. Los tres juegos son accesibles desde el menú y se pueden completar.
2. Cada juego cumple su `spec.md` y sus criterios de aceptación.
3. No existen bloqueos, pantallas sin salida ni errores que impidan jugar.
4. Los objetivos táctiles principales cumplen 56×56 px en todos los tamaños de
   texto.
5. El texto escalable no se corta ni desborda en 360×640, 20:9 ni tablet.
6. La app funciona sin conexión y sin permisos innecesarios.
7. Las estadísticas muestran nivel actual, mejor tiempo y mejor puntaje por
   juego, y se conservan al reiniciar la app.
8. La publicidad, cuando se active, aparece solo en menú principal y estadísticas
   y nunca tapa controles.
9. El APK se instala y ejecuta en un Android de gama baja con rendimiento
   aceptable.
10. Un usuario del perfil objetivo completa una partida sin ayuda.

---

## 14. Organización de la documentación (SDD)

| Documento | Ubicación | Contenido |
|-----------|-----------|-----------|
| Spec global | `docs/spec.md` | Visión, objetivos y requisitos transversales |
| Plan global | `docs/plan.md` | Fases, decisiones y checklist |
| Test global | `docs/test.md` | Estrategia y pruebas transversales |
| Spec de juego | `docs/<juego>/spec.md` | Reglas y objetivos del juego |
| Plan de juego | `docs/<juego>/plan.md` | Checklist del juego |
| Test de juego | `docs/<juego>/test.md` | Casos de prueba del juego |
| Dirección de arte | `docs/art_direction.md` | Estilo visual, paleta, geometría e importación |
| Manifiesto de assets | `assets/asset-manifest.json` | Lista de assets, estado, origen y licencia |
| Plantilla visual | `docs/<juego>/` | Plantillas visuales aprobadas (no se exportan) |

Regla: los documentos globales resumen y enlazan; no duplican las reglas de cada
juego.

---

## 15. Glosario

- **MVP:** versión mínima jugable con los tres juegos.
- **SDD:** desarrollo guiado por especificación (spec → plan → pruebas).
- **Minijuego:** cada uno de los juegos independientes de la colección.
- **Nivel:** dificultad o etapa propia de cada juego.
- **Pista:** ayuda propia de cada juego; no es un contador global.
- **Puntaje:** valor calculado al terminar, con la fórmula de cada juego.
- **Banner:** anuncio rectangular no interactivo, solo en menú y estadísticas.
- **Modo relajado:** configuración sin presión de tiempo ni aceleración agresiva.
