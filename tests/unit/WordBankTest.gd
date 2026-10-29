extends GdUnitTestSuite

## Fase 1 — Banco de palabras (spec §5 y §5.1 · plan Fase 1 · UT-WS-07).
## Carga los 10 recursos y verifica: ids y nombres de la spec, ≥60 palabras
## válidas por categoría (4–15 letras A–Z, sin duplicar), ≥600 en total y
## que ninguna palabra se repita entre categorías (evita palabras ambiguas
## cuando una partida combina 2 o 4 grupos, C3).

const EXPECTED_IDS: Array[String] = [
	"animales", "plantas", "frutas_verduras", "objetos", "profesiones",
	"paises_ciudades", "comidas", "instrumentos", "herramientas", "ropa",
]

const EXPECTED_NAMES: Array[String] = [
	"Animales y Naturaleza", "Plantas y Flores", "Frutas y Verduras",
	"Cosas del Hogar", "Profesiones y Oficios", "Países y Geografía",
	"Comidas y Postres", "Instrumentos Musicales", "Herramientas y Bricolaje",
	"Ropa y Costura",
]


func test_banco_carga_las_diez_categorias_de_la_spec() -> void:
	var categories := WordBank.load_all()
	assert_int(categories.size()).is_equal(10)
	var ids: Array[String] = []
	var names: Array[String] = []
	for category in categories:
		ids.append(category.category_id)
		names.append(category.theme_name)
	assert_array(ids).contains_exactly(EXPECTED_IDS)
	assert_array(names).contains_exactly(EXPECTED_NAMES)


func test_cada_categoria_pasa_su_validacion() -> void:
	for category in WordBank.load_all():
		var errors: PackedStringArray = category.validate()
		assert_bool(errors.is_empty()) \
			.override_failure_message("%s: %s" % [category.category_id, str(errors)]) \
			.is_true()


func test_cada_categoria_tiene_al_menos_60_palabras_validas() -> void:
	for category in WordBank.load_all():
		var valid := 0
		for w in category.words:
			var normalized := WordSearchTheme.normalize_word(w)
			if normalized == w and normalized.length() >= 4 and normalized.length() <= 15:
				valid += 1
		assert_int(valid).is_greater_equal(60) \
			.override_failure_message("%s: solo %d palabras válidas (mínimo 60)" % [
				category.category_id, valid,
			])


func test_total_del_banco_supera_600_palabras() -> void:
	var total := 0
	for category in WordBank.load_all():
		total += category.words.size()
	assert_int(total).is_greater_equal(600) \
		.override_failure_message("total del banco = %d (mínimo 600)" % total)


func test_ninguna_palabra_se_repite_entre_categorias() -> void:
	var owner_by_word := {}
	for category in WordBank.load_all():
		for w in category.words:
			assert_bool(owner_by_word.has(w)) \
				.override_failure_message("'%s' está en %s y en %s" % [
					w, str(owner_by_word.get(w, "?")), category.category_id,
				]) \
				.is_false()
			owner_by_word[w] = category.category_id
	assert_int(owner_by_word.size()).is_greater_equal(600)


func test_load_by_id_devuelve_la_categoria_pedida() -> void:
	var category := WordBank.load_by_id("instrumentos")
	assert_bool(category != null).is_true()
	assert_str(category.theme_name).is_equal("Instrumentos Musicales")
	assert_bool(WordBank.load_by_id("no_existe") == null).is_true()
