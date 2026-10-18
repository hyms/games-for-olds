class_name WordSearchTheme
extends Resource

## Tema para el minijuego Sopa de Letras
@export var theme_name: String = "General"
@export_multiline var description: String = ""

## Lista de palabras candidatas para este tema
@export var words: Array[String] = []

## Tamaño del grid (NxN)
@export_range(5, 12) var grid_size: int = 8

## Número máximo de palabras a colocar
@export_range(3, 15) var max_words: int = 8

## Longitud mínima de palabra aceptada
@export_range(3, 8) var min_word_length: int = 4

## Permite colocar palabras en diagonal
@export var allow_diagonal: bool = true

## Permite colocar palabras al revés (invertidas)
@export var allow_backwards: bool = true

## Filtra palabras por longitud mínima al generar
func get_filtered_words() -> Array[String]:
	var filtered: Array[String] = []
	for w in words:
		var clean: String = w.strip_edges().to_upper()
		if clean.length() < min_word_length:
			continue
		if clean.length() > grid_size:
			continue
		# Limpiar caracteres no alfabéticos
		var cleaned: String = ""
		for c in clean:
			if c >= "A" and c <= "Z":
				cleaned += c
		if cleaned.length() < min_word_length or cleaned.length() > grid_size:
			continue
		clean = cleaned
		filtered.append(clean)
	return filtered
