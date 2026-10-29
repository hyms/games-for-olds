extends GdUnitTestSuite

## UT-WS-06 — ShuffleBagSystem (spec §4.3 y §5 · test.md §2.2).
## En 20 partidas cada uno de los 10 grupos temáticos aparece exactamente
## 2 veces; la bolsa solo se rebaraja al agotarse; el estado es serializable
## para SaveManager (Fase 3 global).

const GROUP_IDS: Array[String] = [
	"animales", "plantas", "frutas_verduras", "objetos", "profesiones",
	"paises_ciudades", "comidas", "instrumentos", "herramientas", "ropa",
]


func test_en_20_partidas_cada_grupo_aparece_dos_veces() -> void:
	var bag := ShuffleBagSystem.new(GROUP_IDS, 42)
	var counts := {}
	for i in 20:
		var group := bag.next_group()
		counts[group] = int(counts.get(group, 0)) + 1
	assert_int(counts.size()).is_equal(GROUP_IDS.size())
	for id in GROUP_IDS:
		var appearances := int(counts.get(id, 0))
		assert_int(appearances) \
			.override_failure_message("grupo %s: %d apariciones en 20 partidas (mínimo 2)" % [
				id, appearances,
			]) \
			.is_equal(2)


func test_las_primeras_diez_partidas_no_repiten_grupo() -> void:
	var bag := ShuffleBagSystem.new(GROUP_IDS, 7)
	var seen := {}
	for i in GROUP_IDS.size():
		var group := bag.next_group()
		assert_bool(seen.has(group)) \
			.override_failure_message("grupo repetido en la primera vuelta: %s" % group) \
			.is_false()
		seen[group] = true
	assert_int(bag.remaining()).is_equal(0)


func test_la_bolsa_se_rebaraja_solo_al_agotarse() -> void:
	var bag := ShuffleBagSystem.new(GROUP_IDS, 3)
	assert_int(bag.refill_count).is_equal(1)  # la bolsa inicial
	for i in 18:
		bag.next_group()
	assert_int(bag.refill_count).is_equal(1)
	assert_int(bag.remaining()).is_equal(1)
	bag.next_group()  # 20.ª saca: agota la bolsa sin rebarajar
	assert_int(bag.refill_count).is_equal(1)
	assert_int(bag.remaining()).is_equal(0)
	bag.next_group()  # 21.ª: aquí sí se rebaraja
	assert_int(bag.refill_count).is_equal(2)
	assert_int(bag.remaining()).is_equal(19)


func test_misma_semilla_da_misma_secuencia() -> void:
	var a := ShuffleBagSystem.new(GROUP_IDS, 1234)
	var b := ShuffleBagSystem.new(GROUP_IDS, 1234)
	for i in 20:
		assert_str(a.next_group()).is_equal(b.next_group())


func test_estado_serializable_restaura_la_misma_bolsa() -> void:
	var bag := ShuffleBagSystem.new(GROUP_IDS, 99)
	for i in 7:
		bag.next_group()
	var restored := ShuffleBagSystem.from_dict(bag.to_dict())
	assert_int(restored.remaining()).is_equal(bag.remaining())
	assert_int(restored.refill_count).is_equal(bag.refill_count)
	# La secuencia continúa idéntica: se restaura también el estado del RNG.
	for i in 10:
		assert_str(restored.next_group()).is_equal(bag.next_group())
