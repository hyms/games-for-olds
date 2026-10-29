# 🧪 Plan de Pruebas y TDD — Sopa de Letras

> **Documento SDD de pruebas específicas para el minijuego Sopa de Letras.**
> Cobertura de pruebas unitarias, de integración, algorítmicas y de usabilidad para adultos mayores (40+).

---

## 1. Estrategia de Testing (GdUnit4 / GUT)

Siguiendo el enfoque TDD, el desarrollo del generador de tableros, selección de palabras y validación de categorías se realiza mediante pruebas unitarias automatizadas ejecutadas sin la interfaz gráfica.

---

## 2. Pruebas Unitarias Algorítmicas (Core Logic)

### 2.1 Generador de Tableros (`WordSearchGeneratorTest.gd`)

```gdscript
# Ejemplo de estructura de prueba GdUnit4 para el generador
extends GdUnitTestSuite

var generator: WordSearchGenerator

func before_test():
    generator = WordSearchGenerator.new()

func test_easy_difficulty_dimensions_and_directions():
    var config = DifficultyConfig.new(Difficulty.EASY) # 8x8, 5 palabras, solo Horiz/Vert
    var result = generator.generate_board(config, ["PERRO", "GATO", "MESA", "SILLA", "LIMA"])
    
    assert_int(result.grid.size()).is_equal(8)
    assert_int(result.grid[0].size()).is_equal(8)
    assert_bool(result.placed_words.size() >= 4).is_true() # Cumple tolerancia min 80%

func test_overlap_validity():
    # Verifica que al solapar palabras compartan exactamente la misma letra
    var grid = generator.create_empty_grid(10, 10)
    var success = generator.try_place_word("CASA", Vector2i(0,0), Vector2i(1,0), grid)
    assert_bool(success).is_true()
```

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

---

## 3. Pruebas de Integración y Módulos Visuales

### 3.1 Interacción Táctil y Drag
- **IT-WS-01 (Selección por Arrastre):** Simular evento de toque `InputEventScreenTouch` en celda $(0,0)$ y arrastre `InputEventScreenDrag` hasta $(0,3)$. Verificar que se emita la señal `word_selected(start_coords, end_coords)`.
- **IT-WS-02 (Selección Inválida):** Arrastrar formando una línea no recta o que no coincida con una palabra. Confirmar que el resaltado temporal se desvanece sin penalizar ni restar intentos.
- **IT-WS-03 (Solapamiento Visual):** Seleccionar una palabra que cruce sobre otra ya encontrada. La celda cruzada debe adoptar el estilo visual de la palabra recién completada.

### 3.2 Zoom y Desplazamiento
- **IT-WS-04 (Pinch-To-Zoom):** Enviar evento multitáctil simulando 2 dedos alejándose. El contenedor `WordGridContainer` debe escalar manteniendo legibilidad de celdas.

---

## 4. Pruebas de Sistema, HUD y Progresión

| ID | Área | Descripción del Test | Criterio de Aprobación |
|---|---|---|---|
| **ST-WS-01** | Footer de Palabras | Encontrar una palabra de la lista. | La palabra se tacha visualmente y se mueve automáticamente al final de la lista inferior. |
| **ST-WS-02** | Sistema de Pistas | Activar Pista con 3 disponibles. | La palabra objetivo resplandece por 3 segundos. Contador decrementa a 2. |
| **ST-WS-03** | *Retirado* | ~~Rewarded Ads~~ | Retirado por decisión C2: la Sopa no tiene anuncios ni recarga de pistas. Ver `plan.md` §2. |
| **ST-WS-04** | Desbloqueo de Nivel | Completar la partida número 20 en dificultad Fácil. | `SaveManager` registra el progreso, aparece el modal *"¡Has desbloqueado el nivel Medio!"* y se activa el submenú. |
| **ST-WS-05** | Modal de Victoria | Encontrar todas las palabras del tablero. | Aparece el modal con animación de confeti Pixel Art, tiempo final transcurrido y botones "Siguiente Partida" / "Menú Principal". |
