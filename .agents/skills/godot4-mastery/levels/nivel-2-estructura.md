# Nivel 2 — Estructura: señales, recursos, escenas (Godot 4.7)

Extraído de `signals`, `groups`, `resources`, `nodes_and_scene_instances`,
`singletons_autoload`, `scene_tree`, `instancing_with_signals` (docs stable).

## 1. Señales (observer pattern — columna vertebral del proyecto)

### Declarar

```gdscript
signal word_found(word: String)              # con tipo (recomendado en este proyecto)
signal all_words_found                       # sin payload
signal health_changed(old_value: int, new_value: int)  # con argumentos tipados
```

- Los argumentos declarados **deben** pasarse en `emit()`.
- Nombres en pasado, snake_case (`word_found`, no `find_word`).

### Conectar (Callable, API de Godot 4)

```gdscript
# En _ready del padre/consumidor:
word_grid.word_found.connect(_on_word_found)
btn_back.pressed.connect(_on_back_pressed)
btn_theme1.pressed.connect(_on_theme_selected.bind(0))  # argumentos extra al final

# Callback con la firma del signal (+ los bind() al final):
func _on_word_found(word: String) -> void:
    found_words += 1

# Desconectar al salir si el emisor sobrevive:
word_grid.word_found.disconnect(_on_word_found)
```

- `connect()` devuelve `Error` si falla; en código normal se ignora, pero si el
  nodo podría no existir: `if is_instance_valid(x): x.sig.connect(...)`.
- Editor: pestaña **Node** del dock para conectar sin código (misma mecánica).
- `await señal` para flujo secuencial: `await get_tree().process_frame`,
  `await get_tree().scene_changed`.

### Decisión señal vs referencia directa

- **Señal** cuando el emisor no debe saber quién escucha (subescenas, celdas →
  grid, grid → HUD). El hijo emite, el padre conecta.
- **Referencia directa** solo para llamadas descendentes puntuales vía
  `@onready` (`ws.start_with_theme()`), siempre con null-check.

## 2. Grupos (broadcast a muchos nodos)

```gdscript
add_to_group("enemies")
var guards: Array[Node] = get_tree().get_nodes_in_group("guards")
get_tree().call_group("enemies", "hide")                 # método en cada nodo
get_tree().call_group_flags(
    SceneTree.GROUP_CALL_DEFERRED | SceneTree.GROUP_CALL_REVERSE,
    "enemies", "hide")                                   # al final del frame, orden inverso
```

- Útil para "resetear todas las celdas" o "pausar todo" sin referencias.
- Grupos marcados con `persistent = true` se guardan dentro del `.tscn`.

## 3. Resources (datos del juego) — ver también skill `godot-resources`

```gdscript
class_name WordSearchTheme
extends Resource

@export var theme_name: String = "General"
@export_multiline var description: String = ""
@export var words: Array[String] = []
@export_range(5, 12) var grid_size: int = 8
@export_range(3, 15) var max_words: int = 8
@export var allow_diagonal: bool = true
```

- `.tres` = texto (diff-friendly, editar en Inspector) — formato de este proyecto.
- Carga: `preload("res://...")` si la ruta es constante; `load()` /
  `ResourceLoader.exists(path)` si es dinámica (Selector de temas).
- Guarda: `ResourceSaver.save(res, "res://tema.tres")`.
- Exponer en un nodo: `@export var current_theme: WordSearchTheme` y asignar en
  el Inspector; en runtime `if current_theme:` antes de usar.

## 4. PackedScene: instanciar y destruir

```gdscript
# Constante (compile-time) — preferido:
const CELL_SCENE: PackedScene = preload("res://minigames/word_search/WordCell.tscn")

func _build_grid() -> void:
    var cell: WordCell = CELL_SCENE.instantiate() as WordCell   # tipar SIEMPRE con `as`
    add_child(cell)                      # 1) añadir AL ÁRBOL primero
    cell.grid_pos = Vector2i(x, y)       # 2) luego configurar posición/estado
    cell.cell_pressed.connect(_on_cell_pressed)

# Destruir:
cell.queue_free()   # ✅ seguro: elimina al final del frame
# cell.free()       # ⚠️ inmediato; solo para nodos NUNCA añadidos al árbol
```

- Tras `queue_free()`, toda referencia existente queda **invalidada sin pasar
  por `null`**: usa `is_instance_valid(ref)` antes de acceder (no `if ref == null`).
- `has_method("x")` antes de llamadas dinámicas: `if obj.has_method("smash"): obj.smash()`.

## 5. Cambiar de escena

```gdscript
# Forma oficial y chequeada:
var tree: SceneTree = get_tree()
var err: Error = tree.change_scene_to_file("res://minigames/word_search/WordSearch.tscn")
if err != OK:
    push_error("[Scene] no se pudo cargar la escena")

# Esperar a que la nueva escena esté accesible (patrón del proyecto):
await tree.process_frame
var ws: WordSearch = tree.current_scene as WordSearch
if ws:
    ws.start_with_theme(selected)

# Reiniciar la escena actual:
get_tree().reload_current_scene()
```

- `change_scene_to_file()` **devuelve `Error`** (se puede capturar; también se
  puede ignorar).
- El cambio ocurre al final del frame → **nunca** asumas `current_scene` nuevo
  en la línea siguiente sin `await process_frame` (o `await scene_changed`).

### Autoload (mínimo en este proyecto)

```gdscript
# autoloads/SceneManager.gd — único singleton del proyecto
signal scene_changing(path: String)
signal scene_changed(path: String)

func go_to_main_menu() -> void:
    scene_changing.emit(MAIN_MENU)
    get_tree().change_scene_to_file(MAIN_MENU)
```

- Autoloads sobreviven a los cambios de escena → solo para estado global real
  (navegación, audio global, guardado). No meter lógica de juego ahí.
- Señales globales desde autoload = desacople entre escenas sin referencias.

## 6. Deferred: ejecutar al final del frame

```gdscript
grab_focus.call_deferred()                 # Callable
node.call_deferred("rotate", angle)
node.set_deferred("disabled", true)         # la propiedad NO cambia hasta el idle
var v: int = 1
print(v)  # sigue 1 hasta await get_tree().process_frame
```

- **Obligatorio** para operaciones que tocan el árbol durante un callback de
  señal/física: `add_child`, `queue_free` de nodos en colisión, `grab_focus`
  al cambiar de escena.
- `call_deferred` siempre devuelve `null`; no llamarlo recursivamente.

## 7. Flujo de datos entre escenas (patrón del proyecto)

```
MainMenu ──(change_scene_to_file)──▶ ThemeSelector
ThemeSelector: carga .tres ──elegir─▶ change_scene ──await process_frame──▶
              (current_scene as WordSearch).start_with_theme(selected)
WordSearch._ready(): conecta word_grid.word_found / all_words_found → HUD
WordGrid emite ↑ (hijo no sabe quién escucha)
```

Regla: **los datos viajan hacia abajo por parámetros** (`start_with_theme`),
**los eventos viajan hacia arriba por señales** (`word_found`).
