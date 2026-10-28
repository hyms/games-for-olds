# Plan General de Pruebas — Clásicos Mente Activa 2D (v0.3)

> **Documento SDD global de pruebas y calidad.**
> Define la estrategia, los servicios core, la accesibilidad, las estadísticas y
> la publicidad transversal. Las pruebas de cada juego viven en su `test.md`.
>
> **Versión:** 0.3 · **Fecha:** 26/10/2026 · **Estado:** MVP en definición
> **Documentación relacionada:** [Spec global](spec.md) · [Plan global](plan.md) ·
> [Pruebas de Sopa de Letras](word_search/test.md)

---

## 1. Estrategia

### 1.1 Framework

- **GdUnit4** como único framework de pruebas.
- Pruebas unitarias de lógica pura y servicios, sin cargar escenas visuales.
- Pruebas de integración para señales, navegación y persistencia.
- Pruebas manuales para accesibilidad, publicidad real y usabilidad.
- Ejecución headless en CI: el comando debe fallar con código distinto de cero
  cuando una prueba falla.

### 1.2 Niveles

| Nivel | Cuándo | Dónde |
|-------|--------|-------|
| Humo | Tras cada cambio | Editor de Godot |
| Unitario | Al cerrar cada servicio | GdUnit4 headless |
| Integración | Al cerrar cada fase | Editor + Android |
| Accesibilidad | Al cerrar cada pantalla | Android real |
| Usabilidad | Antes de publicar | Con usuario 40+ |

### 1.3 Enfoque

- **Caja negra** para lo que ve el jugador; **caja blanca** para servicios.
- **Accesibilidad como requisito funcional:** un fallo de accesibilidad es un
  defecto, no una mejora.
- **Casos límite siempre:** texto grande, JSON corrupto, offline, reinicios,
  cambios de escena repetidos y primera ejecución.
- **Trazabilidad:** cada caso tiene ID y cada defecto referencia su caso.

---

## 2. Entornos

| ID | Entorno | Uso |
|----|---------|-----|
| E1 | Editor Godot, 360×640 | Desarrollo diario |
| E2 | Editor Godot, 720×1280 | Escalado intermedio |
| E3 | Android gama baja (2–3 GB RAM) | Rendimiento mínimo |
| E4 | Android gama media (4–6 GB RAM) | Referencia |
| E5 | Pantalla 20:9 | Sin recortes |
| E6 | Tablet vertical | Escalado y centrado |
| E7 | Modo avión | Uso 100 % offline |

Configuraciones: texto Normal / Grande / Muy grande; sonido on/off; primera
ejecución y ejecución con datos previos; instalación limpia y actualización.

---

## 3. Pruebas de servicios core

### 3.1 `SaveManager`

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `CORE-SAVE-01` | Archivo por defecto | Borrar `user://save_data.json` e iniciar `load_data()` | Se crea JSON válido con `version`, ajustes por defecto y estadísticas vacías; sin contador global de pistas |
| `CORE-SAVE-02` | Registrar resultado | Registrar una victoria de Sopa nivel 2 con tiempo y puntaje; recargar | `current_level` avanza si se completó; `best_time_seconds` y `best_score` solo mejoran; `plays` y `wins` se incrementan |
| `CORE-SAVE-03` | JSON corrupto | Escribir texto inválido y ejecutar `load_data()` | Se detecta, se guarda respaldo, se restauran valores por defecto y no se cierra la app |
| `CORE-SAVE-04` | Escritura segura | Interrumpir la escritura y volver a cargar | El archivo anterior sigue siendo válido; no queda un JSON a medias |
| `CORE-SAVE-05` | Migración de versión | Abrir un archivo con `version` anterior | Se migra al esquema actual conservando ajustes y estadísticas |
| `CORE-SAVE-06` | Reinicio de estadísticas | Ejecutar `reset_stats()` con confirmación | Todos los juegos vuelven a nivel 1 y a valores iniciales |

### 3.2 `ThemeManager`

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `CORE-THEME-01` | Cambio de tema | Conectar `theme_changed`; llamar `set_theme("dark_contrast")` | Se emite la señal con el tema actualizado y la UI cambia |
| `CORE-THEME-02` | Tamaño de texto | Llamar `set_text_size("large")` | Cambian los tamaños de fuente; **no** cambia el viewport ni se reducen los objetivos táctiles |
| `CORE-THEME-03` | Persistencia de texto | Cambiar a "xlarge", reiniciar la app | El tamaño se conserva y se aplica al arrancar |

### 3.3 `AudioManager`

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `CORE-AUDIO-01` | Volumen por bus | Cambiar BGM y SFX | Cada bus cambia sin afectar al otro |
| `CORE-AUDIO-02` | Silencio total | Activar mute | No suena nada hasta desactivarlo |
| `CORE-AUDIO-03` | Segundo plano | Minimizar y volver | El audio se pausa y reanuda correctamente |

### 3.4 `AdManager`

> En el MVP el proveedor es **no-op**. La publicidad real entra en la Fase 8.

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `CORE-AD-01` | No-op en editor | Ejecutar `show_banner("main_menu")` en editor | No se carga ningún anuncio y no hay errores |
| `CORE-AD-02` | Offline | Simular offline y mostrar banner | Se emite `banner_failed`, el banner se oculta y el layout no se rompe |
| `CORE-AD-03` | Ubicaciones permitidas | Recorrer menú y estadísticas | Solo existe contenedor de banner en menú principal y estadísticas |
| `CORE-AD-04` | Sin anuncios en partida | Entrar a cada juego | No hay banner ni contenedor publicitario en la partida |

---

## 4. Estadísticas y progresión

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `STAT-01` | Estadísticas por juego | Abrir la pantalla de estadísticas | Cada juego muestra nivel actual, mejor tiempo del nivel y mejor puntaje |
| `STAT-02` | Nivel actual | Completar el nivel 1 de un juego | `current_level` avanza al siguiente nivel |
| `STAT-03` | Mejor tiempo | Completar el mismo nivel más rápido | `best_time_seconds` baja; si es más lento, no empeora |
| `STAT-04` | Mejor puntaje | Completar el mismo nivel con más puntaje | `best_score` sube; si es menor, no baja |
| `STAT-05` | Nivel sin terminar | Abandonar una partida | No se registra victoria ni mejor tiempo |
| `STAT-06` | Reinicio | Restablecer estadísticas y confirmar | Todos los juegos vuelven al estado inicial |
| `STAT-07` | Persistencia | Reiniciar la app | Nivel, mejor tiempo y mejor puntaje se conservan |

---

## 5. Accesibilidad

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `UI-ACC-01` | Objetivos táctiles | Medir botones en menú, ajustes, estadísticas y juegos | ≥ 56×56 px en todos los tamaños de texto |
| `UI-ACC-02` | Tamaño de texto Normal | Revisar etiquetas y botones | Texto base ≥ 20 px |
| `UI-ACC-03` | Tamaño Grande | Activar Grande | Texto principal ≥ 24 px; controles siguen ≥ 56×56 |
| `UI-ACC-04` | Tamaño Muy grande | Activar Muy grande | Texto principal ≥ 28 px; nada se corta ni se sale de pantalla |
| `UI-ACC-05` | Sin desbordes con texto grande | Recorrer menú, ajustes, estadísticas y partidas en E1/E5/E6 | Todo el texto se lee completo; los botones no se solapan |
| `UI-ACC-06` | Contraste por paleta | Medir texto y bordes en Papel Cálido, Oscuro Alto Contraste y Azul Calma | ≥ 4,5:1 en texto y ≥ 3:1 en bordes/estados |
| `UI-ACC-07` | Nunca solo color | Revisar estados seleccionado/encontrado/error | Siempre hay forma, borde, icono o texto adicional |
| `UI-ACC-08` | Feedback inmediato | Tocar celdas y botones | Respuesta visual en < 0,2 s |
| `UI-ACC-09` | Movimiento reducido | Activar la opción | El fondo animado se detiene y la app sigue usable |
| `UI-ACC-10` | Sin gestos finos | Jugar solo con toques | Toda acción tiene alternativa táctil simple |

---

## 6. Navegación y humo global

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `NAV-01` | Menú principal | Abrir la app | Se ven los tres juegos, Estadísticas, Ajustes y Salir |
| `NAV-02` | Entrar a cada juego | Menú → cada juego | Se abre la pantalla correcta |
| `NAV-03` | Volver | Desde cada pantalla, volver | Menú en máximo dos toques |
| `NAV-04` | Ida y vuelta | Entrar y salir 20 veces | Sin bloqueos ni estado viejo |
| `NAV-05` | Juego pendiente | Entrar a Tangram o Bloques si aún no existen | Mensaje claro; sin pantalla vacía ni bloqueo |
| `NAV-06` | Atrás del sistema | Pulsar atrás en menú y partida | Comportamiento predecible; no cierra por accidente |
| `NAV-07` | Rotación | Girar el móvil | La app permanece vertical |
| `SMK-01` | Humo de juego | Entrar, iniciar, actuar y salir | Sin errores y con feedback visual |
| `SMK-02` | Consola | Recorrer el flujo completo | Sin errores ni advertencias nuevas |

---

## 7. Publicidad (Fase 8)

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `ADS-01` | Banner en menú | Activar publicidad de prueba | Banner en la parte superior, sin tapar botones |
| `ADS-02` | Banner en estadísticas | Abrir estadísticas | Banner en la parte superior, sin tapar tarjetas ni botones |
| `ADS-03` | Sin anuncios en partida | Entrar a cada juego | No aparece ningún anuncio |
| `ADS-04` | Offline | Activar modo avión | El banner se oculta y el layout se reacomoda sin saltos |
| `ADS-05` | Fallo de carga | Simular `banner_failed` | Mensaje discreto o espacio vacío; el juego sigue funcionando |
| `ADS-06` | Sin rewarded | Revisar la app | No existe flujo de anuncio bonificado en el MVP |
| `ADS-07` | Consentimiento | Primera ejecución con publicidad | Se solicita consentimiento antes de cargar anuncios |

---

## 8. Rendimiento y estabilidad

| ID | Caso | Pasos | Resultado esperado |
|----|------|-------|--------------------|
| `PERF-01` | FPS en gama baja | Usar 5 minutos en E3 | 60 fps estables (mínimo 55 puntual) |
| `PERF-02` | Arranque | Abrir en frío | < 3 s hasta el menú |
| `PERF-03` | Carga de nivel | Iniciar una partida | < 0,5 s hasta que el tablero es jugable |
| `PERF-04` | Memoria | Sesión de 15 minutos | < 150 MB y sin crecimiento anómalo |
| `PERF-05` | Cambios de escena | Entrar y salir 20 veces | Sin fugas ni errores de nodos liberados |
| `PERF-06` | Segundo plano | Minimizar y volver | Se recupera sin perder estado |

---

## 9. Usabilidad

Protocolo con adultos 40+ y 60+:

1. **Autonomía inicial:** "Abre la app, agranda el texto y empieza una Sopa en
   nivel Fácil". Éxito en ≤ 45 s sin asistencia.
2. **Comprensión de estadísticas:** "Mira tu nivel, tu mejor tiempo y tu mejor
   puntaje". Éxito: identifica los tres datos sin ayuda.
3. **Salida sin frustración:** "Sal a la mitad de una partida". Éxito: encuentra
   Volver o Pausa en un intento.

---

## 10. Matriz de trazabilidad

Cada requisito del [spec global](spec.md) debe tener al menos una prueba. Si un
requisito no aparece aquí, no está cubierto.

| Requisito (spec) | Pruebas | Fase |
|------------------|---------|------|
| §5.1 Menú principal | `NAV-01` | 4 |
| §5.2 Estadísticas | `STAT-01`, `STAT-06`, `STAT-07`, `ADS-02` | 4 |
| §5.3 Ajustes | `CORE-THEME-01`…`03`, `CORE-AUDIO-01`…`03` | 3–4 |
| §5.4 Partida | `SMK-01`, `NAV-02`, `ADS-04` | 5–7 |
| §5.5 Fin de partida | `PRG-*` del juego, `STAT-02`…`05` | 5–7 |
| §5.6 Navegación | `NAV-01`…`NAV-07` | 3–4 |
| §6 Accesibilidad | `UI-ACC-01`…`UI-ACC-10` | 4, 9 |
| §7 Plataforma y rendimiento | `PERF-01`…`PERF-06`, `NAV-07` | 9 |
| §8 Arquitectura y servicios | `CORE-SAVE-01`…`06`, `CORE-THEME-01`…`03`, `CORE-AUDIO-01`…`03`, `CORE-AD-01`…`04` | 3 |
| §9 Publicidad | `ADS-01`…`ADS-07`, `CORE-AD-01`…`04` | 8 |
| §10 Persistencia y estadísticas | `CORE-SAVE-01`…`06`, `STAT-01`…`STAT-07` | 3–4 |
| §13 Criterios de aceptación | Regresión §11 + planes de cada juego | 9 |

Regla de mantenimiento: al añadir un requisito al spec se añade su fila aquí y
su caso concreto; al eliminar un caso, se revisa la fila para no dejar huecos.

---

## 11. Regresión y criterios de salida

Antes de cada entrega:

1. Instalación limpia y arranque en frío.
2. Menú → estadísticas → ajustes → los tres juegos.
3. Humo de cada juego.
4. Accesibilidad básica (`UI-ACC-01` a `UI-ACC-08`).
5. Cambios de escena repetidos 20 veces.
6. Consola sin errores ni advertencias nuevas.
7. Modo avión.
8. Actualización sobre una instalación anterior.
9. Una partida completa del juego modificado.

No se publica si existe un **P0** (cierre, bloqueo, pérdida de datos) o un **P1**
abierto en navegación, accesibilidad básica, estadísticas o flujo principal.

---

## 12. Severidades y reporte

| Severidad | Descripción | Ejemplo |
|-----------|-------------|---------|
| P0 | Bloqueante total | La app se cierra al entrar a un juego |
| P1 | Impide jugar o incumple accesibilidad | No se puede completar un nivel o el texto se corta |
| P2 | Molesto con alternativa | Un botón secundario no responde |
| P3 | Cosmético | Un texto descentrado |

Plantilla: ID, título, severidad, pantalla, entorno, pasos, resultado esperado,
resultado obtenido, evidencia, frecuencia, caso relacionado y notas.
