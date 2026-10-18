@tool
class_name BigButton
extends Button

## Componente botón grande pensado para móviles y adultos mayores.
## - Usa custom_minimum_size >= 56x56
## - Añadido al grupo ui_focusable para navegación futura
## - Permite ampliar área táctil con grow_hitbox

@export var grow_hitbox: float = 0.0 ## px extra para expandir rect global (opcional)

func _ready() -> void:
	# Asegura tamaño mínimo accesible
	if custom_minimum_size.x < Constants.MIN_TOUCH_SIZE.x:
		custom_minimum_size.x = Constants.MIN_TOUCH_SIZE.x
	if custom_minimum_size.y < Constants.MIN_TOUCH_SIZE.y:
		custom_minimum_size.y = Constants.MIN_TOUCH_SIZE.y
	size = custom_minimum_size

	add_to_group(Constants.GROUP_UI_FOCUSABLE)
	add_to_group(Constants.GROUP_TOUCHABLE)

	mouse_filter = Control.MOUSE_FILTER_STOP

func _get_global_rect() -> Rect2:
	var rect := super._get_global_rect()
	if grow_hitbox > 0.0:
		rect = rect.grow(grow_hitbox)
	return rect
