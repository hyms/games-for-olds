extends GdUnitTestSuite

## Primer test puro (Fase 0.5): normalización y filtros de `WordSearchTheme`.
## No escenas ni nodos: solo la lógica de `resources/themes/WordSearchTheme.gd`.


func _make_theme(words: Array[String], grid_size: int = 8, min_word_length: int = 4) -> WordSearchTheme:
	var ws_theme := WordSearchTheme.new()
	ws_theme.grid_size = grid_size
	ws_theme.min_word_length = min_word_length
	ws_theme.words = words
	return ws_theme


func test_normaliza_a_mayusculas_y_quita_espacios() -> void:
	var ws_theme := _make_theme(["casa", "  mesa  ", "LUNA"])
	var result: Array[String] = ws_theme.get_filtered_words()
	assert_array(result).contains_exactly(["CASA", "MESA", "LUNA"])


func test_quita_tildes_dieresis_y_enye() -> void:
	var ws_theme := _make_theme(["ESPAÑA", "pingüino", "jardín"])
	var result: Array[String] = ws_theme.get_filtered_words()
	assert_array(result).contains_exactly(["ESPANA", "PINGUINO", "JARDIN"])


func test_descarta_palabras_mas_cortas_que_el_minimo() -> void:
	var ws_theme := _make_theme(["sol", "luna", "mar", "mesa"])
	var result: Array[String] = ws_theme.get_filtered_words()
	assert_array(result).contains_exactly(["LUNA", "MESA"])


func test_descarta_palabras_mas_largas_que_el_tablero() -> void:
	var ws_theme := _make_theme(["mesa", "refrigerador"], 8)
	var result: Array[String] = ws_theme.get_filtered_words()
	assert_array(result).contains_exactly(["MESA"])


func test_descarta_palabras_sin_letras_validas() -> void:
	var ws_theme := _make_theme(["1234", "m3s4", "casa"])
	var result: Array[String] = ws_theme.get_filtered_words()
	assert_array(result).contains_exactly(["CASA"])


func test_el_filtro_no_modifica_la_lista_original() -> void:
	var ws_theme := _make_theme(["sol", "mesa"])
	var _first: Array[String] = ws_theme.get_filtered_words()
	assert_array(ws_theme.words).contains_exactly(["sol", "mesa"])


# --- Fase 1: validate() del banco (spec §5) ----------------------------------

const LETTERS := "ABCDEFGHIJKLMNOPQRSTUVWXYZ"


## Categoría de prueba válida: identificador, nombre y `count` palabras únicas.
func _make_category(words: Array[String]) -> WordSearchTheme:
	var category := _make_theme(words, 15, 4)
	category.category_id = "prueba"
	category.theme_name = "Prueba"
	return category


## `count` palabras únicas de 6 letras A–Z (p. ej. "AACASA", "ABCASA", ...).
func _valid_words(count: int) -> Array[String]:
	var out: Array[String] = []
	var index := 0
	while out.size() < count:
		var first := LETTERS.substr(int(index / 26.0), 1)
		var second := LETTERS.substr(index % 26, 1)
		out.append(first + second + "CASA")
		index += 1
	return out


func test_validate_acepta_una_categoria_completa() -> void:
	var category := _make_category(_valid_words(60))
	assert_bool(category.validate().is_empty()) \
		.override_failure_message("validate() = %s" % str(category.validate())) \
		.is_true()


func test_validate_detecta_duplicados_longitud_y_palabras_sin_normalizar() -> void:
	var words := _valid_words(60)
	words.append(words[0])       # duplicada
	words.append("ABC")          # demasiado corta
	words.append("MESAÑ")        # con Ñ: debe ir como MESAN
	var errors: PackedStringArray = _make_category(words).validate()
	assert_int(errors.size()).is_equal(3)


func test_validate_marca_como_invalida_una_categoria_vacia() -> void:
	var errors: PackedStringArray = _make_category([]).validate()
	# sin palabras: mínimo 60 incumplido + categoría vacía tras el filtro
	assert_int(errors.size()).is_greater_equal(2)
