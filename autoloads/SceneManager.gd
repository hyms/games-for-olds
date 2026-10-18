extends Node

## Autoload: SceneManager
## Gestiona cambios de escena con enfoque en desacoplamiento (Signals).
## Añadir como Autoload con nombre "SceneManager" en Project Settings > Globals > Autoloads

signal scene_changing(from_path: String, to_path: String)
signal scene_changed(to_path: String)

var current_scene_path: String = ""

func _ready() -> void:
	var tree: SceneTree = get_tree()
	if is_instance_valid(tree.current_scene) and not tree.current_scene.scene_file_path.is_empty():
		current_scene_path = tree.current_scene.scene_file_path

## Cambia a otra escena por ruta res://...
func go_to(path: String) -> void:
	if path.is_empty():
		push_warning("[SceneManager] Ruta vacía. Ignorando cambio de escena.")
		return
	if current_scene_path == path:
		push_warning("[SceneManager] Ya estás en la misma escena: %s" % path)
		return

	var tree: SceneTree = get_tree()
	scene_changing.emit(current_scene_path, path)
	await tree.process_frame ## Pequeño respiro para que listeners reaccionen

	# Cambio inmediato por ahora. Más adelante añadiremos transiciones (fade/crossfade)
	var err := tree.change_scene_to_file(path)
	if err != OK:
		push_error("[SceneManager] Error al cargar escena '%s' (code: %d)" % [path, err])
		return

	current_scene_path = path
	scene_changed.emit(current_scene_path)

## Reinicia la escena actual
func reload_current() -> void:
	if current_scene_path.is_empty():
		return
	go_to(current_scene_path)

## Vuelve al menú principal
func go_to_main_menu() -> void:
	go_to(Constants.SCENE_MAIN_MENU)
