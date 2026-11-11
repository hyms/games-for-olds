# 🧪 Plan de Pruebas y TDD — Sopa de Letras (cobertura ≥ 60%)

> **Documento SDD de pruebas específicas para el minijuego Sopa de Letras.**
> Roadmap y suite de verificación para alcanzar **≥ 60 % de cobertura de pruebas**
> antes de la liberación en producción. Cubre lógica del núcleo, estado y progresión,
> e interacción UI/entrada, con integración en el entorno de pruebas headless de CI.

---

## 1. Objetivo y criterio de aceptación (coverage target)

| Índice | Objetivo | Umbral | Cómo se mide |
|--------|----------|--------|--------------|
| **CA-01** | Cobertura de línea/rama en `ClasicosMenteActiva.minigames.word_search` | ≥ 60 % | `dotnet test --collect:"XPlat Code Coverage"` |
| **CA-02** | Cobertura de línea/rama en `ClasicosMenteActiva.core` (SaveManager, DifficultyConfig) | ≥ 60 % | `dotnet test --collect:"XPlat Code Coverage"` |
| **CA-03** | Cada requisito del `spec.md` con al menos una prueba trazable (ID `UT-WS` / `IT-WS` / `ST-WS` / `VT-WS` / `CORE-SAVE`) | 100 % | Matriz de trazabilidad §7 |
| **CA-04** | Cero pruebas fallando en ejecución headless de CI | 0 fallos | `dotnet test` devuelve código de salida distinto de cero si falla |

**Resultado esperado:** con CA-01 y CA-02 cumplidas, el minijuego se considera listo para producción respecto a calidad de pruebas (puerta de calidad de la Fase 9).

---

## 2. Marco de pruebas y entorno headless

### 2.1 Framework

El proyecto estandarizó en **GdUnit4Net** (C#, ejecutable con `dotnet test` y el adaptador VSTest), que es el equivalente de **GUT / Godot Unit Test** para código C#. No se utiliza GDScript para las pruebas del minijuego.

Paquetes NuGet (`ClasicosMenteActiva2D.csproj`): `gdUnit4.api` 5.1.0-rc6, `gdUnit4.test.adapter` 3.2.0, `gdUnit4.analyzers` 1.1.0, `Microsoft.NET.Test.Sdk`.

Atributos y convenciones:

| Atributo / clase | Uso |
|------------------|-----|
| `[TestSuite]` | Marca la clase que contiene pruebas |
| `[RequireGodotRuntime]` | Obligatorio cuando el prueba toca objetos de Godot (nodos, recursos, escenas) |
| `[TestCase]` | Cada caso de prueba individual |
| `[BeforeTest]` / `[AfterTest]` | Inicialización y limpieza por caso (rutas `user://test_*` aisladas) |
| `ISceneRunner` | Carga escenas reales (`WordSearchMain.tscn`) para pruebas de integración |
| `Utils.CreateRng(seed)` / `seed ≥ 0` | Barajado determinista en pruebas del generador |

### 2.2 Estructura de carpetas

| Carpeta | Contenido | Convención |
|---------|-----------|------------|
| `tests/unit/` | Pruebas puras de lógica y recursos, sin escenas visuales | `*Test.cs`, clase con `[TestSuite]`, cabecera con los IDs cubiertos |
| `tests/integration/` | Pruebas con escenas (`SceneRunner`) | `*Test.cs`, clase con `[TestSuite]`, simulación de input emulado |

Cada fichero de prueba debe documentar en su cabecera qué IDs de este documento cubre.

### 2.2 Lista de Chequeo de Pruebas Unitarias

| ID | Módulo A Probar | Caso de Prueba | Resultado Esperado |
|---|---|---|---|
| **UT-WS-01** | `WordSearchGenerator` | Dimensiones por dificultad | Fácil = $8\times 8$, Medio = $10\times 10$, Difícil = $12\times 12$, Maestro = $15\times 15$. |
| **UT-WS-02** | `WordSearchGenerator` | Direcciones en modo Fácil | Solo genera vectores $(1,0)$ y $(0,1)$ (sin diagonales ni reversos). |
| **UT-WS-03** | `WordSearchGenerator` | Direcciones en modo Difícil/Maestro | Permite vectores inversos $(-1,0)$, $(0,-1)$ y diagonales. |
| **UT-WS-04** | `WordSearchGenerator` | Normalización de texto | Convierte "ESPAÑA" a "ESPANA" y "ZANAHORIA" sin tildes/caracteres especiales. |
| **UT-WS-05** | `WordSearchGenerator` | Margen de tolerancia 20% | Si se piden 20 palabras en $15\times 15$ y solo caben 17, aprueba el tablero ($\ge 16$). |
| **UT-WS-06** | `ShuffleBagSystem` | Distribución de categorías | En 20 partidas de Fácil, cada uno de los 10 grupos temáticos aparece al menos 2 veces. |
| **UT-WS-07** | `WordBank` + `WordSearchTheme.validate` | Banco de palabras (spec §5 y §5.1) | Carga 10 categorías con sus ids y nombres; cada una supera `validate()` con ≥60 palabras válidas (4–15 letras A–Z, sin duplicados); ≥600 en total; ninguna palabra repetida entre categorías. |

### 2.3 Cobertura automatizada actual

| Archivo | Cubre |
|---|---|
| `tests/unit/DifficultyConfigTest.cs` | `UT-WS-01` y datos de la spec §4.2: los 4 `.tres`, mínimo del 80 %, categorías, par, encadenado de niveles, `validate()` y `MinimumFor`. |
| `tests/unit/WordSearchGeneratorTest.cs` | `UT-WS-01` a `UT-WS-05`: dimensiones, direcciones por nivel, normalización, tolerancia del 80 %, fallo controlado, solape y tiempo de 15×15 (< 0,5 s). |
| `tests/unit/WordSelectionTest.cs` | Mezcla de 1/1/2/4 categorías, palabras normalizadas y sin repetir, avance de la Shuffle Bag y respaldo sin bolsa. |
| `tests/unit/DragSelectionTest.cs` | Lógica de arrastre: bloqueo de dirección, ajuste a la línea dominante, límites del tablero, cancelación y ruta. |
| `tests/unit/ShuffleBagSystemTest.cs` | `UT-WS-06`: reparto de los 10 grupos en 20 extracciones. |
| `tests/unit/WordBankTest.cs` | `UT-WS-07`: banco completo. |
| `tests/integration/WordSearchDragTest.cs` | `IT-WS-01` a `IT-WS-03` con `SceneRunner` y ratón emulado. |
| `tests/unit/PixelCellVisualStateTest.cs` | `VT-WS-01` a `VT-WS-04`: estados visuales de PixelCell (Normal/Found), contraste de colores `#EBF2F5`/`#099C92` vs `#FFFFFF`, y herencia `WordCell : PixelCell`. |
| `tests/unit/PixelCellThemeTest.cs` | `VT-WS-03`: validación de contraste ≥ 4,5:1 con ThemeManager.OnThemeChanged cambios dinámicos. |
| `tests/integration/WordSearchWordTest.cs` | `VT-WS-05` a `VT-WS-07`: tachado BBCode `~~PALABRA~~`, transición de estados y panel de palabras HUD. |

---

## 2.4 Trazabilidad: Spec → Plan → Pruebas

Este sección mapea el cumplimiento de los requisitos desde la especificación hasta los casos de prueba, asegurando cobertura completa y sin contradicciones.

| Requisito (spec) | Fase | Casos de Prueba | Estado |
|------------------|------|-----------------|--------|
| §3.1 Estados visuales de celda | 3 | `VT-WS-01` a `VT-WS-04` | Implementado ✅ |
| §3.2 Patrones de señal/observer | 3 | `VT-WS-04` (herencia), `IT-WS-01` a `IT-WS-03` | Implementado ✅ |
| §3.3 Tachado BBCode en panel | 5 | `VT-WS-05` a `VT-WS-07`, `ST-WS-01` | Implementado ✅ |
| §4.1 Submenú de dificultad | 7 | Sin IDs propios (ver plan.md) | Pendiente de ampliar |
| Puntaje (C7) | 8 | Casos `STAT` globales | Pendiente de implementar |

**Huecos de cobertura en `test.md`:** Los casos de puntaje global y el submenú de dificultad sin IDs propios serán abordados en la Fase 5 y al cerrar la Fase 9, respectivamente. `VT-WS-03` cubre el contraste dinámico con ThemeManager, y `IT-WS-01` a `IT-WS-03` cubren la selección por arrastre con signals/observer pattern.

---

### 3. Pruebas de Integración y Módulos Visuales
| `tests/unit/PixelCellVisualStateTest.cs` | `VT-WS-01` a `VT-WS-04`: estados visuales de PixelCell (Normal/Found), contraste de colores `#EBF2F5`/`#099C92` vs `#FFFFFF`, y herencia `WordCell : PixelCell`. |
| `tests/unit/PixelCellThemeTest.cs` | `VT-WS-03`: validación de contraste ≥ 4,5:1 con ThemeManager.OnThemeChanged cambios dinámicos. |
| `tests/integration/WordSearchWordTest.cs` | `VT-WS-05` a `VT-WS-07`: tachado BBCode `~~PALABRA~~`, transición de estados y panel de palabras HUD. |

---

## 3. Pruebas de Integración y Módulos Visuales

### 3.1 Interacción Táctil y Drag
- **IT-WS-01 (Selección por Arrastre):** Simular evento de toque `InputEventScreenTouch` en celda $(0,0)$ y arrastre `InputEventScreenDrag` hasta $(0,3)$. Verificar que se emita la señal `word_selected(start_coords, end_coords)`.
- **IT-WS-02 (Selección Inválida):** Arrastrar formando una línea no recta o que no coincida con una palabra. Confirmar que el resaltado temporal se desvanece sin penalizar ni restar intentos.
- **IT-WS-03 (Solapamiento Visual):** Seleccionar una palabra que cruce sobre otra ya encontrada. La celda cruzada debe adoptar el estilo visual de la palabra recién completada.

> **Estado:** los tres casos están automatizados en
> `tests/integration/WordSearchDragTest.cs` sobre la escena real
> (`WordSearchMain.tscn`) con `ISceneRunner`: el arrastre se simula con el ratón
> emulado sobre los centros de celda calculados por el tablero, y el caso
> inválido espera 800 ms para comprobar que la celda vuelve a reposo con
> transparencia restaurada. `IT-WS-01` comprueba además que la celda encontrada
> cambia de **relleno** (turquesa `#04544d`), no solo el borde.

### 3.2 Zoom y Desplazamiento
- **IT-WS-04 (Pinch-To-Zoom):** Enviar evento multitáctil simulando 2 dedos alejándose. El contenedor `WordGridContainer` debe escalar manteniendo legibilidad de celdas.

### 3.3 Estados Visuales de PixelCell y Validación de Colores

Este subsección cubre las pruebas de validación visual del componente `PixelCell`/`WordCell`, asegurando el renderizado correcto en los estados Normal y Encontrado.

| ID | Componente | Acción | Criterio de Aprobación |
|---|---|---|---|
| **VT-WS-01** | `PixelCell` | Verificar estado Normal | Fondo `#EBF2F5`, borde `#A0BFC7`, texto `#101010` (gris oscuro) |
| **VT-WS-02** | `PixelCell` | Cambiar a estado Encontrado | Fondo `#099c92`, texto `#FFFFFF` (blanco puro), borde oculto |
| **VT-WS-03** | `PixelCell` | Validar contraste de texto | ≥ 4,5:1 para texto sobre fondo turquesa (≥ 3:1 para bordes) |
| **VT-WS-04** | `WordCell` | Herencia de `PixelCell` | `WordCell : PixelCell` compila y renderiza sin errores |

### 3.4 Tachado BBCode en Panel de Palabras

| ID | Acción | Criterio de Aprobación |
|---|---|---|
| **VT-WS-05** | Seleccionar una palabra completa | La palabra en el panel inferior muestra `~~PALABRA~~` con tachado visual |
| **VT-WS-06** | Verificar tachado después de encontrar | El texto de la palabra encontrada tiene línea central visible |
| **VT-WS-07** | Cambiar entre estados de celda | Transición fluida de Normal → Encontrado con cambio de color turquesa `#099c92` y texto blanco `#FFFFFF` |
---

## 4. Pruebas de Sistema, HUD y Progresión

| ID | Área | Descripción del Test | Criterio de Aprobación |
|---|---|---|---|
| **ST-WS-01** | Footer de Palabras | Encontrar una palabra de la lista. | La palabra se tacha visualmente y se mueve automáticamente al final de la lista inferior. |
| **ST-WS-02** | Sistema de Pistas | Activar Pista con 3 disponibles. | La palabra objetivo resplandece por 3 segundos. Contador decrementa a 2. |
| **ST-WS-03** | *Retirado* | ~~Rewarded Ads~~ | Retirado por decisión C2: la Sopa no tiene anuncios ni recarga de pistas. Ver `plan.md` §2. |
| **ST-WS-04** | Desbloqueo de Nivel | Completar la partida número 20 en dificultad Fácil. | `SaveManager` registra el progreso, aparece el modal *"¡Has desbloqueado el nivel Medio!"* y se activa el submenú. |
| **ST-WS-05** | Modal de Victoria | Encontrar todas las palabras del tablero. | Aparece el modal con animación de confeti Pixel Art, tiempo final transcurrido y botones "Siguiente Partida" / "Menú Principal". |
