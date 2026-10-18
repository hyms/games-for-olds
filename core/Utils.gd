class_name Utils
extends RefCounted

## Aplica un "safe margin" para hitboxes táctiles.
## Útil cuando el sprite visual es pequeño pero queremos área mayor.
static func get_touch_rect(control: Control, margin: float = 8.0) -> Rect2:
	if not control:
		return Rect2()
	var rect: Rect2 = control.get_global_rect()
	rect = rect.grow(margin)
	return rect

## Convierte un path relativo a res:// a una versión legible si hace falta
static func scene_name_from_path(path: String) -> String:
	if path.is_empty():
		return ""
	var name: String = path.get_file().get_basename()
	return name.replace("_", " ").capitalize()
