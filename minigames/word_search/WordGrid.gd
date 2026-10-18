class_name WordGrid
extends Control

## Señales
signal word_found(word: String)
signal all_words_found

## Referencias
@export var cell_scene: PackedScene

var grid_size: int = 8
var cells_grid: Array = [] # [y][x] WordCell
var current_theme: WordSearchTheme
var placements: Array[WordGenerator.WordPlacement] = []
var grid_data: Array = []

## Selección
var selecting: bool = false
var selected_path: Array[WordCell] = []
var selected_positions: Array[Vector2i] = []

func setup(theme: WordSearchTheme) -> void:
	current_theme = theme
	grid_size = theme.grid_size
	var result := WordGenerator.generate(theme)
	grid_data = result.grid
	placements = result.placements
	_clear_grid()
	_build_grid()
	_clear_selection()

func _clear_grid() -> void:
	for child in get_children():
		child.queue_free()
	cells_grid.clear()

func _build_grid() -> void:
	cells_grid = []
	var cell_size := Vector2(48, 48)
	var spacing := 4
	for y in grid_size:
		cells_grid.append([])
		for x in grid_size:
			var cell := cell_scene.instantiate() as WordCell
			add_child(cell)
			cell.grid_pos = Vector2i(x, y)
			var letter: String = grid_data[y][x]
			cell.set_letter(letter)
			cell.cell_pressed.connect(_on_cell_pressed)
			cell.position = Vector2(x * (cell_size.x + spacing), y * (cell_size.y + spacing))
			cells_grid[y].append(cell)
	var grid_w: float = grid_size * (cell_size.x + spacing) - spacing
	var grid_h: float = grid_size * (cell_size.y + spacing) - spacing
	custom_minimum_size = Vector2(grid_w, grid_h)

func _on_cell_pressed(cell: WordCell) -> void:
	if cell.is_found:
		return
	if not selecting:
		_start_selection(cell)
	else:
		_end_selection(cell)

func _start_selection(cell: WordCell) -> void:
	selecting = true
	_clear_selection()
	_add_to_selection(cell)

func _add_to_selection(cell: WordCell) -> void:
	if cell.is_found:
		return
	if selected_positions.has(cell.grid_pos):
		return
	selected_positions.append(cell.grid_pos)
	selected_path.append(cell)
	cell.set_selected(true)

func _end_selection(_last_cell: WordCell) -> void:
	_validate_selection()
	selecting = false
	_clear_selection()

func _clear_selection() -> void:
	for c in selected_path:
		if is_instance_valid(c) and not c.is_found:
			c.set_selected(false)
	selected_path.clear()
	selected_positions.clear()

func _validate_selection() -> void:
	if selected_positions.is_empty():
		return
	var sel_str: String = ""
	for c in selected_path:
		sel_str += c.letter
	var sel_str_rev: String = sel_str.reverse()
	for i in placements.size():
		var p := placements[i]
		if p.word == sel_str or p.word == sel_str_rev:
			_mark_found(p)
			return
		if _positions_match(p.positions, selected_positions):
			_mark_found(p)
			return
		if _positions_match(p.positions, selected_positions.duplicate().reverse()):
			_mark_found(p)
			return

func _positions_match(a: Array[Vector2i], b: Array[Vector2i]) -> bool:
	if a.size() != b.size():
		return false
	for i in a.size():
		if a[i] != b[i]:
			return false
	return true

func _mark_found(placement: WordGenerator.WordPlacement) -> void:
	for pos in placement.positions:
		var c := _get_cell_at(pos.x, pos.y)
		if c:
			c.set_found(true)
	word_found.emit(placement.word)
	placements.erase(placement)
	if placements.is_empty():
		all_words_found.emit()
	_clear_selection()

func _get_cell_at(x: int, y: int) -> WordCell:
	if x < 0 or x >= grid_size or y < 0 or y >= grid_size:
		return null
	return cells_grid[y][x]
