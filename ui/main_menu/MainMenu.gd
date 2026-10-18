extends Control

## MainMenu
## Punto de entrada. Usa SceneManager para navegar (desacoplado).
## Más adelante conectaremos a las escenas reales de cada minijuego.

const THEME_SELECTOR_SCENE := "res://ui/theme_selector/ThemeSelector.tscn"

@onready var btn_word_search: BigButton = %BtnWordSearch
@onready var btn_tangram: BigButton = %BtnTangram
@onready var btn_blocks: BigButton = %BtnBlocks
@onready var btn_quit: BigButton = %BtnQuit

func _ready() -> void:
	# Conexiones con señales Godot 4 (.connect)
	if is_instance_valid(btn_word_search):
		btn_word_search.pressed.connect(_on_word_search_pressed)
	if is_instance_valid(btn_tangram):
		btn_tangram.pressed.connect(_on_tangram_pressed)
	if is_instance_valid(btn_blocks):
		btn_blocks.pressed.connect(_on_blocks_pressed)
	if is_instance_valid(btn_quit):
		btn_quit.pressed.connect(_on_quit_pressed)

	# Enfoque inicial accesible (teclado/mandos si se usa)
	if is_instance_valid(btn_word_search):
		btn_word_search.grab_focus()

func _on_word_search_pressed() -> void:
	SceneManager.go_to(THEME_SELECTOR_SCENE)

func _on_tangram_pressed() -> void:
	print("[MainMenu] Tangram/Puzles de encaje aún no implementado.")
	# SceneManager.go_to("res://minigames/tangram/Tangram.tscn")

func _on_blocks_pressed() -> void:
	print("[MainMenu] Lógica de Bloques (estilo Dr. Mario) aún no implementado.")
	# SceneManager.go_to("res://minigames/blocks/Blocks.tscn")

func _on_quit_pressed() -> void:
	get_tree().quit()
