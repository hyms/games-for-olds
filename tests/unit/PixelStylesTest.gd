extends GdUnitTestSuite

## Fase 0.6 — Dirección de arte: contraste, geometría y coherencia del tema.
## Reglas de `docs/art_direction.md` §2.3 (WCAG 2.1): texto ≥ 4,5:1 y
## bordes/estados ≥ 3:1. El test es la garantía de que la paleta no se rompe.

const THEME_PATH := "res://core/AccessibleTheme.tres"
const BIG_BUTTON_PATH := "res://ui/components/BigButton.tscn"

const FAMILIES: Array[String] = [
	"PURPURA", "PURPURA_PRESIONADO", "AMARILLO", "TURQUESA", "CIAN",
	"CELDA", "PLACA", "ARENA", "CELDA_SELECCION",
]


func _fill(name: String) -> Color:
	match name:
		"PURPURA": return PixelStyles.PURPURA
		"PURPURA_PRESIONADO": return PixelStyles.PURPURA_PRESIONADO
		"AMARILLO": return PixelStyles.AMARILLO
		"TURQUESA": return PixelStyles.TURQUESA
		"CIAN": return PixelStyles.CIAN
		"CELDA": return PixelStyles.CELDA
		"PLACA": return PixelStyles.PLACA
		"ARENA": return PixelStyles.ARENA
		"CELDA_SELECCION": return PixelStyles.CELDA_SELECCION
	return PixelStyles.ARENA


func _ratio(a: Color, b: Color) -> float:
	return PixelStyles.contrast_ratio(a, b)


func test_texto_legible_sobre_cada_familia_de_color() -> void:
	for family: String in FAMILIES:
		var fill: Color = _fill(family)
		var text: Color = PixelStyles.text_color(fill)
		assert_bool(_ratio(text, fill) >= 4.5) \
			.override_failure_message("%s: texto sobre %s = %.2f (mín. 4.5)" \
				% [family, fill.to_html(), _ratio(text, fill)]) \
			.is_true()


func test_borde_tinta_visible_sobre_cada_familia_y_sobre_fondo() -> void:
	for family: String in FAMILIES:
		var fill: Color = _fill(family)
		assert_bool(_ratio(PixelStyles.TINTA, fill) >= 3.0) \
			.override_failure_message("%s: borde interior = %.2f (mín. 3)" \
				% [family, _ratio(PixelStyles.TINTA, fill)]) \
			.is_true()
	assert_bool(_ratio(PixelStyles.TINTA, PixelStyles.ARENA) >= 3.0).is_true()
	assert_bool(_ratio(PixelStyles.BORDE, PixelStyles.ARENA) >= 3.0).is_true()


func test_bordes_del_tablero_cumple_tres_contra_relleno_y_fondo() -> void:
	var bordes: Array[Color] = [
		PixelStyles.BORDE, PixelStyles.ENCONTRADA_BORDE, PixelStyles.PISTA_BORDE,
	]
	for border: Color in bordes:
		assert_bool(_ratio(border, PixelStyles.CELDA) >= 3.0) \
			.override_failure_message("borde %s vs crema = %.2f" \
				% [border.to_html(), _ratio(border, PixelStyles.CELDA)]) \
			.is_true()
		assert_bool(_ratio(border, PixelStyles.ARENA) >= 3.0) \
			.override_failure_message("borde %s vs arena = %.2f" \
				% [border.to_html(), _ratio(border, PixelStyles.ARENA)]) \
			.is_true()


func test_trazo_de_seleccion_visible_sobre_las_celdas() -> void:
	assert_bool(_ratio(PixelStyles.TRAZO, PixelStyles.CELDA) >= 3.0).is_true()
	assert_bool(_ratio(PixelStyles.TINTA, PixelStyles.CELDA_SELECCION) >= 4.5) \
		.is_true()


func test_texto_sobre_fondos_claros_cumple_cuatro_coma_cinco() -> void:
	var claros: Array[Color] = [
		PixelStyles.CELDA, PixelStyles.PLACA, PixelStyles.ARENA,
	]
	for bg: Color in claros:
		assert_bool(_ratio(PixelStyles.TINTA, bg) >= 4.5) \
			.override_failure_message("tinta sobre %s = %.2f" % [bg.to_html(), _ratio(PixelStyles.TINTA, bg)]) \
			.is_true()


func test_estilos_del_tablero_usan_radio_y_borde_esperados() -> void:
	var normal: StyleBoxFlat = PixelStyles.cell_normal()
	assert_int(normal.corner_radius_top_left).is_equal(PixelStyles.RADIUS)
	assert_int(normal.border_width_left).is_equal(2)
	assert_bool(normal.bg_color.is_equal_approx(PixelStyles.CELDA)).is_true()
	assert_int(PixelStyles.cell_found().border_width_left).is_equal(3)
	assert_int(PixelStyles.cell_hint().border_width_left).is_equal(3)


func test_foco_y_presionado_cambian_forma_no_solo_color() -> void:
	var focus: StyleBoxFlat = PixelStyles.button_focus()
	assert_int(focus.outline_size).is_greater_equal(3)
	assert_bool(_ratio(focus.outline_color, PixelStyles.ARENA) >= 3.0).is_true()
	var pressed: StyleBoxFlat = PixelStyles.button_pressed()
	assert_int(pressed.border_width_left).is_equal(3)
	assert_int(pressed.shadow_size).is_equal(0)


func test_tema_global_cumple_contraste_y_tipografia() -> void:
	var theme := load(THEME_PATH) as Theme
	assert_bool(theme != null).is_true()
	var normal := theme.get_stylebox("normal", "Button") as StyleBoxFlat
	var font_color := theme.get_color("font_color", "Button")
	assert_bool(_ratio(font_color, normal.bg_color) >= 4.5).is_true()
	var disabled := theme.get_stylebox("disabled", "Button") as StyleBoxFlat
	var disabled_color := theme.get_color("font_disabled_color", "Button")
	assert_bool(_ratio(disabled_color, disabled.bg_color) >= 4.5).is_true()
	var label_color := theme.get_color("font_color", "Label")
	assert_bool(_ratio(label_color, PixelStyles.ARENA) >= 4.5).is_true()
	assert_int(theme.default_font_size).is_greater_equal(20)
	assert_bool(theme.default_font != null).is_true()


func test_bigbutton_cumple_el_tamano_minimo_tactil() -> void:
	var scene := load(BIG_BUTTON_PATH) as PackedScene
	assert_bool(scene != null).is_true()
	var button := scene.instantiate() as Button
	assert_bool(button.custom_minimum_size.x >= Constants.MIN_TOUCH_SIZE.x
		and button.custom_minimum_size.y >= Constants.MIN_TOUCH_SIZE.y) \
		.override_failure_message("BigButton: %s (mín. %s)" % [str(button.custom_minimum_size), str(Constants.MIN_TOUCH_SIZE)]) \
		.is_true()
	button.free()
