extends Control

## WordSearch Game

@export var current_theme: WordSearchTheme

@onready var lbl_theme: Label = %LblTheme
@onready var lbl_words: Label = %LblWords
@onready var word_grid: WordGrid = %WordGrid
@onready var btn_back: BigButton = %BtnBack
@onready var btn_restart: BigButton = %BtnRestart
@onready var btn_hint: BigButton = %BtnHint

var total_words: int = 0
var found_words: int = 0

func _ready() -> void:
	btn_back.pressed.connect(_on_back_pressed)
	btn_restart.pressed.connect(_on_restart_pressed)
	btn_hint.pressed.connect(_on_hint_pressed)
	word_grid.word_found.connect(_on_word_found)
	word_grid.all_words_found.connect(_on_all_found)
	if current_theme:
		_start_game(current_theme)

## Inicia partida con un tema
func start_with_theme(theme: WordSearchTheme) -> void:
	current_theme = theme
	_start_game(theme)

func _start_game(theme: WordSearchTheme) -> void:
	lbl_theme.text = "Tema: %s" % theme.theme_name
	word_grid.setup(theme)
	total_words = word_grid.placements.size()
	found_words = 0
	_update_words_label()

func _update_words_label() -> void:
	lbl_words.text = "Palabras encontradas: %d/%d" % [found_words, total_words]

func _on_word_found(_word: String) -> void:
	found_words += 1
	_update_words_label()

func _on_all_found() -> void:
	lbl_words.text = "¡Todas encontradas! (%d/%d)" % [found_words, total_words]

func _on_back_pressed() -> void:
	SceneManager.go_to_main_menu()

func _on_restart_pressed() -> void:
	if current_theme:
		_start_game(current_theme)

func _on_hint_pressed() -> void:
	print("[WordSearch] Pista aún no implementada")
