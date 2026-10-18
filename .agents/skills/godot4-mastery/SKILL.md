---
name: godot4-mastery
description: >
  Guía propia de Godot 4.7 por niveles, generada desde la documentación oficial
  estable (docs.godotengine.org/en/stable vía Context7). Nivel 1: GDScript
  estático, anotaciones, ciclo de vida de nodos, naming, debug. Nivel 2: señales,
  grupos, Resources/.tres, PackedScene, cambio de escenas, deferred. Nivel 3:
  UI/Control y theme overrides, entrada táctil/InputMap, Tween, pausa, export
  Android. Incluye tabla de errores reales de parser/runtime (reverse() void,
  "Cannot infer type", push_info, nodos liberados). Úsala al editar .gd/.tscn/.tres
  de este proyecto, al corregir un "Parser Error" o "Cannot infer type", o al
  implementar cualquier minijuego (sopa de letras, tangram, bloques).
---

# Godot 4.7 — Guía propia por niveles (docs *stable*)

Fuente: <https://docs.godotengine.org/en/stable/> consultada con Context7
(librería `/websites/godotengine_en_stable`). Objetivo del proyecto: **Godot 4.7,
GDScript 2.0, móvil-first (Android), accesibilidad para adultos 40+**.

## Cómo usar este skill

1. Lee primero **referencias/errores-comunes.md** si llegaste con un error de
   parser o runtime reportado por el editor: casi todos los errores de este
   proyecto ya están catalogados ahí con su fix.
2. Carga el nivel que corresponda a la tarea:

   | Nivel | Archivo | Cargar cuando... |
   |-------|---------|------------------|
   | 1 | `levels/nivel-1-fundamentos.md` | escribas/corrijas `.gd`: variables, funciones, `@export`, `_ready`, tipado, strings, debug |
   | 2 | `levels/nivel-2-estructura.md` | conectes señales, crees Resources, instancies escenas, cambies de escena, pases datos entre escenas |
   | 3 | `levels/nivel-3-juego-interfaz.md` | hagas UI/Control, botones accesibles, input táctil, animaciones Tween, pausa o exportes a Android |

3. **Verifica APIs antes de usarlas.** Regla de oro aprendida a fuego en este
   proyecto: un método puede existir y aun así **devolver `void`** y no poder
   usarse como expresión. Antes de escribir una llamada nueva a API de Godot,
   consulta la firma en Context7 (`query-docs` sobre `/websites/godotengine_en_stable`,
   p. ej. *"Array reverse return type"*), mirando el bloque `## TipoMetodo(...)`
   y `### Returns`. Ver `referencias/errores-comunes.md` §1.

## Reglas de oro de este proyecto

- **Tipado explícito en toda declaración de variable**: `var x: int = 0`, nunca
  `var x := expresion_no_trivial`. El editor del usuario corre con análisis
  estricto y `:=` con llamadas a métodos dispara *"Cannot infer type"*.
  (`:=` solo se tolera con constructores literales obvios: `var d := Vector2(1,2)`.)
- **`String.reverse()` devuelve una copia; `Array.reverse()` es `void`** e
  in-place. No existe `.reversed()` en ningún a de los dos.
- Un **solo Autoload**: `SceneManager` (señales `scene_changing`/`scene_changed`);
  navegación por rutas string `res://...`.
- Datos como **`Resource` con `class_name` + `@export`** y `.tres` editables en
  el Inspector; nunca configura dura hardcodeada en scripts.
- **Desacoplamiento por señales** (emisor `emit()` → padre `connect()`), no
  referencias directas padre→hijo salvo `@onready`.
- **Accesibilidad**: botones ≥ 56×56 px, alto contraste, feedback visual claro
  en cada estado (normal/seleccionado/encontrado).
- `push_info()` **no existe** en GDScript → `print()` / `push_error()` /
  `push_warning()`.
- Solo `pnpm` (nunca `npm`/`npx`) para comandos de paquetes.

## Estructura típica de flujo (referencia rápida)

```gdscript
# Selector de tema → partida (patrón usado en ThemeSelector → WordSearch)
var err: Error = tree.change_scene_to_file(SCENE_PATH)
if err == OK:
    await tree.process_frame          # la nueva escena ya es current_scene
    var ws: WordSearch = tree.current_scene as WordSearch
    if ws:
        ws.start_with_theme(selected)
```
