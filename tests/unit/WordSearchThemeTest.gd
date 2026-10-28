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
	var ws_theme := _make_theme(["mesa", "refrigerador"], grid_size = 8)
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
