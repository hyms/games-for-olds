# Dirección de Arte — Clásicos Mente Activa 2D (v0.3)

> **Documento SDD global** de estilo visual.
> Extraído de la plantilla y requerimientos visuales aprobados por el usuario. Se aplica a todos los
> minijuegos y pantallas, empezando por la Sopa de Letras.
>
> **Versión:** 0.3 · **Fecha:** 31/10/2026 · **Estado:** Sincronizado con paleta definitiva e intento visual 2
> **Documentación relacionada:** [Spec global](spec.md) · [Plan global](plan.md) ·
> [Plan de la Sopa](word_search/plan.md) ·
> [Manifiesto de assets](../assets/asset-manifest.json)
>
> **Plantilla de referencia:** `docs/word_search/wathersoup.jpg`.

---

## 1. Marco técnico

- **Motor:** Godot 4.x, 2D, renderizado `mobile`.
- **Viewport:** 360×640 lógicos, estirado `canvas_items`, orientación vertical con soporte estricto para áreas de seguridad (*Safe Area* / *Notch*).
- **Plataforma:** Android (gama media/baja), escritorio para desarrollo.
- **Escala de la plantilla:** La imagen mide 574×1024 px y equivale a nuestro viewport 360×640. **Factor de conversión: ÷ 1,594** (1 px de plantilla = 0,627 px lógicos).
- **Filtrado:** Vecino más cercano (nearest), sin mipmaps (`default_texture_filter=0`).

---

## 2. Sistema visual

### 2.1 Lenguaje de formas

- Esquinas redondeadas pequeñas y consistentes (radio ≈ 5 px lógicos).
- Bordes limpios de 2 px lógicos en tono oscuro (`#4e6077` o `#535456`) para garantizar alto contraste y delimitación clara frente al fondo marfil/blanco.
- Sombra dura y pequeña (2 px lógicos) sin difuminado.
- Contornos oscuros y contundentes; sin degradados ni efectos de profundidad complejos.
- **Sin marco contenedor:** El tablero de celdas no lleva borde ni recuadro exterior envolvente; flota libremente sobre el fondo global.

### 2.2 Paleta de Colores Oficial (Tokens de Tema)

| Token / Rol | Hex | Uso en UI |
|-------------|-----|-----------|
| Fondo global (`background`) | `#fffcfb` | Color de fondo principal para toda la aplicación y pantallas |
| Primario (`primary`) | `#b0c2cc` | Celdas del tablero en reposo, bordes de marcos y botones estándar |
| Bloqueado / Inactivo (`blocked`) | `#efefee` | Estados inactivos, elementos deshabilitados o celdas secundarias |
| Secundario (`secondary`) | `#4e6077` | Celdas seleccionadas, textos de alto contraste, acentos y bordes principales |
| Tinta de texto oscuro (`text_dark`) | `#101010` | Letras sobre celdas en reposo y fondos claros |
| Tinta de texto claro (`text_light`) | `#fffcfb` | Letras sobre fondos oscuros (ej. celdas seleccionadas en `#4e6077`) |
| Amarillo acento | `#fdc109` | Acciones mágicas (pistas) |
| Turquesa acento | `#099c92` | Confirmaciones y superficies de éxito |
| Naranja pista | `#f9881e` | Animación y resaltado de pistas |

### 2.3 Contraste verificado (WCAG 2.1)

| Par | Ratio | ¿Cumple? |
|-----|-------|----------|
| Texto `#101010` sobre primario `#b0c2cc` | 7,4:1 | Sí |
| Texto blanco `#fffcfb` sobre secundario `#4e6077` | 6,8:1 | Sí |
| Texto `#101010` sobre fondo global `#fffcfb` | 18,5:1 | Sí |
| Borde secundario `#4e6077` sobre fondo `#fffcfb` | 6,8:1 | Sí |

---

## 3. Geometría

| Elemento | Lógico (px) | Notas |
|----------|-------------|-------|
| Celda (Tablero 8x8) | 41 × 41 | Se ajusta dinámicamente según la dificultad |
| Separación entre celdas | 6 | Margen uniforme entre botones de letra |
| Margen lateral | 19 | El tablero queda centrado simétricamente |
| Radio de esquina | 5 | Uniforme en celdas, tarjetas y botones |
| Botón de navegación / HUD | **56 × 56** | Tamaño accesible optimizado para usuarios 40+ |

---

## 4. Componentes y Estados de Celdas

### 4.1 Celda del tablero
- Relleno inicial con color **Primario (`#b0c2cc`)**, radio de esquina 5 px, borde oscuro `#4e6077` o `#535456` de 2 px.
- Letra centrada en tinta `#101010`.

### 4.2 Estados de la celda
| Estado | Fondo / Relleno | Borde | Color de Texto |
|--------|-----------------|-------|----------------|
| **Reposo** | Primario (`#b0c2cc`) | Oscuro (`#4e6077`) | Oscuro (`#101010`) |
| **Seleccionada** | Secundario (`#4e6077`) | Oscuro (`#4e6077`) | Blanco (`#fffcfb`) |
| **Encontrada** | Turquesa (`#099c92`) | Turquesa oscuro | Blanco (`#fffcfb`, negrita) |
| **Bloqueada / Inactiva** | Bloqueado (`#efefee`) | Gris sutil | Gris oscuro (`#535456`) |

### 4.3 Panel Superior de Palabras
- Sin marcos ni recuadros oscuros envolventes.
- Integrado en flujo continuo (*wrapping*) sobre el fondo global (`#fffcfb`).

---

## 5. Tipografía

- **Letras del tablero:** `PressStart2P.ttf` (OFL), a **24 px** lógicos (múltiplo de la rejilla de 8 px para nitidez perfecta con filtrado `nearest`).
- **Interfaz y HUD:** `AtkinsonHyperlegible-Regular.ttf` (OFL, Braille Institute), configurada en `core/AccessibleTheme.tres` como tipografía por defecto.

---

## 6. Producción de assets e iconos

- Todos los componentes gráficos estructurales se generan dinámicamente mediante `StyleBoxFlat` utilizando estrictamente la paleta de tokens oficial (`primary`, `secondary`, `blocked`, `background`).
- Iconos: PNG con alfa de 16×16 px escalados a 32×32 con filtrado `nearest`.