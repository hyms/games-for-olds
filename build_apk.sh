#!/usr/bin/env bash
set -e

# Configuración de rutas y binarios
GODOT_BIN="${GODOT_BIN:-$HOME/Downloads/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64}"
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$PROJECT_PATH/builds"
APK_NAME="ClasicosMenteActiva2D-v0.4.0.apk"

echo "=========================================="
echo " Compilando APK v0.4.0 - Sopa de Letras"
echo "=========================================="

# 1. Actualizar versión a 0.4.0 en project.godot si fuera necesario
if grep -q 'config/version="0.1.0"' "$PROJECT_PATH/project.godot"; then
    echo "-> Actualizando versión a 0.4.0 en project.godot..."
    sed -i 's/config\/version="0.1.0"/config\/version="0.4.0"/g' "$PROJECT_PATH/project.godot"
fi

# 2. Compilar C# (.NET 8.0)
echo "-> Compilando proyecto C# con dotnet..."
export DOTNET_ROLL_FORWARD=LatestMajor
dotnet build "$PROJECT_PATH/ClasicosMenteActiva2D.csproj" -c Release

# 3. Crear directorio de salidas
mkdir -p "$OUTPUT_DIR"

# 4. Exportar APK con Godot Headless
echo "-> Exportando APK con Godot..."
if [ ! -f "$GODOT_BIN" ]; then
    echo "¡Error! No se encuentra el binario de Godot en: $GODOT_BIN"
    echo "Exporta la variable GODOT_BIN con la ruta correcta."
    exit 1
fi

if [ ! -f "$PROJECT_PATH/export_presets.cfg" ]; then
    echo "Aviso: No se encontró export_presets.cfg. Creado automáticamente."
fi

"$GODOT_BIN" --path "$PROJECT_PATH" --headless --export-release "Android" "$OUTPUT_DIR/$APK_NAME"

echo "=========================================="
echo " ¡Build completada con éxito!"
echo " APK generado en: $OUTPUT_DIR/$APK_NAME"
echo "=========================================="
