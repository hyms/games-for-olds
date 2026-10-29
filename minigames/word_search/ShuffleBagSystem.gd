class_name ShuffleBagSystem
extends RefCounted

## Bolsa de grupos temáticos (Shuffle Bag, spec §5 · prueba UT-WS-06).
##
## La bolsa contiene cada grupo exactamente `COPIES_PER_GROUP` veces y se
## baraja al llenarla: en 20 partidas (una bolsa entera) cada uno de los 10
## grupos temáticos aparece 2 veces. La bolsa solo se rebaraja al agotarse.
## Estado serializable con `to_dict()`/`from_dict()` para `SaveManager`
## (llega en la Fase 3 global; aquí queda el gancho listo).

## Copias de cada grupo por bolsa: 10 grupos × 2 = 20 partidas (spec §4.3).
const COPIES_PER_GROUP := 2

## Versión del estado serializado (para migrar guardados futuros).
const DICT_VERSION := 1

## Veces que se ha rebarajado la bolsa (1 = la inicial). Para tests.
var refill_count: int = 0

var _rng := RandomNumberGenerator.new()
var _group_ids: Array[String] = []
var _bag: Array[String] = []


## Crea la bolsa con los grupos indicados y la llena barajada.
## `seed_value >= 0` fija la semilla (para tests deterministas); -1 = aleatoria.
func _init(group_ids: Array[String] = [], seed_value: int = -1) -> void:
	_group_ids = group_ids.duplicate()
	if seed_value >= 0:
		_rng.seed = seed_value
	else:
		_rng.randomize()
	_refill()


## Siguiente grupo a jugar. Agota la bolsa actual y, solo entonces, la rellena.
func next_group() -> String:
	if _group_ids.is_empty():
		push_warning("[ShuffleBag] sin grupos: no se puede sortear nada")
		return ""
	if _bag.is_empty():
		_refill()
	var index := _rng.randi_range(0, _bag.size() - 1)
	var group: String = _bag.pop_at(index)
	return group


## Grupos que quedan en la bolsa (0 = agotada, la siguiente sacada rebaraja).
func remaining() -> int:
	return _bag.size()


## Grupos conocidos por la bolsa (los que se reparten entre copias).
func known_groups() -> Array[String]:
	return _group_ids.duplicate()


## Serializa el estado completo para guardarlo (SaveManager, Fase 3).
func to_dict() -> Dictionary:
	return {
		"version": DICT_VERSION,
		"group_ids": _group_ids.duplicate(),
		"bag": _bag.duplicate(),
		"refill_count": refill_count,
		"rng_state": _rng.state,
	}


## Reconstruye la bolsa desde el estado guardado (continúa la misma secuencia).
static func from_dict(data: Dictionary) -> ShuffleBagSystem:
	if int(data.get("version", 0)) != DICT_VERSION:
		push_warning("[ShuffleBag] versión de estado desconocida: %s" % str(data.get("version")))
	var ids: Array[String] = []
	for id in data.get("group_ids", []):
		ids.append(String(id))
	var bag := ShuffleBagSystem.new(ids, 0)
	bag._bag.clear()
	for id in data.get("bag", []):
		bag._bag.append(String(id))
	bag.refill_count = int(data.get("refill_count", 0))
	bag._rng.state = int(data.get("rng_state", 0))
	return bag


## Llena la bolsa con `COPIES_PER_GROUP` copias de cada grupo y la baraja
## con el RNG propio (así una semilla determinista da secuencias repetibles).
func _refill() -> void:
	_bag.clear()
	for id in _group_ids:
		for copy_index in COPIES_PER_GROUP:
			_bag.append(id)
	for i in range(_bag.size() - 1, 0, -1):
		var j := _rng.randi_range(0, i)
		var swap := _bag[i]
		_bag[i] = _bag[j]
		_bag[j] = swap
	refill_count += 1
