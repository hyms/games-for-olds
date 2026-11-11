# Dirección de Arte — Clásicos Mente Activa 2D (v0.5)

> **Documento SDD global de estilo visual, librería de componentes y temas dinámicos.**
> Define las reglas de diseño para la UI en Pixel Art adaptada a dispositivos móviles con notch, pantallas OLED/LCD y soporte para personalización de colores global.

---

## 1. Reglas Generales de Pantalla y Arte Retro

* **Regla de Oro (Pixel Art Puro):** Todo elemento visual (UI, botones, iconos, contenedores, tipografías y efectos) debe dibujarse mediante sprites pixel art o `StyleBoxFlat` sin suavizado de bordes (*anti-aliasing* desactivado).
* **Fondo Principal:** Base clara adaptable por tema (`#FFFFFF` por defecto).
* **Safe Area (Notch Support):** Todo layout de escena debe envolverse en un `MarginContainer` con:
  * `margin_top = 48`
  * `margin_bottom = 16`
  * `margin_left = 16`
  * `margin_right = 16`
* **Texturas y Filtros:** Texturas con `Texture Filter = Nearest` sin anti-aliasing ni mipmaps en importación y nodos de Godot.

---

## 2. Sistema de Temas Dinámicos de Color (ThemeManager Architecture)

Para permitir que el usuario cambie el aspecto visual de la aplicación completa (ej. Tema Clásico, Tema Noche / Dark, Tema Pastel, Tema Retro GameBoy), la paleta de colores se abstrae en recursos de datos y tokens globales.

### 2.1 Estructura de Data Theme (`ThemeData.cs`)

Cada tema es una clase pura o `Resource` que define los siguientes tokens de color:

```csharp
public class ThemeData
{
    public string ThemeId { get; set; } = "classic";
    public Color BackgroundColor { get; set; } = Color.FromHtml("#FFFFFF");
    public Color HeaderBannerColor { get; set; } = Color.FromHtml("#A0BFC7");
    public Color SubheaderBannerColor { get; set; } = Color.FromHtml("#477987");
    public Color CellIdleBackgroundColor { get; set; } = Color.FromHtml("#EBF2F5");
    public Color CellIdleBorderColor { get; set; } = Color.FromHtml("#A0BFC7");
    public Color CellIdleTextColor { get; set; } = Color.FromHtml("#101010");
    public Color CellFoundBackgroundColor { get; set; } = Color.FromHtml("#099C92");
    public Color CellFoundTextColor { get; set; } = Color.FromHtml("#FFFFFF");
    public Color FooterTextColor { get; set; } = Color.FromHtml("#477987");
    public Color FooterStrikethroughColor { get; set; } = Color.FromHtml("#099C92");
}
```

### 2.2 Notificación Replay / Inyección de Temas
El `ThemeManager.cs` (Autoload / Singleton) emite un evento `event Action<ThemeData> OnThemeChanged` al seleccionar una paleta nueva. Todas las celdas, botones y banners se suscriben para actualizar su `StyleBoxFlat` o `SelfModulate` en tiempo real.

---

## 3. Librería Reutilizable de Componentes Pixel Art (`res://scenes/ui/components/`)

Todos los minijuegos deben reutilizar los siguientes componentes prefabricados:

| Componente | Ruta de la Escena | Descripción y Función Visual |
|---|---|---|
| **PixelButton** | `res://scenes/ui/components/PixelButton.cs` | Botón táctil universal (`BigButton.tscn` lo hereda). Borde de píxel de 2 px, bisel duro de 1 px y hundimiento de 2 px al pulsar. Estilos centralizados en `core/ui/PixelButtonStyle.cs`. |
| **PixelBanner** | `res://scenes/ui/components/PixelBanner.tscn` | Panel de título o sub-banner personalizable con marco estilo retro de 3 píxeles de grosor. |
| **PixelCell** | `res://scenes/ui/components/PixelCell.cs` | Celda interactiva reutilizable para Sopa de Letras, Tangram o grillas de lógica con estados `Idle`, `Hover/Selected` y `Completed`. Estilos en `core/ui/PixelCellStyle.cs`. |
| **PixelHeaderHUD** | `res://scenes/ui/components/PixelHeaderHUD.tscn` | Barra de navegación superior estandarizada (Atrás, Pausa, Cronómetro y Progreso). |
| **WordsPanel** | `res://minigames/word_search/WordsPanel.cs` | Panel inferior de la Sopa de Letras: lista de palabras con divisores pixelados (`\|`), tachado turquesa y confeti. Marco de 2 px, sombra dura, esquinas rectas y márgenes internos de 12/8 px. |

### 3.1 Botones con relieve 3D

Todos los botones derivan de `PixelButton` y comparten una única fábrica, `PixelButtonStyle.For(variant, state, theme)`. No se permiten estilos locales por escena.

| Variante | Uso | Tipografía | Tamaño | Márgenes (X/Y) |
|---|---|---|---|---|
| `Hud` | Atrás / Pausa del minijuego | VCR OSD Mono | 16 px | 10 / 10 |
| `Action` | Acciones de modales (Continuar, Pista…) | VCR OSD Mono | 18 px | 14 / 12 |
| `Menu` | Menú principal y dificultad | Atkinson Hyperlegible | 20 px | 16 / 12 |

| Estado | Relleno | Texto/icono | Relieve | Sombra dura | Contenido |
|---|---|---|---|---|---|
| Normal | `HeaderBannerColor` (#A0BFC7) | `TextColor(fill)` → #101010 | Elevado | Sí, (2,2) | Base |
| Hover | `SubheaderBannerColor` (#477987) | Blanco | Hundido 1 px | No | Sin desplazar |
| Pressed | `CellFoundBackgroundColor` oscurecido 30 % | Blanco | Hundido 1 px | No | +2 px abajo |
| Focus | Anillo exterior oscuro, centro transparente | Igual que normal | Elevado | La del normal | Sin desplazar |
| Disabled | `Arena` (#B0C2CC) | #101010 | Plano | No | Base |

Reglas fijas: `CornerRadius = 0`, `AntiAliasing = false`, borde exterior de 2 px, bisel interior de 1 px (luz `#FFFFFF` arriba/izquierda, sombra `#535456` abajo/derecha), `Alpha = 1.0` y `icon_max_width = 16` (los PNG de 32 px son un 2× exacto). El desplazamiento del estado pulsado conserva la altura total: `top + 2`, `bottom − 2`.


---

## 4. Tipografías Oficiales

Jerarquía tipográfica de la Sopa de Letras (`docs/word_search/spec.md` §1, `assets/fonts/README.md`):

* **Títulos, banners y modales:** `PressStart2P.ttf` (spec §1.1)
* **HUD, cronómetro, contadores y botones:** `VCR_OSD_Mono.ttf` (spec §1.2)
* **Celdas del tablero, matriz y lista de palabras pendientes:** `PixelOperator.ttf` (spec §1.3)
* **Interfaz accesible global:** `AtkinsonHyperlegible-Regular.ttf`

---

## 5. Micro-animaciones de Retroalimentación (*Game Juice*)

1. **Selección de Celda:** Escalado elástico suave (`1.0 -> 1.06 -> 1.0`) mediante `CreateTween()`.
2. **Acierto de Palabra:** Destello de color de acento (`CellFoundBackgroundColor`) en las celdas coincidentes con un micro-rebote y efecto de tachado en el footer.
3. **Pulsación de Botones UI:** Desplazamiento vertical de 2 píxeles hacia abajo al presionar para simular la pulsación física de un botón de consola clásica.