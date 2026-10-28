class_name PixelStyles
extends RefCounted

## Fábrica de estilos "pixel art limpio" del proyecto.
## Fuente única de valores: `docs/art_direction.md` (plantilla `docs/word_search/wathersoup.jpg`).
## Los contrastes se verifican en `tests/unit/PixelStylesTest.gd`.

# --- Paleta base -------------------------------------------------------------
const ARENA := Color("#b5ad96") ## Fondo general de pantallas
const CELDA := Color("#fbf3da") ## Relleno de celdas del tablero
const PLACA := Color("#ead9bb") ## Interior de placas y modales
const TINTA := Color("#101010") ## Letras y texto sobre claros
const BORDE := Color("#535456") ## Borde visible de placas, celdas y botones
const SOMBRA := Color("#8f8874") ## Sombra dura decorativa (no delimita)

# --- Acentos -----------------------------------------------------------------
const PURPURA := Color("#9944c6") ## Botón principal y píldora de categoría
const PURPURA_PRESIONADO := Color("#7a33a0") ## Estado presionado del púrpura
const AMARILLO := Color("#fdc109") ## Acción mágica (pista)
const TURQUESA := Color("#099c92") ## Confirmación / estado encontrado
const CIAN := Color("#08d0df") ## Acciones de sistema (refrescar, salir)

# --- Estados del tablero -----------------------------------------------------
const CELDA_SELECCION := Color("#6095fb") ## Relleno de la celda en selección
const TRAZO := Color("#2b56b8") ## Línea de arrastre (≥ 3:1 sobre crema y arena)
const ENCONTRADA_BORDE := Color("#04544d") ## Borde de celda encontrada
const PISTA_BORDE := Color("#8a4600") ## Borde de celda en pista
const PISTA_ACENTO := Color("#f9881e") ## Animación de pista sobre color oscuro

# --- Textura de esquina ------------------------------------------------------
const RADIUS := 5 ## Radio de esquina (plantilla 8 px ÷ 1,594)


## Contraste WCAG 2.1 entre dos colores (1.0 – 21.0).
static func contrast_ratio(a: Color, b: Color) -> float:
	var light := maxf(_luminance(a), _luminance(b))
	var dark := minf(_luminance(a), _luminance(b))
	return (light + 0.05) / (dark + 0.05)


## Color de texto legible sobre un relleno: blanco o tinta, el de mayor contraste.
static func text_color(fill: Color) -> Color:
	if contrast_ratio(Color.WHITE, fill) >= contrast_ratio(TINTA, fill):
		return Color.WHITE
	return TINTA


## Celda en reposo: crema con borde oscuro y sombra dura.
static func cell_normal() -> StyleBoxFlat:
	return _box(CELDA, BORDE, 2, RADIUS)


## Celda seleccionada (trazo en curso): relleno azul con borde oscuro.
## El estado lo refuerza la línea de trazo (TRAZO) dibujada encima.
static func cell_selected() -> StyleBoxFlat:
	return _box(CELDA_SELECCION, BORDE, 2, RADIUS)


## Celda encontrada: borde turquesa oscuro grueso (la marca "✔" es el complemento).
static func cell_found() -> StyleBoxFlat:
	return _box(CELDA, ENCONTRADA_BORDE, 3, RADIUS, false)


## Celda resaltada por pista: borde naranja oscuro con pulso (3 s).
static func cell_hint() -> StyleBoxFlat:
	return _box(CELDA, PISTA_BORDE, 3, RADIUS, false)


## Placa de encabezado o modal.
static func plate() -> StyleBoxFlat:
	return _box(PLACA, BORDE, 2, RADIUS)


## Píldora de categoría.
static func pill() -> StyleBoxFlat:
	return _box(PURPURA, TINTA, 2, 999, false)


## Botón de 56×56 con el color de familia indicado.
static func button(fill: Color = PURPURA) -> StyleBoxFlat:
	var box := _box(fill, TINTA, 2, RADIUS)
	box.content_margin_left = 16.0
	box.content_margin_right = 16.0
	box.content_margin_top = 12.0
	box.content_margin_bottom = 12.0
	return box


## Estado presionado: sin sombra y borde más grueso (cambio de forma, no de color).
static func button_pressed(fill: Color = PURPURA) -> StyleBoxFlat:
	var box := button(fill)
	box.set_border_width_all(3)
	box.shadow_size = 0
	box.shadow_color = Color(0, 0, 0, 0)
	return box


## Estado foco: anillo exterior oscuro de 3 px (visible sobre cualquier fondo).
static func button_focus(fill: Color = PURPURA) -> StyleBoxFlat:
	var box := button(fill)
	box.outline_size = 3
	box.outline_color = TINTA
	box.set_expand_margin_all(3)
	return box


## Estado deshabilitado: plano con borde, sin sombra.
static func button_disabled() -> StyleBoxFlat:
	return _box(ARENA, BORDE, 2, RADIUS, false)


static func _box(bg: Color, border: Color, border_width: int, radius: int,
		shadow: bool = true) -> StyleBoxFlat:
	var box := StyleBoxFlat.new()
	box.bg_color = bg
	box.border_color = border
	box.set_border_width_all(border_width)
	box.set_corner_radius_all(radius)
	box.anti_aliasing = false
	box.corner_detail = 2
	if shadow:
		box.shadow_size = 2
		box.shadow_color = SOMBRA
		box.shadow_offset = Vector2(2, 2)
	return box


static func _luminance(color: Color) -> float:
	return 0.2126 * _channel(color.r) + 0.7152 * _channel(color.g) \
		+ 0.0722 * _channel(color.b)


static func _channel(value: float) -> float:
	if value <= 0.03928:
		return value / 12.92
	return pow((value + 0.055) / 1.055, 2.4)
