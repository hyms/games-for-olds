class_name WordSearchTheme
extends Resource

## Categoría del banco de palabras de la Sopa de Letras (spec §5 y §5.1).
## Un archivo `.tres` por grupo temático, cargado por `WordBank`.
## Evolución del modelo original de "tema" (decisión C9): se le añade
## `category_id` y la validación del banco; el resto de propiedades de
## dificultad (`grid_size`, etc.) se migran a `DifficultyConfig` en la Fase 2.

## Identificador estable del grupo temático (= nombre del archivo .tres).
## Lo usan el Shuffle Bag y la progresión guardada.
@export var category_id: String = ""

## Nombre visible para el jugador (spec §5.1).
@export var theme_name: String = "General"

@export_multiline var description: String = ""

## Lista de palabras del grupo, ya normalizadas (A–Z, sin tildes ni Ñ).
@export var words: Array[String] = []

## Reglas del banco (spec §5): mínimo 60 palabras por categoría y
## longitudes de 4 a 15 letras (tableros de 8×8 a 15×15, decisión C3).
const MIN_WORDS := 60
const MIN_WORD_LENGTH := 4
const MAX_WORD_LENGTH := 15

## Sustituciones de tildes, diéresis y Ñ para normalizar a A–Z (ESPAÑA → ESPANA)
const ACCENT_REPLACEMENTS := {
	"Á": "A", "É": "E", "Í": "I", "Ó": "O", "Ú": "U", "Ü": "U", "Ñ": "N",
}

# --- Propiedades de dificultad (temporales: las absorbe DifficultyConfig) ----
# Mientras existan, conservan los valores con los que se crearon los recursos.

## Tamaño del grid (NxN)
@export_range(5, 15) var grid_size: int = 8

## Número máximo de palabras a colocar
@export_range(3, 20) var max_words: int = 8

## Longitud mínima de palabra aceptada
@export_range(3, 8) var min_word_length: int = 4

## Permite colocar palabras en diagonal
@export var allow_diagonal: bool = true

## Permite colocar palabras al revés (invertidas)
@export var allow_backwards: bool = true


## Normaliza una palabra a mayúsculas A–Z sin tildes, diéresis ni Ñ.
## También elimina espacios y cualquier carácter no alfabético (ESPAÑA → ESPANA).
static func normalize_word(word: String) -> String:
	var upper := word.strip_edges().to_upper()
	var cleaned := ""
	for c in upper:
		var base := String(ACCENT_REPLACEMENTS.get(c, c))
		if base >= "A" and base <= "Z":
			cleaned += base
	return cleaned


## Filtra palabras por longitud mínima al generar (sin tocar la lista original).
func get_filtered_words() -> Array[String]:
	var filtered: Array[String] = []
	for w in words:
		var clean: String = normalize_word(w)
		if clean.length() < min_word_length or clean.length() > grid_size:
			continue
		filtered.append(clean)
	return filtered


## Comprueba la categoría contra las reglas del banco (spec §5).
## Devuelve la lista de errores; vacío = categoría válida.
## Comprueba: identificador, mínimo 60 palabras, normalización, longitud
## 4–15, sin duplicados y que no quede vacía tras el filtro.
func validate() -> PackedStringArray:
	var errors: PackedStringArray = []
	if category_id.strip_edges().is_empty():
		errors.append("falta category_id")
	if theme_name.strip_edges().is_empty():
		errors.append("falta theme_name")
	if words.size() < MIN_WORDS:
		errors.append("solo %d palabras, mínimo %d" % [words.size(), MIN_WORDS])
	var seen: Dictionary = {}
	for w in words:
		var normalized: String = normalize_word(w)
		if normalized.is_empty():
			errors.append("palabra sin letras válidas: '%s'" % w)
			continue
		if normalized != w:
			errors.append("palabra sin normalizar: '%s' (debe ir como %s)" % [w, normalized])
		if normalized.length() < MIN_WORD_LENGTH or normalized.length() > MAX_WORD_LENGTH:
			errors.append("longitud %d fuera de %d–%d: '%s'" % [
				normalized.length(), MIN_WORD_LENGTH, MAX_WORD_LENGTH, w,
			])
		if seen.has(normalized):
			errors.append("palabra duplicada: '%s'" % normalized)
		seen[normalized] = true
	if seen.is_empty():
		errors.append("categoría vacía tras el filtro")
	return errors
