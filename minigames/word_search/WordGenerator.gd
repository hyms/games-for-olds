class_name WordGenerator
extends RefCounted

## Direcciones posibles (8 direcciones + opcional invertido)
const DIRECTIONS := [
	Vector2i(1,0),   # derecha
	Vector2i(-1,0),  # izquierda
	Vector2i(0,1),   # abajo
	Vector2i(0,-1),  # arriba
	Vector2i(1,1),   # abajo-derecha
	Vector2i(-1,1),  # abajo-izquierda
	Vector2i(1,-1),  # arriba-derecha
	Vector2i(-1,-1), # arriba-izquierda
]

## Resultado de generación
class WordPlacement:
	var word: String
	var positions: Array[Vector2i] = [] # posiciones (col, fila) en orden

class GenerationResult:
	var grid: Array = [] # 2D [fila][col]
	var placements: Array[WordPlacement] = []
	var grid_size: int = 0
	var success: bool = false

## Genera grid NxN con palabras del tema
static func generate(theme: WordSearchTheme) -> GenerationResult:
	var res: WordGenerator.GenerationResult = GenerationResult.new()
	res.grid_size = theme.grid_size
	var size: int = theme.grid_size
	
	# Inicializar grid vacío (espacio)
	res.grid = []
	for y in size:
		res.grid.append([])
		for x in size:
			res.grid[y].append(" ") # vacío
	
	# Obtener palabras filtradas y barajar
	var candidates: Array[String] = theme.get_filtered_words()
	candidates.shuffle()
	
	# Limitar a max_words
	var words_to_place: Array[String] = []
	for w in candidates:
		if words_to_place.size() >= theme.max_words:
			break
		words_to_place.append(w)
	
	# Intentar colocar palabras
	var placements_list: Array[WordPlacement] = []
	for word in words_to_place:
		var placed: WordGenerator.WordPlacement = _try_place_word(res.grid, size, word, theme, placements_list)
		if placed:
			placements_list.append(placed)
	
	res.placements = placements_list
	res.success = placements_list.size() > 0
	
	# Rellenar espacios vacíos con letras aleatorias
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.randomize()
	var letters := "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
	for y in size:
		for x in size:
			if res.grid[y][x] == " ":
				var l: String = letters[rng.randi() % letters.length()]
				res.grid[y][x] = l
	
	return res

## Intenta colocar una palabra
static func _try_place_word(grid: Array, size: int, word: String, theme: WordSearchTheme, existing: Array[WordPlacement]) -> WordPlacement:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.randomize()
	var dirs := DIRECTIONS.duplicate()
	dirs.shuffle()
	for d in dirs:
		var variants := [word]
		if theme.allow_backwards:
			variants.append(word.reverse())
		for wvar in variants:
			for attempt in 60:
				var start_x := rng.randi_range(0, size - 1)
				var start_y := rng.randi_range(0, size - 1)
				if _can_place(grid, size, wvar, start_x, start_y, d):
					var wp := WordPlacement.new()
					wp.word = word
					for i in wvar.length():
						var px := start_x + d.x * i
						var py := start_y + d.y * i
						grid[py][px] = wvar[i]
						wp.positions.append(Vector2i(px, py))
					return wp
	return null

## Comprueba si cabe y no solapa con letra distinta
static func _can_place(grid: Array, size: int, word: String, sx: int, sy: int, dir: Vector2i) -> bool:
	for i in word.length():
		var px := sx + dir.x * i
		var py := sy + dir.y * i
		if px < 0 or px >= size or py < 0 or py >= size:
			return false
		var existing_char := grid[py][px]
		if existing_char != " " and existing_char != word[i]:
			return false
	return true
