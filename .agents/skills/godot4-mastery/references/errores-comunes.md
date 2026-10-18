# Errores reales de parser/runtime en Godot 4.7 (y sus fixes)

Catálogo vivo. Cada entrada = error tal cual lo reporta el editor → causa → fix.
Añadir aquí todo error nuevo que aparezca en este proyecto.

## §1 `Parser Error: Cannot get return value of call to "reverse()" because it returns "void"`

**Causa:** `Array.reverse()` y `Array.sort()`/`shuffle()` devuelven **`void`**
(revierten in-place). No pueden usarse como expresión.

```gdscript
var r := arr.reverse()                 # ❌ void
var r: Array[Vector2i] = arr.duplicate().reverse()  # ❌ idem
```

**Fix:**

```gdscript
var r: Array[Vector2i] = arr.duplicate()  # 1) copia
r.reverse()                               # 2) mutación in-place (retorno ignorado)
# o iterar al revés: for i in range(arr.size() - 1, -1, -1):
```

**Distinguir:** `String.reverse()` SÍ devuelve `String` (copia) — ahí está
permitido usar el retorno: `var rev: String = s.reverse()`.

## §2 `Parser Error: Cannot find member "reversed" in base "String"`

**Causa:** no existe `.reversed()` en `String` ni en `Array` de GDScript
(eso es Python). Surge al "arreglar" §1 cambiando todo a `reversed()`.

**Fix:** `String` → `.reverse()` (devuelve copia). `Array` → `duplicate()` +
`.reverse()` in-place (ver §1). Verificado contra docs stable:
`String.reverse()` = *Returns a reversed copy*; `Array.reverse()` = `void`.

## §3 `Cannot infer type of "x"` / `The variable type is being inferred from...`

**Causa:** `:=` con expresión no trivialmente inferible (llamada a método,
`get_node()`, operación con operandos de distinto tipo int/float).

```gdscript
var err := tree.change_scene_to_file(p)   # ❌ ¿Error? ¿Variant?
var tree := get_tree()                    # ❌ el analizador no infiere SceneTree
var value := complex_function()           # ❌
```

**Fix:** tipo explícito SIEMPRE (regla del proyecto):

```gdscript
var err: Error = tree.change_scene_to_file(p)
var tree: SceneTree = get_tree()
var value: Resultado = complex_function()
```

`:=` solo con literales/constructores obvios: `var d := Vector2(48, 48)`.

## §4 `Invalid call. Nonexistent function 'push_info' in base ...`

**Causa:** `push_info()` no existe en GDScript (confusión con el depurador de
otros lenguajes).

**Fix:** `print(...)` para información; `push_error(...)` / `push_warning(...)`
cuando se quiera stack trace en el debugger.

## §5 `_get_global_rect()` / "Cannot override"

**Causa:** método no virtual en Godot 4 (no está en la lista de sobreescribibles).

**Fix:** no sobreescribir. Los virtuales válidos de Control son pocos
(`_get_minimum_size`, `_gui_input`, `_notification`, `_draw`, `_process`, etc.);
ante la duda consultar APIDOC antes de escribir `func _...`.

## §6 Referencia a nodo `null` / "Cannot call on a previously freed instance"

**Causa:** tras `queue_free()`/cambio de escena la referencia queda invalidada
**sin convertirse en `null`**; comparar con `null` no detecta nada.

**Fix:**

```gdscript
if is_instance_valid(cell):     # ✅ única comprobación fiable
    cell.set_selected(false)
if cell.has_method("set_letter"):   # para duck-typing seguro
    cell.set_letter(letter)
```

## §7 `Invalid call. Nonexistent function 'instantiate' in base 'Nil'`

**Causa:** `preload`/`load` de escena mal escrito o path inexistente, o nodo
recibido por `@onready` con `%Nombre` sin marcar "Access as Unique Name".

**Fix:** verificar ruta del recurso; en el editor, clic derecho en el nodo ▸
*Access as Unique Name*; validar `if x:` antes de llamar.

## §8 Cambio de escena: `current_scene` todavía es la vieja

**Causa:** `change_scene_to_file()` aplica el cambio **al final del frame**.

**Fix:** `await get_tree().process_frame` (o `await get_tree().scene_changed`)
antes de castear `tree.current_scene`.

## §9 Errores de tipado en arrays: `Cannot assign a value of type X to Array[Y]`

**Causa:** `Array[T]` es invariante; un `Array[Node]` no se asigna a
`Array[Control]`, y `Array[Array[int]]` ni siquiera está permitido.

**Fix:** tipos exactos desde la declaración; para arrays de arrays usar
`Array[Array]` sin tipo interno o estructuras propias (clase inner).

## §10 `.tres`/`.tscn`: UIDs regenerados tras abrir el editor

**Síntoma:** el editor reescribe `uid://...` en archivos escritos a mano.

**No es bug** — es el importador asignando UIDs. Hacer commit de los `.uid` y
`.tscn` regenerados; no "arreglarlos" a mano.

## Protocolo ante un error nuevo

1. Copiar el mensaje **exacto** del editor.
2. Buscarlo en este archivo (§).
3. Si es nuevo: leer la firma real del método en Context7
   (`query-docs` sobre `/websites/godotengine_en_stable`, p. ej.
   *"Array reverse return type"*) — **nunca asumir** que un método devuelve
   algo sin ver `## TipoMetodo(...) -> Tipo` o `### Returns`.
4. Aplicar el fix, commit, y **añadir la entrada aquí**.
