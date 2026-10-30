# 🔤 Especificación Técnica — Sopa de Letras

> **Documento SDD** del minijuego *Sopa de Letras* de **Clásicos Mente Activa 2D**.
> Especifica las reglas, mecánicas, dificultades, combinación de categorías y arquitectura técnica.
>
> Ubicación en repositorio: `minigames/word_search/` (decisión C1; la ruta correcta según D1)

---

## 1. Visión y Propuesta de Valor

Ofrecer una experiencia de búsqueda de palabras relajante, altamente accesible e intuitiva para adultos mayores de 40 años (con foco en 60+).
- **Entrada limpia:** Sin registro, sin pantallas confusas.
- **Sin frustración:** Operación fluida mediante gestos táctiles sencillos, margen de error amplio y sin cronómetros ni derrotas punitivas.
- **Variedad infinita offline:** Selección aleatoria de palabras sin repetición mediante un banco local amplio (*Shuffle Bag Algorithm*).

---

## 2. Objetivos y Métricas de Éxito

| # | Objetivo | Criterio de Logro |
|---|----------|-------------------|
| O1 | Autonomía Inicial | Un adulto 40+ comprende la mecánica y selecciona su primera palabra en $\le 20\text{ s}$. |
| O2 | Duración de Partida | Sesiones ágiles adaptadas por dificultad (Fácil: 2-4 min, Maestro: 8-12 min). |
| O3 | Accesibilidad Visual | Texto de celdas con alto contraste; soporte para pinch-to-zoom táctil de 2 dedos. |
| O4 | Rendimiento | 60 fps estables en Android gama baja con renderizado fluido de grilla. |

---

## 3. Arquitectura y Mecánicas de Juego

### 3.1 Interacción Táctil y Selección
- **Gestión por Arrastre (Drag Only):** El jugador presiona una celda inicial y arrastra el dedo en línea recta (horizontal, vertical o diagonal).
- **Resaltado en Tiempo Real:** Un trazo retro semitransparente sigue la trayectoria activa del dedo.
- **Validación de Línea Recta:** Al soltar el dedo, se evalúa si el segmento cubre una palabra pendiente.
- **Solapamiento de Palabras (Overlap):** Si dos palabras se cruzan compartiendo letras, prevalece el efecto/color visual de la **última palabra encontrada**.

### 3.2 Zoom y Adaptabilidad de Pantalla
- **Resolución Estándar de Referencia:** $375 \times 812$ (baseline moderna tipo iPhone X / Android 19.5:9).
- **Vista Completa Inicial:** El tablero encaja al 100 % en el área activa por defecto.
- **Pinch-to-Zoom:** Gestos de 2 dedos permiten ampliar el tablero con desplazamiento lateral suave para maximizar la legibilidad en pantallas reducidas.

### 3.3 HUD Superior y Menú de Pausa
- **HUD Superior Visibles:**
  1. Botón de Menú/Pausa.
  2. Contador de palabras restantes (ej. "Palabras: 4/5").
  3. Tiempo transcurrido (medidor no punitivo).
- **Menú Modal de Pausa:**
  1. Botón "Ayuda / Pista" con contador de usos disponibles (configurable desde `SaveManager.cs`).
  2. Botón "Reiniciar Partida".
  3. Botón "Salir al Menú Principal".

### 3.4 Sistema de Pistas (3 por nivel, sin anuncios)
- **Visualización:** Al presionar Pista, la palabra completa resplandece temporalmente con un efecto de animación por unos segundos.
- **Límite:** Máximo **3 pistas** por nivel, reiniciadas al empezar o reiniciar la partida.
- **Sin recarga publicitaria:** no hay anuncio bonificado ni dependencia de conexión (decisión C2). Al agotarse, el botón queda inactivo con el texto *"Sin pistas (se recuperan al reiniciar)"*.

### 3.5 Pantalla de Victoria
- Muestra modal centrado con animación Pixel Art (estrellas/confeti).
- Resume el tiempo tomado y dificultad superada.
- Ofrece botones de acción directa: **"Siguiente Partida"** y **"Menú Principal"**.

---

## 4. Dificultades, Progresión y Flujo de Pantallas

### 4.1 Submenú de Selección de Dificultad
Previo al tablero, el juego cuenta con un submenú dedicado que muestra los niveles con su estado de candado y progreso acumulado.

### 4.2 Niveles de Dificultad y Matriz de Reglas

| Nivel | Tamaño Tablero | Palabras Objetivo | Tolerancia Generación | Direcciones Permitidas | Combinación de Categorías |
|---|---|---|---|---|---|
| **Fácil** | $8 \times 8$ | 5 | $\ge 4$ (mín. 80%) | Horizontal y Vertical (Izquierda $\to$ Derecha, Arriba $\to$ Abajo) | 1 Grupo Temático por partida |
| **Medio** | $10 \times 10$ | 8 | $\ge 7$ (mín. 80%) | Agrega Diagonales directas | 1 Grupo Temático por partida |
| **Difícil** | $12 \times 12$ | 12 | $\ge 10$ (mín. 80%) | Agrega Direcciones Inversas (Derecha $\to$ Izquierda, Abajo $\to$ Arriba) | Mezcla de **2 Grupos Temáticos** |
| **Maestro** | $15 \times 15$ | 20 | $\ge 16$ (mín. 80%) | Matriz completa de 8 direcciones | Mezcla de **4 Grupos Temáticos** |

*Nota sobre la Tolerancia de Generación (Fallback 20%):* Si tras $X$ reintentos el algoritmo de inserción no logra colocar el 100% de las palabras, se permite iniciar la partida si alcanza al menos el 80% de la meta para evitar bloqueos del sistema.

### 4.3 Reglas de Desbloqueo
- **Fácil y Medio:** Requieren completar **20 partidas** en la dificultad previa (10 grupos temáticos $\times 2$ repeticiones mínimas).
- **Difícil y Maestro:** Requieren completar **20 partidas** en la dificultad anterior.
- **Modal de Logro:** Al alcanzar la partida 20, se despliega un mensaje modal *"¡Has desbloqueado el nivel [XXXX]!"*.

---

## 5. Banco de Palabras Offline y Algoritmo Shuffle Bag

Para asegurar la sensación de variedad infinita sin depender de internet:
- **Banco de Palabras Local (`user://word_bank.json` o Resource):** Contiene 10 categorías temáticas con un mínimo de 50 a 100 palabras por categoría (total $\ge 600$ palabras).
- **Mecanismo Shuffle Bag:** En modos **Fácil** y **Medio**, se llena una bolsa aleatoria sin repetición con los 10 grupos. Se asegura que cada grupo se juegue al menos 2 veces en 20 partidas antes de reiniciar la bolsa.
- **Footer Dinámico de Lista de Palabras:** Las palabras a buscar se listan en el footer inferior en orden aleatorio. Conforme son encontradas y tachadas, se desplazan automáticamente al final de la lista.

### 5.1 Las 10 Categorías Temáticas (40+ Friendly)
1. **Animales y Naturaleza** (PERRO, GATO, DELFIN, ELEFANTE...)
2. **Plantas y Flores** (ROSA, GIRASOL, ORQUIDEA, PINO...)
3. **Frutas y Verduras** (MANZANA, PLATANO, ZANAHORIA, MANGO...)
4. **Cosas del Hogar** (MESA, SILLA, CUCHARA, LAMPARA...)
5. **Profesiones y Oficios** (MEDICO, CARPINTERO, MAESTRO, FARMACEUTICO...)
6. **Países y Geografía** (MEXICO, ARGENTINA, ESPANA, LIMA...)
7. **Comidas y Postres** (PASTEL, PAELLA, CHOCOLATE, EMPANADA...)
8. **Instrumentos Musicales** (PIANO, GUITARRA, FLAUTA, VIOLIN...)
9. **Herramientas y Bricolaje** (MARTILLO, ALICATE, TALADRO, SIERRA...)
10. **Ropa y Costura** (CAMISA, ZAPATO, BUFANDA, VESTIDO...)

---

## 6. Arquitectura de Software (Godot 4)

El minijuego aplica los patrones del núcleo general:

1. **Strategy Pattern (`WordSearchGenerator.cs`):** Recibe la `DifficultyConfig` y genera la matriz sin acoplar la UI.
2. **Composite Pattern (`WordGridContainer.cs`):** Administra la jerarquía de celdas (`WordCell.cs`), permitiendo zoom y escalado fluido.
3. **Template Method (`WordSearchMain.cs`):** Hereda de `BaseMinigame.cs` para reutilizar pausado, registro de estadísticas y ciclo de vida (sin interfaz de anuncios: la Sopa no muestra publicidad, C2).
4. **Datos de dificultad (`DifficultyConfig`):** un recurso `.tres` por nivel en `resources/difficulties/`, cargados por `DifficultyCatalog`. El generador y el HUD leen de ahí; no hay condicionales por número de nivel.
5. **Selección de palabras (`WordSelection` + `WordSearchSession`):** la Shuffle Bag reparte los grupos según `category_count` y de ellos salen las palabras de la partida; la sesión guarda el nivel elegido entre el selector y el tablero (en memoria hasta que `SaveManager` persista en la Fase 7).
6. **Selección por arrastre (`DragSelection`):** lógica pura (inicio, bloqueo de dirección, ajuste a la línea dominante, límites) separada del contenedor, que solo traduce entrada y pinta el trazo (`WordTraceOverlay`).
