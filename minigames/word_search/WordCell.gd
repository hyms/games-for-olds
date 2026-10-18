class_name WordCell
extends BigButton

## Celda individual del grid de Sopa de Letras
signal cell_pressed(cell: WordCell)

var grid_pos: Vector2i = Vector2i.ZERO
var letter: String = "A"
var is_selected: bool = false
var is_found: bool = false

func _ready() -> void:
	super._ready()
	text = letter
	pressed.connect(_on_pressed)
	_update_style()

func set_letter(l: String) -> void:
	letter = l.to_upper()
	text = letter
	_update_style()

func set_selected(selected: bool) -> void:
	is_selected = selected
	_update_style()

func set_found(found: bool) -> void:
	is_found = found
	is_selected = false
	_update_style()
	disabled = true

func _on_pressed() -> void:
	cell_pressed.emit(self)

func _update_style() -> void:
	if is_found:
		modulate = Color(0.4, 0.9, 0.5, 1.0) # verde
	elif is_selected:
		modulate = Color(1.0, 0.9, 0.4, 1.0) # amarillo
	else:
		modulate = Color(1.0, 1.0, 1.0, 1.0)
