# Plan de Trabajo Global — Clásicos Mente Activa 2D (v0.3)

> **Documento SDD global del producto.**
> Roadmap, decisiones y checklist de alto nivel. El detalle de cada juego vive en
> su propio `plan.md`.
>
> **Versión:** 0.3 · **Fecha:** 26/10/2026 · **Estado:** Fase 3 en curso
> **Documentación relacionada:** [Spec global](spec.md) · [Pruebas globales](test.md) ·
> [Sopa de Letras](../minigames/word_search/spec.md)

## Leyenda

- `[x]` Hecho y verificado en Godot
- `[ ]` Pendiente
- 🟡 Parcial o pendiente de verificación
- 🔜 En curso (siguiente tarea a atacar)
- **P1** Bloqueante para la fase · **P2** Importante · **P3** Mejora

---

## Decisiones tomadas

| ID | Decisión | Resultado |
|----|----------|-----------|
| D1 | Estructura de carpetas | **Mantener la estructura actual del skill:** `autoloads/`, `core/`, `ui/`, `minigames/`, `resources/`, `assets/`, `tests/`, `docs/`. Sin migración a `scenes/`. |
| D2 | Pistas | **Cada juego las define.** La Sopa usa 3 por partida, sin anuncios. El global no impone contadores. |
| D3 | Tiempo y estadísticas | **Pantalla global por juego** con nivel actual, mejor tiempo del nivel actual y mejor puntaje. El tiempo se mide en segundo plano. |
| D4 | Modo accesible | **Solo cambia el tamaño del texto:** Normal 20 px, Grande 24 px, Muy grande 28 px. No cambia el viewport ni reduce los objetivos táctiles. |
| D5 | Publicidad | **Entra al final (Fase 8).** Solo banners en la parte superior del menú principal y de estadísticas. Nunca en partidas. Sin Rewarded Ads en el MVP. |
| D6 | Dificultad | **Cada juego define sus dificultades.** La Sopa define Fácil (8×8), Medio (10×10) y Difícil (10×10 con invertidas). |
| D7 | Framework de pruebas | **GdUnit4**, con ejecución headless. |

---

## Orden de dependencias

```text
Fase 0  Base técnica (hecha)
Fase 1  Sopa: base funcional (hecha con salvedades)
   │
Fase 2  Alineación SDD y decisiones (hecha)
   │
Fase 3  Servicios core + GdUnit4 (en curso)
   │
Fase 4  BaseMinigame + ajustes + estadísticas + texto accesible
   │
Fase 5  Sopa: cierre de jugabilidad
   ├──────────────► Fase 6  Tangram
   └──────────────► Fase 7  Lógica de Bloques
                         │
Fase 8  Publicidad (banners en menú y estadísticas)
   │
Fase 9  Calidad, rendimiento y entrega
```

Regla: ninguna fase de juego se cierra sin sus servicios core y sin sus pruebas
mínimas.

---

## Fase 0 — Base técnica

- [x] Estructura actual de carpetas (`autoloads/`, `core/`, `ui/`, `minigames/`, `resources/`, `assets/`, `tests/`, `docs/`).
- [x] Proyecto en vertical 360×640, estirado `canvas_items`, orientación móvil.
- [x] Emulación de toque → ratón para escritorio.
- [x] Autoload único `SceneManager` con navegación por rutas y señales tipadas.
- [x] Tema accesible compartido con tipografía y estilos de alto contraste.
- [x] Componente `BigButton` reutilizable (mínimo 56×56, foco).
- [x] Menú principal con los tres juegos y Salir.
- [x] Documentación SDD global y del primer minijuego.
- [x] Skill del proyecto (`godot4-mastery`) con guías y catálogo de errores.
- [x] Repositorio con `.gitignore`, `.gitattributes` y remoto `origin`.

---

## Fase 1 — Sopa de Letras: base funcional

- [x] Spec, plan y pruebas del minijuego.
- [x] Recurso de temática editable y cinco temáticas.
- [x] Normalización de palabras y filtros de longitud.
- [x] Generador procedural con diagonales e invertidas configurables.
- [x] Celdas con estados normal / seleccionada / encontrada.
- 🟡 Selección por toque inicial + final: la ruta no se completa y la validación solo ve la primera celda.
- 🟡 Validación de palabras: código presente pero inoperante hasta corregir la selección.
- [x] Contador, botones Volver / Reiniciar / Pista y paso de temática.
- [x] Tipado estricto corregido en los scripts actuales.

> La Fase 1 no se cierra hasta corregir la selección en la Fase 5.

---

## Fase 2 — Alineación SDD y decisiones

> Cerrada el 26/10/2026. Todos los documentos son coherentes entre sí y no
> quedan P0/P1 documentales.

- [x] Resolver D1–D7 (ver tabla de decisiones).
- [x] Reescribir `docs/spec.md` con estructura actual, estadísticas, texto accesible y publicidad al final.
- [x] Reescribir `docs/test.md` con GdUnit4, servicios, estadísticas y banners.
- [x] Reescribir este plan con el orden definitivo.
- [x] Actualizar `minigames/word_search/spec.md` con niveles, 3 pistas y puntaje.
- [x] Actualizar `minigames/word_search/plan.md` con estados reales.
- [x] Actualizar `minigames/word_search/test.md` con límite de pistas, niveles y estadísticas.
- [x] Revisar enlaces y rutas entre documentos (0 enlaces rotos).
- [x] Cerrar la revisión SDD de la Sopa sin P0/P1 documentales.

---

## Fase 3 — Servicios core y pruebas (P1) 🔜

> `SceneManager` sigue siendo el único autoload. Los servicios son clases de
> `core/` que él instancia; las pruebas pueden crear instancias aisladas.

### 3.1 Infraestructura

- [ ] Instalar GdUnit4 y documentar la ejecución headless.
- [ ] Crear `tests/unit` y `tests/integration` con convención de nombres.
- [ ] Primeros tests puros: `WordGenerator`, `WordSearchTheme` y serialización JSON.
- [ ] Comando de tests reproducible en CI.

### 3.2 SaveManager

- [ ] Esquema JSON v1: `version`, `settings`, `stats`.
- [ ] Valores por defecto y creación del archivo si no existe.
- [ ] Escritura segura (temporal + reemplazo).
- [ ] Recuperación ante JSON corrupto con respaldo.
- [ ] Migración por versión.
- [ ] API de ajustes: volúmenes, tamaño de texto, tema y movimiento reducido.
- [ ] API de estadísticas: `current_level`, `best_time_seconds`, `best_score`, `plays`, `wins`.
- [ ] Señal `data_changed`.
- [ ] Tests `CORE-SAVE-01` a `CORE-SAVE-06`.

### 3.3 ThemeManager

- [ ] Tres paletas: Papel Cálido, Oscuro Alto Contraste, Azul Calma.
- [ ] Tamaños de texto Normal / Grande / Muy grande, solo tipografía.
- [ ] Aplicación en caliente a la escena actual.
- [ ] Validación de contraste ≥ 4,5:1 en texto y ≥ 3:1 en bordes.
- [ ] Señales `theme_changed` y `text_size_changed`.
- [ ] Tests `CORE-THEME-01` a `CORE-THEME-03`.

### 3.4 AudioManager

- [ ] Buses BGM y SFX.
- [ ] Volumen general, por bus y silencio total.
- [ ] Persistencia de ajustes.
- [ ] Pausa/reanudación en segundo plano.
- [ ] Tests `CORE-AUDIO-01` a `CORE-AUDIO-03`.

### 3.5 AdManager

- [ ] Interfaz de proveedor de banners.
- [ ] Proveedor no-op para editor, escritorio y offline.
- [ ] Señales `banner_loaded` y `banner_failed`.
- [ ] Ubicaciones permitidas: `main_menu` y `stats`.
- [ ] Sin anuncios en partidas.
- [ ] Tests `CORE-AD-01` a `CORE-AD-04`.

### 3.6 SceneManager

- [ ] Navegación a menú, estadísticas, ajustes y juegos.
- [ ] Exposición de `save`, `theme`, `audio` y `ads`.
- [ ] Transiciones con fundido.
- [ ] Tests de navegación y cambio de escena repetido.

---

## Fase 4 — BaseMinigame, ajustes y estadísticas (P1)

### 4.1 BaseMinigame

- [ ] Escena/clase base con encabezado, pausa, final de partida y acceso a ajustes.
- [ ] Gancho de estadísticas: nivel, tiempo, puntaje y resultado.
- [ ] Espacio opcional para ayudas propias del juego.
- [ ] Sin publicidad dentro de la base.
- [ ] Migrar la Sopa a `BaseMinigame`.

### 4.2 Ajustes

- [ ] Acceso desde el menú y desde la pausa.
- [ ] Audio y música (on/off y volumen).
- [ ] Tamaño de texto: Normal / Grande / Muy grande.
- [ ] Selector de tema de color.
- [ ] Movimiento reducido.
- [ ] Estadísticas y reinicio con confirmación.
- [ ] Persistencia inmediata.

### 4.3 Pantalla de estadísticas

- [ ] Acceso desde el menú principal.
- [ ] Tarjeta por juego: nivel actual, mejor tiempo del nivel y mejor puntaje.
- [ ] Reinicio de estadísticas con confirmación.
- [ ] Contenedor de banner en la parte superior (no-op hasta Fase 8).
- [ ] Pruebas `STAT-01` a `STAT-07`.

### 4.4 Fondo animado y texto accesible

- [ ] Fondo pixel art en bucle con presupuesto de rendimiento.
- [ ] Opción de movimiento reducido.
- [ ] Verificar 60 fps con el fondo activo.
- [ ] Verificar que el texto grande no desborda en 360×640, 20:9 y tablet.
- [ ] Verificar 56×56 px en los tres tamaños de texto.

---

## Fase 5 — Sopa de Letras: cierre de jugabilidad

### 5.1 Selección (P1)

- [ ] Construir la ruta recta completa entre primera y última celda.
- [ ] Rechazar selecciones no alineadas o de una sola celda.
- [ ] Validar por secuencia de celdas y por palabra, en cualquier sentido.
- [ ] Feedback de selección inválida sin penalización.
- [ ] Impedir iniciar selección en celdas ya encontradas.

### 5.2 Tablero y niveles (P1)

- [ ] Tablero visible y completo en 8×8 y 10×10.
- [ ] Celda adaptable al espacio disponible (sin 48 px fijos).
- [ ] Variación de estilo “WordCell”.
- [ ] Temática por defecto si se abre la escena directamente.
- [ ] Handoff del selector con la señal oficial de cambio de escena.
- [ ] Selector de nivel Fácil / Medio / Difícil.
- [ ] Tablero completo en 360×640, 20:9 y tablet.

### 5.3 Lista de palabras y pistas (P1)

- [ ] Lista de palabras pendientes visible y actualizada.
- [ ] Palabras encontradas tachadas o marcadas.
- [ ] 3 pistas por partida, con contador visible.
- [ ] Pista resalta una palabra pendiente durante 3 s.
- [ ] Botón “Sin pistas” al agotarlas; se restauran al reiniciar.
- [ ] Feedback no cromático al usar la pista.

### 5.4 Progreso, puntaje y estadísticas (P1)

- [ ] Contador sincronizado y reinicio limpio.
- [ ] Mensaje de victoria con tiempo, puntaje, Repetir y Volver.
- [ ] Fórmula de puntaje de la Sopa y multiplicador por nivel.
- [ ] Registro de nivel, tiempo y puntaje al terminar.
- [ ] `best_time_seconds` y `best_score` del nivel actual.
- [ ] Pausa con acceso a ajustes.

### 5.5 Pruebas del minijuego (P1)

- [ ] Actualizar `test.md` con los casos nuevos.
- [ ] Tests unitarios de generación, selección y puntaje.
- [ ] Pruebas manuales en editor y Android.
- [ ] Cerrar todos los P0/P1 de la revisión SDD.

---

## Fase 6 — Tangram / Puzles de encaje

- [ ] Crear documentación SDD del minijuego (`spec`, `plan`, `test`).
- [ ] Escena del juego y acceso desde el menú.
- [ ] Siete piezas clásicas con estilo pixel art.
- [ ] Arrastrar, soltar, rotar por pasos y snap con tolerancia.
- [ ] Niveles y dificultades propias como datos editables.
- [ ] Ayudas propias definidas en su `spec.md`.
- [ ] Puntaje propio y estadísticas por nivel.
- [ ] Pausa, ajustes y accesibilidad (sin gestos finos, sin tiempo).
- [ ] Pruebas unitarias, de integración y manuales.

---

## Fase 7 — Lógica de Bloques

- [ ] Crear documentación SDD del minijuego (`spec`, `plan`, `test`).
- [ ] Escena del juego y acceso desde el menú.
- [ ] Tablero, cápsulas, movimiento, giro y caída.
- [ ] Detección de líneas de cuatro o más, eliminación y cadenas.
- [ ] Condiciones de nivel completado y fin de partida.
- [ ] Modo relajado por defecto y modo normal.
- [ ] Niveles, dificultades y ayudas propias.
- [ ] Puntaje propio y estadísticas por nivel.
- [ ] Pausa accesible y colores distinguibles por forma/símbolo.
- [ ] Pruebas unitarias, de integración y manuales.

---

## Fase 8 — Publicidad (banners)

> Entra al final, cuando los servicios y los juegos estén estables.

- [ ] Seleccionar un plugin de banners compatible con Godot 4.7.
- [ ] Configurar IDs de prueba y modo test.
- [ ] Implementar consentimiento (UMP/GDPR).
- [ ] Banner en la parte superior del menú principal.
- [ ] Banner en la parte superior de estadísticas.
- [ ] Reserva de espacio y reacomodo sin saltos al ocultarse.
- [ ] Ocultar banner offline.
- [ ] Fallo de carga sin bloquear la app.
- [ ] Proveedor no-op en editor/escritorio.
- [ ] Política de privacidad y ficha de tienda.
- [ ] Pruebas `ADS-01` a `ADS-07`.

---

## Fase 9 — Calidad, rendimiento y entrega

### 9.1 Pruebas globales (P1)

- [ ] Ejecutar los planes de los tres juegos.
- [ ] Ejecutar `docs/test.md` completo en editor y Android.
- [ ] Probar 16:9, 18:9, 19.5:9 y 20:9.
- [ ] Usabilidad con usuarios 40+ y 60+.
- [ ] Consola sin errores ni advertencias nuevas.

### 9.2 Rendimiento (P1)

- [ ] 60 fps en gama baja (mínimo 55 puntual).
- [ ] Arranque < 3 s; carga de nivel < 0,5 s.
- [ ] Memoria < 150 MB.
- [ ] Sin fugas al cambiar de escena.
- [ ] Segundo plano correcto.

### 9.3 Accesibilidad y contenido (P1)

- [ ] Contraste en las tres paletas.
- [ ] Texto Normal / Grande / Muy grande sin desbordes.
- [ ] 56×56 px en todos los modos.
- [ ] Estadísticas correctas por juego y nivel.
- [ ] Textos, tutoriales y ayuda revisados.

### 9.4 Exportación y publicación (P1)

- [ ] Instalar plantillas de exportación.
- [ ] Configurar preset Android.
- [ ] Build firmado e instalación limpia.
- [ ] Permisos mínimos, offline y políticas de tienda.
- [ ] Ficha, capturas y política de privacidad.
- [ ] Publicar y archivar copia.

---

## Plan de acción inmediato

> Orden de ejecución desde el estado actual.

### Paso 0 — Decisiones y documentación (hecho)

- [x] D1–D7 resueltas.
- [x] `docs/spec.md` v0.3.
- [x] `docs/test.md` v0.3.
- [x] `docs/plan.md` v0.3.
- [x] Sopa: pistas 3 por partida y sin anuncios.
- [x] Sopa: niveles Fácil / Medio / Difícil.
- [x] Sopa: puntaje y estadísticas.
- [x] Profesiones: `FARMACEUTICO` sustituida por `MECANICO`.

### Paso 1 — Cerrar la documentación de la Sopa

- [x] Actualizar `minigames/word_search/plan.md` con estados reales.
- [x] Actualizar `minigames/word_search/test.md` con pistas, niveles y estadísticas.
- [x] Revisar enlaces y matriz de trazabilidad.

### Paso 2 — Infraestructura de pruebas

- [ ] Instalar GdUnit4.
- [ ] Ejecución headless documentada.
- [ ] Primeros tests puros.

### Paso 3 — Servicios core

1. SaveManager.
2. ThemeManager (solo texto).
3. AudioManager.
4. AdManager (no-op).
5. SceneManager (navegación y servicios).

### Paso 4 — BaseMinigame, ajustes y estadísticas

- [ ] Base común.
- [ ] Ajustes con tamaño de texto.
- [ ] Pantalla de estadísticas.
- [ ] Fondo animado y movimiento reducido.

### Paso 5 — Cierre de la Sopa

- [ ] Selección real.
- [ ] Tablero 8×8 / 10×10.
- [ ] Lista de palabras.
- [ ] 3 pistas.
- [ ] Niveles, puntaje y estadísticas.
- [ ] Pruebas completas.

### Paso 6 — Tangram y Bloques

- [ ] Documentación SDD antes de programar.
- [ ] Implementación con sus propias dificultades, ayudas y puntajes.

### Paso 7 — Publicidad

- [ ] Banners en menú y estadísticas.
- [ ] Consentimiento, offline y no-op.

### Paso 8 — Calidad y entrega

- [ ] Regresión, rendimiento, APK firmado y publicación.

### Orden de commits sugerido

1. `docs: decisiones D1-D7 y spec v0.3`
2. `docs: pruebas v0.3 y plan v0.3`
3. `docs: Sopa con niveles, 3 pistas y puntaje`
4. `test: instalar GdUnit4`
5. `feat: SaveManager`, `feat: ThemeManager`, `feat: AudioManager`, `feat: AdManager`
6. `feat: BaseMinigame, ajustes y estadísticas`
7. `fix(word_search): selección, tablero y pistas`
8. `feat: Tangram`, `feat: Lógica de Bloques`
9. `feat: banners en menú y estadísticas`
10. `chore: exportación y publicación`

---

## Backlog global

- [ ] Nuevos juegos clásicos (dominó, memori, crucigramas, sudoku fácil).
- [ ] Nuevas temáticas para Sopa de Letras.
- [ ] Más niveles para Tangram y Bloques.
- [ ] Reto diario opcional sin presión de tiempo.
- [ ] Multijugador local por turnos.
- [ ] Narración de instrucciones por voz.
- [ ] Más paletas de alto contraste y ajustes de daltonismo.
- [ ] Exportación de progreso.
- [ ] Versión para tablets con áreas mayores.
- [ ] iOS.

---

## Definición de terminado (global)

Un juego se considera terminado cuando:

1. Su `spec.md` está implementado y verificado.
2. Su `test.md` se ejecutó completo en editor y Android.
3. Los servicios core que usa tienen pruebas automatizadas.
4. No quedan defectos P0 ni P1 abiertos.
5. Cumple accesibilidad: 56×56 px, texto escalable sin desbordes, contraste y estados no solo por color.
6. Sus ayudas y dificultades cumplen su propio `spec.md`.
7. Sus estadísticas (nivel, mejor tiempo y mejor puntaje) se registran y muestran.
8. Está integrado en el menú y se juega de principio a fin.
9. Está reflejado en este plan y en su plan específico.

---

## Riesgos y supuestos

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| El texto grande desborda botones o listas | Mala legibilidad | Probar 20/24/28 px en E1/E5/E6; permitir crecimiento en altura |
| La selección por toques no se completa | Juego injugable | Corregir ruta y validación en Fase 5 con tests |
| Tablero 10×10 con celdas pequeñas | Dificultad táctil | Celdas ≥ 30 px, toques y pruebas con usuarios |
| Estadísticas mal definidas | Progreso confuso | `current_level`, `best_time_seconds` y `best_score` por nivel |
| Publicidad invasiva | Abandono | Solo banners en menú y estadísticas, nunca en partida |
| Plugin de banners incompatible | Retraso | Spike en Fase 8 y proveedor no-op |
| JSON corrupto | Pérdida de datos | Escritura atómica, respaldo y migración |
| Dificultades mal calibradas | Frustración | Cada juego las define y las prueba en su spec |
| Fondo animado en gama baja | Caída de FPS | Presupuesto y movimiento reducido |
| Tipado estricto de GDScript | Retrasos | Skill del proyecto y catálogo de errores |

---

## Gobernanza SDD

- Cada minijuego nace con `spec.md`, `plan.md` y `test.md` antes de su lógica.
- Los documentos globales resumen y enlazan; no duplican reglas de juego.
- Toda tarea completada se marca en el plan correspondiente.
- Todo cambio de reglas se refleja primero en el spec y luego en el código y las pruebas.
- Las decisiones D1–D7 están cerradas y documentadas.
- Ninguna fase se cierra con P0/P1 abiertos ni con contradicciones entre documentos.
