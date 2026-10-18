extends Control
class_name ThemeSelector

## Selector de temáticas para WordSearch

@onready var btn_theme1: BigButton = %BtnTheme1
@onready var btn_theme2: BigButton = %BtnTheme2
@onready var btn_theme3: BigButton = %BtnTheme3
@onready var btn_theme4: BigButton = %BtnTheme4
@onready var btn_theme5: BigButton = %BtnTheme5
@onready var btn_back: BigButton = %BtnBack

var themes: Array[WordSearchTheme] = []

const THEME_SCENE := "res://minigames/word_search/WordSearch.tscn"
const THEME_PATHS := [
	"res://resources/themes/themes/animales.tres",
	"res://resources/themes/themes/frutas_verduras.tres",
	"res://resources/themes/themes/paises_ciudades.tres",
	"res://resources/themes/themes/objetos.tres",
	"res://resources/themes/themes/profesiones.tres",
]

func _ready() -> void:
	_load_themes()
	btn_theme1.pressed.connect(_on_theme_selected.bind(0))
	btn_theme2.pressed.connect(_on_theme_selected.bind(1))
	btn_theme3.pressed.connect(_on_theme_selected.bind(2))
	btn_theme4.pressed.connect(_on_theme_selected.bind(3))
	btn_theme5.pressed.connect(_on_theme_selected.bind(4))
	btn_back.pressed.connect(_on_back_pressed)
	if themes.size() > 0 and is_instance_valid(btn_theme1):
		btn_theme1.grab_focus()

func _load_themes() -> void:
	themes.clear()
	for path in THEME_PATHS:
		if ResourceLoader.exists(path):
			var t: WordSearchTheme = load(path) as WordSearchTheme
			if t:
				themes.append(t)
	if themes.size() > 0 and is_instance_valid(btn_theme1):
		btn_theme1.text = themes[0].theme_name
	if themes.size() > 1 and is_instance_valid(btn_theme2):
		btn_theme2.text = themes[1].theme_name
	if themes.size() > 2 and is_instance_valid(btn_theme3):
		btn_theme3.text = themes[2].theme_name
	if themes.size() > 3 and is_instance_valid(btn_theme4):
		btn_theme4.text = themes[3].theme_name
	if themes.size() > 4 and is_instance_valid(btn_theme5):
		btn_theme5.text = themes[4].theme_name

func _on_theme_selected(idx: int) -> void:
	if idx < 0 or idx >= themes.size():
		return
	var selected: WordSearchTheme = themes[idx]
	var tree: SceneTree = get_tree()
	var err := tree.change_scene_to_file(THEME_SCENE)
	if err == OK:
		await tree.process_frame
		var ws := tree.current_scene as WordSearch
		if ws:
			ws.start_with_theme(selected)
	else:
		push_error("[ThemeSelector] No se pudo cargar %s" % THEME_SCENE)

func _on_back_pressed() -> void:
	SceneManager.go_to_main_menu()
