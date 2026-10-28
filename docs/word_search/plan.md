# Plan de Trabajo — Sopa de Letras (v0.4)

> **Documento SDD** del minijuego *Sopa de Letras*.
> Checklist de construcción alineado con la nueva [spec](spec.md) y el nuevo
> [plan de pruebas](test.md).
>
> **Versión:** 0.4 · **Fecha:** 27/10/2026 · **Estado:** Replaneación con decisiones cerradas
> **Documentación relacionada:** [Spec global](../spec.md) ·
> [Plan global](../plan.md) · [Pruebas globales](../test.md) ·
> [Spec del juego](spec.md) · [Pruebas del juego](test.md) ·
> [Dirección de arte](../art_direction.md) ·
> [Manifiesto de assets](../../assets/asset-manifest.json)
>
> Ubicación del código: `minigames/word_search/` · Documentación: `docs/word_search/`.
> Plantilla visual aprobada: `assets/reference/wathersoup.jpg`.

## Leyenda

- `[x]` Hecho y verificado en Godot
- `[ ]` Pendiente
- 🟡 Parcial o pendiente de verificación
- 🔜 En curso (siguiente tarea a atacar)
- **P1** Bloqueante · **P2** Importante · **P3** Mejora

---

## 1. Estado real y brecha con la nueva especificación

| Área | Hoy en el repositorio | Objetivo de la nueva spec | Acción |
|------|-----------------------|---------------------------|--------|
| Categorías | 5 temáticas | 10 categorías, 50–100 palabras cada una (≥ 600) | Ampliar y migrar |
| Dificultades | 3 niveles (8/10/10) | 4 niveles (8/10/12/15) con reglas propias | Rediseñar como datos |
| Generador | `WordGenerator` con `WordSearchTheme` | `WordSearchGenerator` + `DifficultyConfig` (Strategy) | Renombrar y separar |
| Tablero | `WordGrid` con celdas fijas de 48 px | `WordGridContainer` con encaje y zoom | Renombrar y rehacer layout |
| Selección | Toque inicial + final (rota) | Arrastre único con trazo en tiempo real | Reimplementar |
| Pistas | 3 por partida, sin anuncios | 3 por nivel, sin anuncios (C2) | Reimplementar |
| Progresión | Sin desbloqueo | Submenú con candados y 20 partidas por nivel | Implementar |
| Victoria | Sin modal | Modal con confeti pixel art y acciones | Implementar |
| HUD | Encabezado simple | Pausa, contador y tiempo no punitivo | Implementar |
| Lista de palabras | No visible | Footer aleatorio que reordena al encontrar | Implementar |
| Puntaje | Fórmula definida (spec anterior) | Fórmula nueva definida en este plan (C7) | Implementar |

> **Selección actual:** el código vigente usa toque inicial/final y además está
> roto (`_end_selection()` ignora la última celda). La nueva spec pide arrastre,
> así que ese código se reemplaza, no se repara.

---

## 2. Decisiones de la replaneación (C1–C8)

Estas decisiones se tomaron el 27/10/2026 y sustituyen a la propuesta anterior
de cada punto. Las que afectan al SDD global se reflejan en `docs/` al cerrar la
Fase 9.

| # | Tema | Decisión | Efecto |
|---|------|----------|--------|
| C1 | Ruta | Se mantiene `minigames/word_search/` (D1). | Corregir en `spec.md` la línea que dice `scenes/minigames/word_search/`. |
| C2 | Anuncios | **Sin anuncios en la Sopa.** 3 pistas fijas por nivel, sin recarga publicitaria. | Se retira `ST-WS-03` (Rewarded) de `test.md`; D5 no cambia. |
| C3 | Dificultades | Se adoptan los **4 niveles** de la nueva spec (8×8/5, 10×10/8, 12×12/12, 15×15/20). | Actualizar D6 y el resumen de la Sopa en `docs/spec.md`. |
| C4 | Interacción | **Solo arrastre.** Sin toque inicial/final. | Excepción documentada al principio “sin gestos finos”: el arrastre se compensa con celdas grandes, bloqueo de dirección y tolerancia. Actualizar `docs/spec.md` §6.8. |
| C5 | Resolución | Viewport 360×640 `canvas_items`; 375×812 solo como equipo de prueba. | El tablero se adapta; no se cambia `project.godot`. |
| C6 | Desbloqueo | **20 partidas por nivel** para desbloquear el siguiente. | Fácil abierto; Medio tras 20 de Fácil; Difícil tras 20 de Medio; Maestro tras 20 de Difícil. |
| C7 | Puntaje | **Fórmula nueva** definida en la Fase 8. | Se mantiene `best_score` en estadísticas. |
| C8 | Zoom | Pinch de 2 dedos **más botones + / − y Centrar**. | El zoom nunca es la única forma de ampliar. |

**Regla:** ninguna decisión de la spec se implementa si contradice esta tabla.
Si cambia una decisión, se actualiza primero aquí y luego el código.

---

## 3. Orden de dependencias

```text
Fase 0    Base técnica (hecha)
Fase 0.5  GdUnit4 + primer test puro
Fase 0.6  Dirección de arte y componentes visuales
Fase 1    Banco de palabras y 10 categorías
Fase 2    DifficultyConfig + WordSearchGenerator (Strategy)
Fase 3    Selección por arrastre (WordGridContainer)
Fase 4    Encaje, zoom y layout del tablero
Fase 5    HUD, pausa, footer de palabras
Fase 6    Pistas (3 fijas, sin anuncios)
Fase 7    Progresión y desbloqueo (20 partidas)
Fase 8    Victoria, puntaje y estadísticas
Fase 9    Integración, accesibilidad, rendimiento y entrega
```

La Fase 2 no empieza sin la Fase 1: sin banco de palabras no se pueden validar
dimensiones ni tolerancia.

---

## Fase 0 — Base técnica compartida

- [x] Estructura actual del proyecto (`autoloads/`, `core/`, `ui/`, `minigames/`, `resources/`, `tests/`).
- [x] Proyecto en vertical 360×640, estirado `canvas_items`.
- [x] Emulación de toque → ratón para escritorio.
- [x] Autoload único `SceneManager`.
- [x] Tema accesible compartido y `BigButton` (56×56).
- [x] Entrada desde el menú principal.
- [x] Documentación SDD del minijuego.

---

## Fase 0.5 — Infraestructura de pruebas (P1)

- [x] Instalar GdUnit4 v6.2.2 en `addons/gdUnit4` y activar el plugin.
- [x] Documentar la ejecución headless:
      `./addons/gdUnit4/runtest.sh --godot_binary <ruta_a_godot> -a res://tests`
      o, sin el script, `godot --headless -s addons/gdUnit4/bin/GdUnitCmdTool.gd -a res://tests`
      (ver `tests/README.md`).
- [x] Crear `tests/unit/` y `tests/integration/` con convención `*Test.gd`.
- [x] Primer test puro de `WordSearchTheme` (`tests/unit/WordSearchThemeTest.gd`):
      mayúsculas, tildes/dieresis/Ñ y filtros por longitud; se añadió
      `ACCENT_REPLACEMENTS` en `WordSearchTheme.gd` para normalizar a A–Z.
- [ ] Ejecutar la suite en el editor y verificar que un test fallido devuelve
      código distinto de cero (GdUnit4 lo garantiza; falta la comprobación en
      este entorno porque no hay binario de Godot en la terminal).
- [ ] Excluir `addons/gdUnit4/*` de las exportaciones (pendiente de
      `export_presets.cfg`).

---

## Fase 0.6 — Dirección de arte y componentes visuales (P2) 🔜

Fuente: [`art_direction.md`](../art_direction.md) y plantilla
`assets/reference/wathersoup.jpg`.

- [x] Analizar la plantilla: paleta, geometría y contrastes.
- [x] Guardar la plantilla en `assets/reference/wathersoup.jpg` (referencia interna).
- [x] Crear `docs/art_direction.md` y `assets/asset-manifest.json`.
- [ ] Elegir fuente pixel para las letras (A–Z, licencia libre).
- [ ] Elegir o generar el set de iconos (estrella, varita, lupa, refrescar,
      candado, pausa, menú, reiniciar, volver, ✔).
- [ ] Implementar `StyleBox` de celda, placa y botón con la paleta del template.
- [ ] Aplicar los estados de celda: reposo, seleccionada (azul), encontrada
      (borde turquesa + ✔) y pista (naranja con pulso).
- [ ] Corregir los contrastes señalados: texto negro sobre turquesa, cian y azul;
      borde oscuro `#535456` en placas y celdas.
- [ ] Botones de navegación a **56×56 px lógicos** (mayores que en el template).
- [ ] Comparar en editor con la plantilla a resolución nativa.

> La validación final de estilo se cierra en la Fase 9; aquí solo se construyen
> los componentes y se aprueba el primer tablero real.

---

## Fase 1 — Banco de palabras y 10 categorías (P1)

### 1.1 Modelo de datos

- [ ] Definir `WordSearchCategory` (o evolución de `WordSearchTheme`) con
      `category_id`, nombre, descripción y `Array[String]` de palabras.
- [ ] Decidir el formato del banco: recursos `.tres` incluidos en la app
      (recomendado para offline) o `user://word_bank.json`.
- [ ] Normalizar a A–Z sin tildes, diéresis ni Ñ (ESPAÑA → ESPANA).
- [ ] Validar duplicados, longitud mínima 4 y máxima igual al tablero.
- [ ] Validar que ninguna categoría quede vacía tras el filtro.

### 1.2 Contenido (10 categorías · ≥ 600 palabras)

- 🟡 Animales y Naturaleza (existe `animales.tres`; falta migrar al modelo nuevo).
- 🟡 Frutas y Verduras (existe `frutas_verduras.tres`; falta migrar).
- 🟡 Países y Geografía (existe `paises_ciudades.tres`; falta migrar).
- 🟡 Cosas del Hogar (existe `objetos.tres`; falta migrar).
- 🟡 Profesiones y Oficios (existe `profesiones.tres`; se mantiene `MECANICO`).
- [ ] Plantas y Flores.
- [ ] Comidas y Postres.
- [ ] Instrumentos Musicales.
- [ ] Herramientas y Bricolaje.
- [ ] Ropa y Costura.
- [ ] Completar 50–100 palabras por categoría (≥ 60 por categoría para dejar margen al filtro).
- [ ] Revisar que todas las palabras sean cotidianas para el perfil 40+/60+.

### 1.3 Shuffle Bag (P1)

- [ ] Implementar bolsa de 10 grupos sin repetición.
- [ ] Garantizar 2 apariciones de cada grupo en 20 partidas.
- [ ] Reiniciar la bolsa solo al agotarla.
- [ ] Persistir el estado de la bolsa en `SaveManager`.
- [ ] Test `UT-WS-06`.

---

## Fase 2 — `DifficultyConfig` y generador (P1)

### 2.1 Datos de dificultad

- [ ] Recurso `DifficultyConfig` con: `id`, nombre, `grid_size`, `target_words`,
      `min_words` (80 %), direcciones permitidas, `category_count`, `par_seconds`
      (para el puntaje) y nivel previo.
- [ ] Cuatro recursos `.tres`:

| Nivel | Tablero | Objetivo | Mínimo | Categorías | Direcciones | Par |
|-------|---------|----------|--------|------------|-------------|-----|
| Fácil | 8×8 | 5 | 4 | 1 | Horizontal y vertical directas | 240 s |
| Medio | 10×10 | 8 | 7 | 1 | + diagonales descendentes | 420 s |
| Difícil | 12×12 | 12 | 10 | 2 | + direcciones inversas | 600 s |
| Maestro | 15×15 | 20 | 16 | 4 | 8 direcciones | 720 s |

- [ ] Separar las dificultades de la lógica: nada de `if nivel == 3` en el generador.

### 2.2 `WordSearchGenerator` (Strategy)

- [ ] Renombrar `WordGenerator` → `WordSearchGenerator.gd` y actualizar referencias.
- [ ] API `generate(config, words) -> GenerationResult` con `grid`, `placements`,
      `placed_count` y `meets_minimum`.
- [ ] Respetar el vector de direcciones de la configuración.
- [ ] Solapar solo con letra coincidente.
- [ ] Rellenar huecos con letras aleatorias (sin dejar celdas vacías).
- [ ] Barajar y mezclar categorías según `category_count`.
- [ ] Reintentar hasta N veces (definir N, p. ej. 30) para alcanzar el objetivo.
- [ ] Aceptar el 80 % (`min_words`) si tras N intentos no se llega al 100 %.
- [ ] Devolver fallo controlado si no se alcanza el mínimo, sin tablero vacío.
- [ ] Medir tiempo de generación en 15×15 (< 0,5 s).
- [ ] Tests `UT-WS-01` a `UT-WS-05`.

---

## Fase 3 — Selección por arrastre (P1)

### 3.1 `WordGridContainer` y celdas

- [ ] Renombrar `WordGrid` → `WordGridContainer.gd` y `WordSearch` → `WordSearchMain.gd`.
- [ ] `WordSearchMain` hereda de `BaseMinigame` cuando exista (Fase 9).
- [ ] `WordCell` expone posición, letra, estado y estilo de pertenencia.
- [ ] Celda con el estilo de la plantilla: crema `#fbf3da`, borde `#535456`,
      radio 5 px y sombra dura de 2 px (`docs/art_direction.md` §4.1).
- [ ] Permitir que una celda pertenezca a varias palabras y que prevalezca el
      estilo de la **última encontrada**.
- [ ] Emitir `word_selected(start_coords, end_coords)` al soltar.

### 3.2 Entrada táctil

- [ ] Iniciar selección con `InputEventScreenTouch` (o ratón emulado).
- [ ] Seguir el trazo con `InputEventScreenDrag` y resaltar celdas en tiempo real.
- [ ] Bloquear la dirección tras un umbral de píxeles para evitar zigzag.
- [ ] Construir la ruta recta completa al soltar (extremos incluidos).
- [ ] Validar solo contra palabras pendientes, en el sentido seleccionado.
- [ ] Rechazar selecciones no alineadas o de una sola celda.
- [ ] Desvanecer la selección inválida en 0,5 s sin penalizar.
- [ ] No permitir seleccionar una palabra ya encontrada.
- [ ] Trazo retro semitransparente con contraste suficiente.
- [ ] Tests `IT-WS-01`, `IT-WS-02`, `IT-WS-03`.

### 3.3 Solo arrastre: usabilidad

- [ ] Tutorial breve de 1 paso la primera vez: “Toca y arrastra para marcar una palabra”.
- [ ] Tolerancia de dirección: si el dedo se desvía, se ajusta a la línea dominante.
- [ ] Celdas grandes y trazo visible para que no haga falta precisión.
- [ ] Cancelar la selección si el dedo sale del tablero.
- [ ] Probar con usuarios 60+ que el arrastre se descubre sin explicación.
- [ ] Documentar la excepción al principio global “sin gestos finos” (C4).

---

## Fase 4 — Encaje, zoom y layout (P1)

- [ ] Calcular el tamaño de celda a partir del área disponible (sin 48 px fijos).
- [ ] Respetar las proporciones del template: celda 41 px, separación 6 px y
      margen lateral 19 px, escalados a cada nivel (`docs/art_direction.md` §3).
- [ ] Tablero completo al 100 % al iniciar, sin scroll.
- [ ] Zoom con pinch de 2 dedos (`1×` a `2,5×`).
- [ ] Botones **+ / −** y **Centrar** como alternativa accesible (C8).
- [ ] Desplazamiento con límites para no perder el tablero.
- [ ] Restablecer zoom al iniciar o reiniciar partida.
- [ ] Evitar conflicto entre arrastre de selección y arrastre de zoom.
- [ ] Verificar 8×8, 10×10, 12×12 y 15×15 en 360×640, 20:9 y tablet.
- [ ] Verificar que las celdas no bajen de 24 px visibles en 15×15 sin zoom.
- [ ] Test `IT-WS-04` y pruebas manuales de encaje.

---

## Fase 5 — HUD, pausa y footer de palabras (P1)

### 5.1 HUD superior

- [ ] Botón de pausa/menú siempre visible.
- [ ] Contador “Palabras: X/Y”.
- [ ] Tiempo transcurrido visible y no punitivo (sin cuenta atrás).
- [ ] Indicador de dificultad actual.
- [ ] Placa de encabezado con el estilo del template (interior `#ead9bb`,
      borde gris de 3 tonos, radio 5 px, sombra dura).
- [ ] Botones y controles del HUD de **56×56 px lógicos**, mayores que en el
      template (`docs/art_direction.md` §3).

### 5.2 Modal de pausa

- [ ] Botón **Pista** con contador de usos.
- [ ] Botón **Reiniciar partida**.
- [ ] Botón **Salir al menú principal**.
- [ ] Cerrar pausa sin perder estado.
- [ ] Foco atrapado en el modal y cierre con la tecla Atrás.

### 5.3 Footer de palabras

- [ ] Listar las palabras en orden aleatorio.
- [ ] Tachar las encontradas.
- [ ] Mover la palabra encontrada al final de la lista.
- [ ] Mantener legibilidad con 5, 8, 12 y 20 palabras.
- [ ] No reducir el tablero por debajo del mínimo acordado.
- [ ] Test `ST-WS-01`.

---

## Fase 6 — Pistas (P1)

- [ ] 3 pistas por nivel, reiniciadas al empezar o reiniciar.
- [ ] Resaltar la palabra completa 3 s con borde y animación (no solo color).
- [ ] Nunca señalar palabras ya encontradas.
- [ ] Contador visible “Pista (3)”, “Pista (2)”, “Pista (1)”.
- [ ] Al llegar a 0, el botón queda inactivo con “Sin pistas” y se explica que
      se recuperan al reiniciar.
- [ ] Sin anuncios, sin compras y sin dependencia de conexión (C2).
- [ ] La pista no penaliza, no bloquea y no interrumpe la partida.
- [ ] Test `ST-WS-02`.
- [ ] Retirar `ST-WS-03` (Rewarded) de `test.md` al cerrar la fase.

---

## Fase 7 — Progresión, desbloqueo y submenú (P1)

- [ ] Submenú de dificultad previo al tablero.
- [ ] Mostrar candado y progreso “X/20” por nivel.
- [ ] Fácil desbloqueado por defecto.
- [ ] Medio tras 20 partidas de Fácil; Difícil tras 20 de Medio; Maestro tras 20 de Difícil.
- [ ] Contar solo partidas completadas (victoria), no intentos abandonados.
- [ ] Modal “¡Has desbloqueado el nivel …!” al alcanzar la partida 20.
- [ ] Persistir partidas completadas y niveles desbloqueados en `SaveManager`.
- [ ] Impedir entrar a un nivel bloqueado sin romper la navegación.
- [ ] Mostrar el progreso también en la pantalla de estadísticas.
- [ ] Test `ST-WS-04`.

---

## Fase 8 — Victoria, puntaje y estadísticas (P1)

- [ ] Modal de victoria con animación pixel art (confeti o estrellas).
- [ ] Mostrar tiempo final, dificultad superada y puntaje.
- [ ] Botones **Siguiente partida** y **Menú principal**.
- [ ] Fórmula de puntaje (C7):
      `puntaje = max(0, (palabras * 100 + max(0, par_seconds - segundos) - pistas * 50) * multiplicador)`,
      redondeado a entero, donde `multiplicador` es 1.0 en Fácil, 1.25 en Medio,
      1.5 en Difícil y 2.0 en Maestro, y `par_seconds` sale de `DifficultyConfig`.
- [ ] Registrar nivel, tiempo, puntaje y resultado en `SaveManager`.
- [ ] Actualizar mejor tiempo y mejor puntaje del nivel.
- [ ] Test `ST-WS-05` y los casos `STAT` globales.
- [ ] Sin anuncios dentro de la partida ni en la victoria.

---

## Fase 9 — Integración, accesibilidad y calidad (P1)

### 9.1 Integración

- [ ] `WordSearchMain` hereda de `BaseMinigame`.
- [ ] Ajustes accesibles desde pausa.
- [ ] Estadísticas visibles en la pantalla global por juego y nivel.
- [ ] Modo avión completo.

### 9.2 Accesibilidad

- [ ] Botones ≥ 56×56 px en los tres tamaños de texto.
- [ ] Contraste ≥ 4,5:1 en texto y ≥ 3:1 en bordes y estados.
- [ ] Estados nunca solo por color (borde, forma o icono).
- [ ] Texto Normal / Grande / Muy grande sin desbordes.
- [ ] Zoom con botones + / − como alternativa al pinch (C8).
- [ ] Validar que el arrastre (C4) funcione con dedos poco precisos.
- [ ] Feedback visual en < 0,2 s.

### 9.3 Rendimiento y estabilidad

- [ ] 60 fps en gama baja con 15×15 y zoom activo.
- [ ] Generación < 0,5 s en 15×15.
- [ ] Memoria estable y sin fugas al entrar y salir 20 veces.
- [ ] Segundo plano correcto.

### 9.4 Pruebas y entrega

- [ ] Ejecutar `UT-WS-01` a `UT-WS-06` en GdUnit4 headless.
- [ ] Ejecutar `IT-WS-01` a `IT-WS-04` con SceneRunner.
- [ ] Ejecutar `ST-WS-01`, `ST-WS-02`, `ST-WS-04` y `ST-WS-05` en editor y Android
      (`ST-WS-03` retirado por C2).
- [ ] Probar 16:9, 19.5:9, 20:9 y tablet.
- [ ] Prueba de usabilidad con el perfil 40+/60+.
- [ ] Consola sin errores ni advertencias.
- [ ] APK instalado y verificado.

---

## 4. Trazabilidad spec → plan → pruebas

| Requisito (spec) | Fase | Pruebas | Estado |
|------------------|------|---------|--------|
| §3.1 Arrastre y solapamiento | 3 | `IT-WS-01`…`IT-WS-03` | Pendiente |
| §3.2 Zoom y encaje | 4 | `IT-WS-04` | Pendiente |
| §3.3 HUD y pausa | 5 | Sin IDs en `test.md` (ampliar) | Pendiente |
| §3.4 Pistas | 6 | `ST-WS-02` | Pendiente |
| §3.5 Victoria | 8 | `ST-WS-05` | Pendiente |
| §4.1 Submenú de dificultad | 7 | `ST-WS-04` | Pendiente |
| §4.2 Matriz de dificultades | 2 | `UT-WS-01`…`UT-WS-05` | Pendiente |
| §4.3 Desbloqueo | 7 | `ST-WS-04` | Pendiente |
| §5 Banco y Shuffle Bag | 1 | `UT-WS-06` | Pendiente |
| §5.1 Diez categorías | 1 | Revisión de contenido | Parcial (5/10) |
| §6 Arquitectura | 2, 3, 9 | Revisión de código | Pendiente |
| Puntaje (C7) | 8 | Casos `STAT` globales | Pendiente |

**Huecos de cobertura en `test.md`:** HUD, pausa, tiempo, accesibilidad,
rendimiento y puntaje no tienen IDs propios. Se ampliarán en la Fase 5 o al
cerrar la Fase 9. `ST-WS-03` (Rewarded) queda fuera de alcance por C2 y debe
retirarse del documento de pruebas.

---

## Backlog específico

- [ ] Categorías adicionales (refranes, cine, música clásica).
- [ ] Reto diario opcional sin presión.
- [ ] Narración por voz de la lista de palabras.
- [ ] Temas de alto contraste adicionales.
- [ ] Tableros superiores a 15×15 como modo experto.

---

## Definición de terminado (por tarea)

1. Funciona en el editor con la dificultad y las categorías reales.
2. No introduce errores ni advertencias.
3. Cumple la accesibilidad de la spec y del SDD global.
4. Se probó el caso feliz y al menos un caso límite.
5. Tiene su test en `test.md` o se justifica por qué es manual.
6. Se registró en este plan.

---

## Riesgos y supuestos

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Tablero 15×15 con celdas mínimas | Dificultad táctil | Zoom, botones +/− y prueba con usuarios |
| Pinch y arrastre compiten por el gesto | Selección imposible | Umbral, bloqueo de dirección y modos separados |
| 600+ palabras por escribir y revisar | Retraso de contenido | Lotes por categoría y revisión con el perfil objetivo |
| Arrastre obligatorio en 60+ | Abandono si no se descubre | Tutorial de 1 paso, tolerancia y prueba de usabilidad |
| Desbloqueo con 20 partidas | Frustración 60+ | Progreso visible “X/20” y conteo solo de victorias |
| Generación parcial por debajo del 80 % | Bloqueo | Mínimo garantizado y mensaje con reintento |
| Ruta `scenes/` en la spec | Enlaces y docs rotos | C1: corregir la spec, no mover el código |
| Zoom y texto grande a la vez | Desbordes | Probar 20/24/28 px con zoom 1× y 2,5× |
| Rendimiento en gama baja con 15×15 | Mala experiencia | Medir FPS y simplificar dibujo del trazo |
