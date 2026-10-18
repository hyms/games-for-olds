extends Control

## MainMenu
## Punto de entrada. Usa SceneManager para navegar (desacoplado).
## Más adelante conectaremos a las escenas reales de cada minijuego.

@onready var btn_word_search: BigButton = %BtnWordSearch
@onready var btn_tangram: BigButton = %BtnTangram
@onready var btn_blocks: BigButton = %BtnBlocks
@onready var btn_quit: BigButton = %BtnQuit

func _ready() -> void:
	# Conexiones con señales Godot 4 (.connect)
	btn_word_search.pressed.connect(_on_word_search_pressed)
	btn_tangram.pressed.connect(_on_tangram_pressed)
	btn_blocks.pressed.connect(_on_blocks_pressed)
	btn_quit.pressed.connect(_on_quit_pressed)

	# Enfoque inicial accesible (teclado/mandos si se usa)
	btn_word_search.grab_focus()

func _on_word_search_pressed() -> void:
	# TODO: Cambiar a res://minigames/word_search/WordSearch.tscn cuando exista
	push_info("[MainMenu] Sopa de Letras (procedural) aún no implementado. Se creará en el siguiente paso.")
	# SceneManager.go_to("res://minigames/word_search/WordSearch.tscn")

func _on_tangram_pressed() -> void:
	# TODO: Cambiar a res://minigames/tangram/Tangram.tscn cuando exista
	push_info("[MainMenu] Tangram/Puzles de encaje aún no implementado.")
	# SceneManager.go_to("res://minigames/tangram/Tangram.tscn")

func _on_blocks_pressed() -> void:
	# TODO: Cambiar a res://minigames/blocks/Blocks.tscn (estilo Dr. Mario) cuando exista
	push_info("[MainMenu] Lógica de Bloques (estilo Dr. Mario) aún no implementado.")
	# SceneManager.go_to("res://minigames/blocks/Blocks.tscn")

func _on_quit_pressed() -> void:
	get_tree().quit()
