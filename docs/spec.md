# Especificación General — Clásicos Mente Activa 2D

> **Documento SDD global del producto.**
>
> Describe la visión, los objetivos generales, los requisitos transversales, la **monetización por anuncios**, el **sistema de guardar estadísticas**, las **opciones de accesibilidad de pantalla/resolución** y la **arquitectura técnica de software (Godot 4 en C#)**.
>
> Cada minijuego extiende esta arquitectura mediante especificaciones detalladas en su carpeta respectiva (`docs/minigames/<juego>/spec.md`).

---

## 1. Visión del Producto

**Clásicos Mente Activa 2D** es una colección de minijuegos de agilidad mental orientada a adultos de 40 años en adelante, con foco prioritario en personas mayores de 60 años y usuarios con baja alfabetización digital.

**Propuesta de valor:** Juegos conocidos, altamente legibles, sin frustración y sin castigos por tiempo. El usuario debe poder abrir la app, ajustar la accesibilidad si lo requiere, seleccionar un minijuego y comenzar a jugar en menos de 30 segundos, sin registro ni necesidad de conexión a internet obligatoria.

---

## 2. Principios de Arquitectura Técnica (C# + Godot 4)

Para garantizar un código limpio, desacoplado y mantenible que no genere bucles infinitos en el agente de IA, se establecen los siguientes principios de arquitectura compatibles con Godot 4 en C#:

### 2.1 Principios SOLID Aplicables (S & O)
1. **Single Responsibility (S):** Cada script o nodo de C# tiene una responsabilidad única. Las escenas UI solo manejan la presentación visual; la lógica de negocio y generación se delega a clases puras o administradores independientes.
2. **Open/Closed (O):** Extensión mediante comportamientos configurables mediante `Resource` o estrategias sin modificar clases centrales.
3. **Restricción de Interfaces Complejas (Evitar L & I excesivos):** En Godot 4 C#, se evita el uso abusivo de jerarquías de interfaces e inyección de dependencias pesadas que choquen con la jerarquía nativa de `Node`. Se prioriza la composición de nodos nativos.

### 2.2 Patrones de Diseño Soportados
* **Factory Pattern (`WordGridFactory` / `CellFactory`):** Creación e instanciación dinámica de celdas y componentes de tablero desacoplados de la UI.
* **Observer / Signals Pattern (`Action` / `event` / Godot Signals):** Comunicación reactiva para eventos (palabra encontrada, tiempo transcurrido, cambio de puntuación).
* **Tween / Game Juice Animations:** Uso de `CreateTween()` para micro-animaciones en selección, acierto y tachado de palabras.

---

## 3. Requisitos de UI / UX y Accesibilidad Visual (Pixel Art Moderno)

* **Fondo Global Limpio:** Blanco puro (`#FFFFFF`) para máximo contraste.
* **Margen de Seguridad de Pantalla (Notch/Safe Area):** `MarginContainer` raíz obligatorio con margen superior mínimo de **48 px** y laterales de **16 px**.
* **Layout Compacto Vertical:** Eliminación de espacios muertos entre la cabecera del juego, el tablero y el panel inferior.
* **Paleta de Colores de Interfaz:**
  * Fondo General: `#FFFFFF`
  * Paneles de Cabecera y Título: `#A0BFC7` / `#6C95A1`
  * Celdas en Reposo: `#EBF2F5` con contorno de píxel `#A0BFC7` / `#477987` y texto en negro/gris oscuro.
  * Celdas Encontradas / Seleccionadas: `#099c92` con texto en **blanco puro (`#FFFFFF`)**.
  * Palabras Encontradas (Panel Inferior): Tachado visual (`~~PALABRA~~`).

---

## 4. Estructura del Repositorio

```text
res://
├── core/                           # Autoloads y Managers C#
│   ├── GameManager.cs              # Estado global y flujo
│   ├── SaveManager.cs              # Guardado local JSON
│   ├── ThemeManager.cs             # Paleta de colores dinámicos
│   └── AudioManager.cs             # Control de sonidos SFX/BGM
├── assets/
│   ├── fonts/                      # PressStart2P & AtkinsonHyperlegible
│   └── audio/                      # Efectos retro de selección y victoria
├── minigames/
│   └── word_search/                # Módulo Sopa de Letras (C1: ruta raíz, no scenes/)
│       ├── WordSearchMain.tscn
│       ├── WordSearchMain.cs
│       ├── WordGridFactory.cs
│       └── components/             # PixelCell, PixelFooterPanel
```