# 🔤 Especificación Técnica y Arquitectura — Sopa de Letras (SDD-WS-01 v3.0)

> **Módulo:** Minijuego Sopa de Letras
>
> **Motor:** Godot 4.x (.NET / C#)
>
> **Patrones de Diseño:** Factory, Singleton, Strategy, Observer.
>
> **Principios:** SRP, OCP, DRY, KISS, YAGNI, Bajo Acoplamiento.

## 1. Tipografías Oficiales del Proyecto

Para mantener la jerarquía visual y la coherencia pixel art, se utilizan tres fuentes retro específicas:

1. **`PressStart2P.ttf`**: Títulos principales, banners de cabecera (`PixelBanner`), modales de victoria y números destacados del temporizador.

2. **`VCR_OSD_Mono.ttf`**: HUD de control, cronómetro retro (`00:50`), contador de progreso (`1/5 Palabras`), botones de interfaz (`Atrás`, `Pausa`, `Pistas`) y mensajes estilo pantalla CRT/arcade.

3. **`PixelOperator.ttf`** (o `PixelOperatorMono.ttf`): Celdas del tablero (`PixelCell`), matriz de letras y lista de palabras pendientes en el panel inferior (`WordsPanel`), garantizando máxima legibilidad en píxel art.

## 2. Directrices Visuales y Regla de Oro (Pixel Art Puro)

* **Cero Suavizado:** Todos los nodos y componentes (`StyleBoxFlat`, fuentes, bordes) deben configurarse con `AntiAliasing = false` y `CornerRadius = 0`.

* **Opacidad Absoluta (Anti-Fantasma):** Prohibido el uso de canales alfa reducidos (`< 1.0`) o modulations translucidas en celdas encontradas o activas. Los fondos y textos son 100% sólidos.

* **Tokens de Color Oficiales (`ThemeData`):**

  * **Fondo Global / Celdas Reposo:** `#EBF2F5` (`CellIdleBackgroundColor`)

  * **Bordes Finos de Grilla:** `#A0BFC7` (`CellIdleBorderColor`, 2px solid)

  * **Texto Oscuro Base:** `#101010` (`TextDark`) con `PixelOperator`

  * **Acento de Acierto / Selección:** `#099C92` (Turquesa sólido, Alfa 1.0)

  * **Texto de Acierto / Selección:** `#FFFFFF` (Blanco puro, Alfa 1.0, sin contornos translucidos)

  * **Subtítulo / Banners Secundarios:** `#6C95A1`

  * **Pista / Ayuda Activa:** Fondo `#F9881E` (Naranja retro), Borde `#8A4600`, Texto `#101010` (contraste 6,9:1).

### 2.1 Botones del HUD y Modales (PixelButton)

Todos los botones del minijuego usan `BigButton : PixelButton` y la fábrica `PixelButtonStyle.For(...)`; las escenas no declaran fuentes, tamaños ni colores propios.

* **Variantes:** `Hud` (Atrás/Pausa, VCR OSD Mono 16 px), `Action` (botones de modales, VCR 18 px), `Menu` (selector de dificultad y menú principal, Atkinson 20 px).

* **Geometría:** borde exterior de 2 px, bisel interior de 1 px, `CornerRadius = 0`, `AntiAliasing = false`, alfa 1.0 y sombra dura solo en reposo/foco.

* **Estados:** reposo elevado; hover y pulsado invierten el bisel (hundido); el pulsado además baja el contenido 2 px sin cambiar la altura total; foco es un anillo exterior de 2 px; deshabilitado es plano en `Arena` con texto `#101010`.

* **Contraste:** cada estado calcula el texto/icono con `PixelStyles.TextColor(fill)`; el tema clásico entrega #101010 sobre #A0BFC7 y blanco sobre #477987 (≥ 4,5:1).


## 3. Especificación de los 4 Niveles de Dificultad

### 3.1 Nivel 1: Fácil

* **Dimensiones de Grilla:** $8 \times 8$ celdas.

* **Cantidad de Palabras:** 5 palabras por tablero.

* **Direcciones Permitidas:** Horizontal (izquierda a derecha) y Vertical (arriba hacia abajo).

* **Pistas / Ayudas:** 3 pistas por partida.

### 3.2 Nivel 2: Medio

* **Dimensiones de Grilla:** $10 \times 10$ celdas.

* **Cantidad de Palabras:** 8 palabras por tablero.

* **Direcciones Permitidas:** Horizontal, Vertical y Diagonal descendente.

* **Pistas / Ayudas:** 3 pistas por partida.

### 3.3 Nivel 3: Difícil

* **Dimensiones de Grilla:** $12 \times 12$ celdas.

* **Cantidad de Palabras:** 12 palabras por tablero.

* **Direcciones Permitidas:** Horizontal, Vertical, Diagonal (ambas orientaciones) e Inversas (derecha a izquierda, abajo hacia arriba).

* **Pistas / Ayudas:** 3 pistas por partida.

### 3.4 Nivel 4: Maestro

* **Dimensiones de Grilla:** $15 \times 15$ celdas.

* **Cantidad de Palabras:** 20 palabras por tablero.

* **Direcciones Permitidas:** Las 8 direcciones (horizontal, vertical, diagonales y sus inversas).

* **Pistas / Ayudas:** 3 pistas por partida.

## 4. Reglas de Juego, Sistema de Pistas y Progresión

### 4.1 Progresión y Criterios de Victoria (20 Nivel / 2 Categoría)

* **Criterio de Desbloqueo de Nivel:** Se requieren **20 partidas ganadas por nivel** para desbloquear el siguiente (C6). Fácil está abierto por defecto; Medio tras 20 de Fácil; Difícil tras 20 de Medio; Maestro tras 20 de Difícil. Se cuentan solo victorias, no intentos abandonados.

* **Distribución por Categorías:** Los grupos temáticos se reparten con la Shuffle Bag según `category_count` de cada dificultad (1/1/2/4 · C3 y §5).

### 4.2 Sistema de Pistas / Ayudas (3 por Partida)

* Cada partida otorga exactamente **3 Pistas / Ayudas**.

* **Mecánica:** Al presionar el botón de pista en el HUD (`PixelButton` con `VCR OSD Mono`), la primera letra no descubierta de una palabra pendiente parpadea en naranja (`#F9881E`) durante 1.5 segundos.

* Al llegar a `0/3`, el botón se deshabilita visualmente.

### 4.3 Celebración y Game Juice (Lluvia de Confeti)

* Al encontrar la última palabra del tablero (`20/20` o final de partida), el cronómetro se detiene y se activa la lluvia de confeti pixel art (`PixelConfettiEmitter` con `CpuParticles2D` en cuadrados de 4x4 o 6x6 píxeles).

* Transcurridos 300 ms, un modal elástico (`LevelCompleteModal`) muestra el tiempo total, pistas usadas y el progreso hacia las 20 victorias.

* **Paleta de confeti (v0.5.0):** los colores proceden **exclusivamente de tokens de la paleta global** — `Turquesa` (#099C92), `Amarillo` (#FDC109), `Cian` (#08D0DF) y `CelebrationRed` (#B45252, token agregado en v0.5.0). No se usan valores hex ad hoc. Ver [`docs/word_search/art_direction.md`](art_direction.md) §8.1.

* **Regla de disparo:** confeti **solo en victoria**. Las palabras individuales encontradas reciben animación sutil de celda y tachado en el footer (`~~PALABRA~~`), no confeti.

## 5. Arquitectura de Software y Patrones de Diseño (C# / Godot 4)

La arquitectura sigue estrictamente los principios **SOLID**, **KISS**, **YAGNI**, **DRY** y **Bajo Acoplamiento**:

1. **Singleton Pattern (`ThemeManager.cs`):**

   * Instancia única accesible globalmente (`/root/ThemeManager`) que administra los datos de `ThemeData` y notifica cambios a la UI mediante eventos.

2. **Factory Pattern (`WordGridFactory.cs`):**

   * Responsable exclusivo de instanciar y ubicar espacialmente los nodos `PixelCell` en la grilla según la dimensión del nivel (SRP).

3. **Strategy Pattern (`IWordPlacementStrategy.cs`):**

   * Desacopla los algoritmos de colocación de palabras en la grilla (`HorizontalVerticalPlacement`, `DiagonalPlacement`, `ReversePlacement`). Permite cambiar o agregar nuevas reglas sin modificar el código existente (OCP).

4. **Observer Pattern (`event Action`):**

   * `WordSearchMain` emite eventos desacoplados (`OnWordFound`, `OnLevelCompleted`, `OnHintUsed`) para que los componentes visuales, sonidos y métricas reaccionen sin acoplamiento directo.

5. **Principios de Diseño:**

   * **SRP (Single Responsibility):** `PixelCell` solo renderiza su estado visual; `WordSearchMain` coordina reglas; `WordGridFactory` construye el tablero.

   * **OCP (Open/Closed):** Nuevos temas o estrategias de colocación se añaden creando clases/recursos sin alterar el flujo principal.

   * **DRY & KISS:** Estilos centralizados en `PixelStyles.cs`, sin estructuras sobre-diseñadas.

   * **YAGNI:** Implementar estrictamente lo necesario para el minijuego sin abstracciones innecesarias.