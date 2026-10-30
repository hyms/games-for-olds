# Pruebas — GdUnit4Net

> Estructura de pruebas del proyecto. Convenciones y comandos; los requisitos
> están en [`docs/test.md`](../docs/test.md).

## Instalación

- Paquetes NuGet en `ClasicosMenteActiva2D.csproj`: `gdUnit4.api` 5.1.0-rc6,
  `gdUnit4.test.adapter` 3.2.0, `gdUnit4.analyzers` 1.1.0 y
  `Microsoft.NET.Test.Sdk`.
- Requiere **Godot 4.7 .NET (mono)** y **.NET SDK ≥ 8**.
- El addon `addons/gdUnit4` (v6.2.2) queda para la integración del editor; los
  tests del proyecto son C#.

## Estructura

| Carpeta | Contenido | Convención |
|---------|-----------|------------|
| `tests/unit/` | Tests puros de lógica y recursos | `*Test.cs`, clase con `[TestSuite]` |
| `tests/integration/` | Tests con escenas (SceneRunner) | `*Test.cs`, clase con `[TestSuite]` |

Cada test que toque objetos de Godot (recursos, nodos, escenas) lleva
`[RequireGodotRuntime]`. Los IDs (`UT-WS-…`, `IT-WS-…`, `ST-WS-…`) se documentan
en [`docs/test.md`](../docs/test.md) y
[`docs/word_search/test.md`](../docs/word_search/test.md); la cabecera del
fichero indica a qué ID pertenece.

## Ejecución

En el editor: panel **GdUnit4** (solo descubre tests GDScript) o el explorador de
tests del IDE (VS Code / Rider) mediante el adaptador VSTest.

Desde terminal, en la raíz del proyecto:

```bash
export GODOT_BIN="$HOME/Downloads/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64"
dotnet test
```

Notas:

- `GODOT_BIN` es la ruta al **binario de Godot .NET**, no al estándar.
- Si solo tienes el runtime .NET 10 instalado y el proyecto apunta a `net8.0`,
  añade `DOTNET_ROLL_FORWARD=LatestMajor`.
- `dotnet test` devuelve código distinto de cero si algún test falla.

## Exportación

Cuando exista `export_presets.cfg`, excluir `tests/` y `addons/gdUnit4/*` de los
builds para que el framework no se empaquete en el APK.
