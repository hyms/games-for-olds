# Nivel 3 — Interfaz, entrada táctil, animación y export (Godot 4.7)

Extraído de `custom_gui_controls`, `gui_skinning`, `scene_unique_nodes`,
`input_examples`, `pausing_games`, Tween/Control/Input APIDOC,
`upgrading_to_godot_4.7`, `command_line_tutorial` (docs stable).

## 1. UI con Control: layout responsivo

- **Anclas (`anchor_*`)**: fracciones 0–1 del rect del padre donde se "clava"
  cada borde. Usa los presets de la barra **Layout** (`set_anchors_preset()`)
  en vez de colocar píxeles a mano:
  - `Full Rect` — ocupa todo el padre (contenedores raíz, overlays).
  - `Center` — centrado (modales, botones grandes).
  - `Top Wide`, `Bottom Wide` — barras HUD superior/inferior.
- **Deja que los Containers posicionen**: `VBoxContainer`, `HBoxContainer`,
  `GridContainer`, `MarginContainer`. Nunca posiciones manualmente dentro de un
  container (pelea contigo).
- Tamaño mínimo: `custom_minimum_size = Vector2(56, 56)` en el editor, o
  `set_custom_minimum_size(...)` en código; los contenedores lo respetan.
  Control custom puede sobreescribir `_get_minimum_size() -> Vector2`.
- **Estabilidad móvil**: con stretch `canvas_items` + `keep`/`expand`, los
  anchors hacen que la UI sobreviva cualquier resolución.

### Theme y overrides (por control concreto)

```gdscript
# Fuente/tamaño/color puntuales (precedencia sobre el Theme global):
lbl.add_theme_font_override("font", my_font)
lbl.add_theme_font_size_override("font_size", 24)
lbl.add_theme_color_override("font_color", Color("#1a1a2e"))
btn.add_theme_stylebox_override("normal", stylebox)

# Constantes de containers:
margin_container.add_theme_constant_override("margin_top", 16)

# Leer del theme en uso:
var c: Color = get_theme_color("font_color")
var fs: int = get_theme_default_font_size()
```

- `SystemFont` (recurso) resuelve una fuente del SO por lista de nombres →
  usado en `AccessibleTheme.tres` para no depender de binarios.
- Un `Theme` asignado a un nodo raíz se hereda hacia abajo por tipo de control;
  los overrides locales ganan.

## 2. Accesibilidad 40+ (requisitos del proyecto)

- Targets táctiles ≥ **56×56 px** con `custom_minimum_size` (auditar celdas,
  botones, filas del selector de temas).
- Contraste alto: texto oscuro sobre fondo claro o viceversa; verificar los
  estados `normal / hover / pressed / focused / disabled` del theme.
- Feedback de estado **siempre visible y no solo por color** (p. ej. palabra
  encontrada: color + tachado/marcado, no solo verde).
- Navegación por foco para mando/teclado: `grab_focus()`, `focus_neighbor`,
  orden de foco; en el editor pestaña **Node → Directional**.

## 3. Entrada: eventos vs polling, táctil

```gdscript
# Evento (discreto): botón pulsado AHORA
func _unhandled_input(event: InputEvent) -> void:
    if event.is_action_pressed("ui_accept"):
        restart()

# Polling (continuo): estado mantenido → va en _process/_physics_process
func _physics_process(delta: float) -> void:
    if Input.is_action_pressed("move_right"):
        position.x += speed * delta
```

- **InputMap** abstrae hardware→acción: define acciones (`ui_accept`, `tap`,
  `restart`) en Project Settings ▸ Input Map y consúltalas SIEMPRE por acción,
  nunca por tecla/código de botón.
- Táctil: `Input.emulate_mouse_from_touch` / Project Settings
  `input_devices/pointing/emulate_mouse_from_touch` (activado en este proyecto)
  → los `BaseButton` responden a toques como si fueran clics de ratón, y
  `_gui_input`/`pressed` funcionan sin código extra.
- Eventos de pantalla crudos (arrastré de selección en la sopa):
  `InputEventScreenTouch` / `InputEventScreenDrag` en `_input(event)` —
  para futuras mejoras de drag-to-select.
- Drag de UI: `Control.mouse_filter` (`STOP` consume, `PASS` propaga,
  `IGNORE` ignora) — relevante si un panel tapa celdas.

## 4. Animación con Tween (sin escenas de animación)

```gdscript
# Creado y ligado al nodo → se auto-destruye si el nodo muere:
var tween: Tween = create_tween()
tween.tween_property(self, "modulate", Color.WHITE, 0.3)          # secuencial
tween.tween_property(self, "scale", Vector2.ONE * 1.2, 0.2)

# En paralelo y con ease:
var tw: Tween = create_tween().set_parallel(true)
tw.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
tw.tween_property(cell, "scale", Vector2(1.15, 1.15), 0.12)
tw.chain().tween_property(cell, "scale", Vector2.ONE, 0.12)       # después de las paralelas

# Esperar a que termine:
await tw.finished
```

- `tween_property(obj, "propiedad", valor_final, duración_seg)`.
- `set_parallel(true)` → los tweeners siguientes corren a la vez; `chain()`
  vuelve a lo secuencial para el resto.
- `tween_method` para valores calculados por frame; `tween_callback` para
  disparar lógica al final (alternativa a `await`).
- `kill()` si el tween debe cancelarse; matar un tween terminado es inocuo.

### Feedback de victoria (uso previsto en Sopa de Letras)

```gdscript
func _on_all_found() -> void:
    var tw: Tween = create_tween().set_parallel(true)
    tw.tween_property(word_grid, "modulate", Color(0.8, 1.0, 0.8), 0.4)
    tw.tween_property(lbl_words, "scale", Vector2(1.1, 1.1), 0.4)
    # + mensaje "¡Completado!" y SFX
```

## 5. Pausa

```gdscript
get_tree().paused = true                     # pausa todo lo PAUSABLE
# Menú de pausa: su raíz debe seguir activo:
pause_menu.process_mode = Node.PROCESS_MODE_WHEN_PAUSED   # (hereda a sus hijos)
# Nodos que deben pausarse:
node.process_mode = Node.PROCESS_MODE_PAUSABLE            # default
node.process_mode = Node.PROCESS_MODE_ALWAYS              # música global, HUD
```

- `SceneTree.paused` no afecta a nodos `ALWAYS` ni al menú en `WHEN_PAUSED`.

## 6. Export y resolución (móvil-first)

- Proyecto: viewport **360×640**, stretch **`canvas_items`** (en 4.7 el default
  nuevo es `canvas_items` + `expand`; este proyecto fija aspect para control).
  Siempre probar en un emulador con resolución distinta.
- Export con plantillas instaladas:

```bash
godot --headless --export-release "Android" /ruta/juego.apk
# La binaria debe ser la del editor, no la plantilla.
# Plantillas: Editor ▸ Manage Export Templates.
```

- Android: keystore (debug/release), `Editor ▸ Project ▸ Export ▸ Android`,
  arquitecturas arm64-v8a. Release mínimo recomendado para tiendas.
- Guardado de escenas de UI: si exportas con `.tres`/`.tscn` escritos a mano,
  Godot los reimporta en la primera apertura (UIDs pueden regenerarse — normal).

## 7. Checklist de UI móvil accesible

- [ ] Todo target táctil ≥ 56×56.
- [ ] Layout por anchors/containers, probado a 360×640 y en tablet.
- [ ] Focus navigation configurada (flechas/direccional).
- [ ] Estados visibles de pressed/focused en el theme.
- [ ] Entrada por acciones InputMap, no códigos de tecla.
- [ ] `emulate_mouse_from_touch` activo → botones responden al toque.
- [ ] Mensajes de estado con texto + color (no solo color).
