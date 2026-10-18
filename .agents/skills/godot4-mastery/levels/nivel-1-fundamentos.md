# Nivel 1 — Fundamentos de GDScript (Godot 4.7)

Extraído de `gdscript_basics`, `gdscript_styleguide`, `static_typing`,
`overridable_functions`, `idle_and_physics_processing` (docs stable).

## 1. Tipado estático (la regla #1 de este proyecto)

```gdscript
# Explícito cuando el tipo es ambiguo o hay aritmética int/float:
var health: int = 0            # NO: var health := 0 (¿int o float querías?)

# Inferido SOLO cuando es obvio en la lectura:
var direction := Vector2(1, 0) # constructor literal → OK
var speed := 200.0             # float literal → OK

# PROHIBIDO: inferir desde llamadas no triviales (origen de "Cannot infer type"):
var value := complex_function()          # ❌ ilegible e inferencia frágil
var value: Resultado = complex_function()# ✅

# Referencias a nodos: SIEMPRE tipo explícito (get_node() devuelve Node):
@onready var lbl: Label = %LblTheme            # ✅
@onready var lbl := get_node("LblTheme")       # ❌ tipado como Node
```

- Tipos compuestos: `Array[int]`, `Array[String]`, `Array[Vector2i]`,
  `Dictionary[String, int]`. **No existen arrays anidados tipados**
  (`Array[Array[int]]` está prohibido).
- Firmas siempre con tipo de retorno: `func f(a: int) -> void:`; `void` no
  acepta `return valor`.
- Los literales `0`/`1` son `int`; para float escribe `0.0`. Una división
  `int / int` trunca (warning `INTEGER_DIVISION`).

## 2. Anotaciones

| Anotación | Uso | Ejemplo |
|---|---|---|
| `@export` | exponer al Inspector | `@export var words: Array[String] = []` |
| `@export_range(a, b, paso, flags)` | slider numérico | `@export_range(5, 12) var grid_size: int = 8` |
| `@export_enum(...)` | desplegable | `@export_enum("Fácil","Medio") var diff: int` |
| `@export_file("*.txt")` | selector de ruta | `@export_file("*.tres") var path: String` |
| `@export_multiline` | textarea | `@export_multiline var description: String` |
| `@onready` |赋值 al entrar al árbol (antes de `_ready`) | `@onready var btn: BigButton = %BtnBack` |
| `@tool` | ejecutar en el editor | solo si lo necesitas realmente |
| `class_name` | registra la clase global | `class_name WordSearch` |

- Los campos export **deben tener valor por defecto** si van en un `_init` con
  parámetros, o el Inspector falla al crear/editar el recurso.

## 3. Ciclo de vida de nodos

```
_enter_tree()   → padre antes que hijos (al entrar al árbol)
_ready()        → hijos antes que PADRE; una sola vez (request_ready() lo repite)
_process(delta) → cada frame renderizado (delta = tiempo del frame anterior)
_physics_process(delta) → tick fijo de física (~60 Hz), antes del paso físico
_exit_tree()    → hijos antes que padre
```

- **Todo movimiento se multiplica por `delta`** para ser independiente de FPS:
  `position.x += speed * delta`.
- Física/kinemática va en `_physics_process`; UI, animaciones y lógica de juego
  en `_process`.
- `delta` es `float` (segundos). `Time.get_ticks_msec()` para cronómetros.

## 4. Referencias a nodos (orden de preferencia de la docs)

1. `@onready var x: Tipo = %Nombre` — **único y más rápido**; marca el nodo como
   "Access as Unique Name" en el árbol de escena (prefijo `%`). No se rompe si
   el nodo se mueve en la jerarquía.
2. `@onready var x: Tipo = $Ruta/Corta` — rápido, se rompe si se reordena.
3. `get_node("Ruta")` en tiempo de ejecución — lento, para rutas dinámicas.
4. `@export var x: Node` — asignado desde el Inspector; válido si el nodo puede
   vivir fuera de la escena.

Si el nodo puede no existir, valida: `if not is_instance_valid(ref): return`.

## 5. Strings y format

```gdscript
var msg: String = "Palabras: %d/%d" % [found, total]   # array si hay >1 placeholder
var single: String = "Tema: %s" % theme.theme_name      # un solo valor: sin corchetes
print("Avance: %d%%" % 50)   # % literal → doble %%

# Propiedades clave (devuelven copia, no mutan):
var upper: String = s.to_upper()
var rev: String = s.reverse()      # ✅ String.reverse() devuelve String copia
var clean: String = s.strip_edges()
var slice: String = s.substr(0, 3)
var n: int = s.length()            # o s.size()
# Iterar caracteres: for c in s:  → c es String de 1 char
```

⚠️ `String` **no** tiene `.reversed()`. Ver `referencias/errores-comunes.md` §1.

## 6. Arrays: mutación vs copia

```gdscript
var a: Array[int] = [3, 1, 2]
a.reverse()      # ⚠️ VOID, in-place. NO sirve como expresión: var b := a.reverse() ❌
a.sort()         # igual: VOID in-place
a.shuffle()      # VOID in-place
var b: Array[int] = a.duplicate()  # copia shallow; luego b.reverse()
var first: int = a[0]              # negativos: a[-1] = último
a.erase(value)   # por valor;  a.remove_at(index)  # por índice
var has: bool = a.has(x)
var i: int = a.find(x)             # -1 si no está
# Iterar al revés (sin reverse()):
for idx in range(a.size() - 1, -1, -1):
    print(a[idx])
```

## 7. Debug y errores

```gdscript
print("ok", value)                      # consola normal
push_error("[WordGrid] grid vacío")     # rojo + stack trace (para errores)
push_warning("tema sin palabras")       # amarillo + stack trace
assert(pos >= 0, "posición inválida")   # SOLO debug builds; sin side effects en la condición
```

- **No existe `push_info()`** → `print()`.
- La docs recomienda `push_error`/`push_warning` sobre `print` para diagnóstico
  porque añaden stack trace y se distinguen de la salida normal.

## 8. Convenciones de nombres (styleguide oficial)

| Elemento | Formato | Ejemplo |
|---|---|---|
| archivos, funciones, variables | snake_case | `word_grid.gd`, `_mark_found()` |
| variables/funciones privadas | prefijo `_` | `_validate_selection()` |
| clases, nodos, enums | PascalCase | `WordGenerator`, `WordCell` |
| constantes, miembros de enum | CONSTANT_CASE | `DIRECTIONS`, `THEME_SCENE` |
| señales | pasado, snake_case | `word_found`, `all_words_found` |
| señales con payload | `sujeto_acción` pasado | `health_changed(old, new)` |

## 9. Comprobación pre-commit para `.gd`

- [ ] Todo `var` con `: Tipo =` explícito salvo constructores literales obvios.
- [ ] Toda función con `-> Tipo` (incluido `-> void`).
- [ ] Ningún método void usado como valor (`:=`, `return`, argumento).
- [ ] Ningún `.reversed()`; `reverse()` solo donde el tipo devuelto se usa (String) o ignorando retorno (Array).
- [ ] Sin `push_info`; `print`/`push_error`/`push_warning` en su lugar.
- [ ] Movimiento multiplicado por `delta`.
