# Pruebas — GdUnit4

> Estructura de pruebas del proyecto. Convenciones y comandos; los requisitos
> están en [`docs/test.md`](../docs/test.md).

## Instalación

- Addon: `addons/gdUnit4` (v6.2.2), plugin activo en `project.godot`.
- Requiere Godot ≥ 4.5 (el proyecto usa 4.7).

## Estructura

| Carpeta | Contenido | Convención |
|---------|-----------|------------|
| `tests/unit/` | Tests puros, sin escenas | `*Test.gd`, `extends GdUnitTestSuite` |
| `tests/integration/` | Tests con escenas (SceneRunner) | `*Test.gd`, `extends GdUnitTestSuite` |

Los IDs de los casos (`UT-WS-…`, `IT-WS-…`, `ST-WS-…`) se documentan en
[`docs/test.md`](../docs/test.md) y en [`docs/word_search/test.md`](../docs/word_search/test.md);
el nombre del fichero indica a qué ID pertenece en el comentario de cabecera.

## Ejecución

En el editor: **Proyecto → Pruebas → GdUnit4** (panel lateral).

Headless (CI o terminal), desde la raíz del proyecto:

```bash
./addons/gdUnit4/runtest.sh --godot_binary /ruta/a/godot -a res://tests
```

Alternativa sin el script:

```bash
godot --headless -s addons/gdUnit4/bin/GdUnitCmdTool.gd -a res://tests
```

El proceso devuelve código distinto de cero si algún test falla.

## Exportación

Cuando exista `export_presets.cfg`, excluir `addons/gdUnit4/*` de los builds
para que el framework no se empaquete en el APK.
