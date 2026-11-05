# 🔤 Especificación Técnica — Sopa de Letras

> **Documento SDD** del minijuego *Sopa de Letras*.
> Especifica las reglas visuales, mecánicas y la arquitectura orientada a C# y Godot 4.

---

## 1. Layout y Ajustes de Pantalla

* **Área Superior (Header):**
  * Banner principal con título "SOPA DE LETRAS" en `#A0BFC7` con bordes pixel art.
  * Sub-banner "FÁCIL - NIVEL 1" encajado con colores inversos.
  * Respetar el margen superior de **48 px** para librar la cámara/notch de dispositivos Android.

* **HUD de Control:**
  * Botones de "Atrás" y "Pausa" alineados horizontalmente con el temporizador (`00:10`) y el contador de progreso (`0/5 Palabras`).
  * Sin duplicaciones de iconos de texto como `(<-)`.

* **Tablero Central:**
  * Celdas independientes con formato de botón pixel art en tono claro (`#EBF2F5`).
  * **Sin recuadro contenedor gris exterior** que sature la pantalla.
  * Al acertar una palabra (ej: `ACORDEON` / `TARA`), las celdas adoptan el color turquesa (`#099c92`) con la letra en **blanco puro (`#FFFFFF`)**.

* **Panel Inferior (Lista de Palabras):**
  * Contenedor con borde pixel art limpio.
  * Texto claro en *AtkinsonHyperlegible*. Palabras separadas por `|`.
  * Las palabras encontradas muestran una línea de tachado central (`~~GUITARRA~~` / `~~ACORDEON~~`).

---

## 2. Arquitectura de C# y Patrones

* **`WordGridFactory.cs` (Factory Pattern):** Encargado de instanciar y posicionar las celdas del tablero usando `PixelCell` como base reutilizable.
* **`PixelCell.cs`:** Componente base para celdas de grilla con estilo pixel art puro (sin anti-aliasing, `corner_radius = 0`). Hereda de `Panel` y soporta estados visuales (Normal, Seleccionado, Encontrado) mediante `StyleBoxFlat` y modulación de color dinámica a través de `ThemeManager.OnThemeChanged`.
* **`WordCell.cs`:** Hereda de `PixelCell` y añade lógica específica de Sopa de Letras (coordenadas, letra, seguimiento de palabra encontrada, `WordCellState`). Aplica colores turquesa `#099c92` y texto blanco `#FFFFFF` al estado Encontrado.
* **`WordSearchMain.cs`:** Controla la lógica de selección por arrastre, emite eventos (`event Action OnWordFound`) y gestiona la transición de estados en `PixelCell`/`WordCell`.