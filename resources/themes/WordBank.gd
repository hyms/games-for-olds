class_name WordBank
extends RefCounted

## Catálogo de las 10 categorías del banco de palabras (spec §5.1).
##
## Formato: recursos `.tres` incluidos en la app (decisión C9): funciona sin
## conexión, no requiere escritura en `user://` y se editan desde el editor.
## Las rutas son explícitas para que el orden de la spec sea estable también
## dentro de una exportación.

## Rutas en el orden de la spec §5.1 (id = nombre del archivo).
const CATEGORY_PATHS: Array[String] = [
	"res://resources/themes/themes/animales.tres",
	"res://resources/themes/themes/plantas.tres",
	"res://resources/themes/themes/frutas_verduras.tres",
	"res://resources/themes/themes/objetos.tres",
	"res://resources/themes/themes/profesiones.tres",
	"res://resources/themes/themes/paises_ciudades.tres",
	"res://resources/themes/themes/comidas.tres",
	"res://resources/themes/themes/instrumentos.tres",
	"res://resources/themes/themes/herramientas.tres",
	"res://resources/themes/themes/ropa.tres",
]


## Carga las 10 categorías en el orden de la spec. Los recursos ausentes o
## inválidos se saltan con error en consola (no rompen la partida).
static func load_all() -> Array[WordSearchTheme]:
	var categories: Array[WordSearchTheme] = []
	for path in CATEGORY_PATHS:
		if not ResourceLoader.exists(path):
			push_error("[WordBank] falta el recurso %s" % path)
			continue
		var category := load(path) as WordSearchTheme
		if category == null:
			push_error("[WordBank] %s no es una WordSearchTheme" % path)
			continue
		categories.append(category)
	return categories


## Carga una categoría por identificador (p. ej. "animales"); null si no existe.
static func load_by_id(category_id: String) -> WordSearchTheme:
	for category in load_all():
		if category.category_id == category_id:
			return category
	return null


## Identificadores de las categorías cargadas, en orden de la spec.
static func ids() -> Array[String]:
	var out: Array[String] = []
	for category in load_all():
		out.append(category.category_id)
	return out
