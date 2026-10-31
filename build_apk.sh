#!/usr/bin/env bash
set -e

# Compilación local del APK de Android sin intervención del agente.
# Requisitos: JDK 17+, Android SDK y plantillas de exportación de Godot 4.7.2 .NET.

# Configuración de rutas y binarios
GODOT_BIN="${GODOT_BIN:-$HOME/Downloads/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64}"
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$PROJECT_PATH/builds"
APK_NAME="ClasicosMenteActiva2D-v0.4.0.apk"
APK_PATH="$OUTPUT_DIR/$APK_NAME"
KEYSTORE="$PROJECT_PATH/android/debug.keystore"

echo "=========================================="
echo " Compilando APK v0.4.0 - Sopa de Letras"
echo "=========================================="

# 1. Keystore de depuración (firma v2 del APK; no se versiona)
if [ ! -f "$KEYSTORE" ]; then
    echo "-> Generando keystore de depuración..."
    mkdir -p "$PROJECT_PATH/android"
    keytool -genkey -v -keystore "$KEYSTORE" -alias debugkey -keyalg RSA \
        -keysize 2048 -validity 10000 -storepass android -keypass android \
        -dname "CN=Debug,O=Debug,C=ES" >/dev/null 2>&1
fi

# 2. Compilar C# (net9.0, runtime del SDK Godot 4.7.2)
echo "-> Compilando proyecto C# con dotnet..."
export DOTNET_ROLL_FORWARD=LatestMajor
dotnet build "$PROJECT_PATH/ClasicosMenteActiva2D.csproj" -c Release

# 3. Asegurar un JDK 17+ para Gradle (Godot lo lee de sus ajustes del editor)
EDITOR_SETTINGS="$HOME/.config/godot/editor_settings-4.7.tres"
JDK_PATH="$(ls -d /usr/lib/jvm/java-21-openjdk-amd64 /usr/lib/jvm/java-17-openjdk-amd64 2>/dev/null | head -1)"
if [ -n "$JDK_PATH" ] && [ -f "$EDITOR_SETTINGS" ]; then
    if grep -q '^export/android/java_sdk_path' "$EDITOR_SETTINGS"; then
        sed -i "s|^export/android/java_sdk_path = .*|export/android/java_sdk_path = \"$JDK_PATH\"|" "$EDITOR_SETTINGS"
    else
        printf 'export/android/java_sdk_path = "%s"\n' "$JDK_PATH" >> "$EDITOR_SETTINGS"
    fi
    echo "-> JDK para Gradle: $JDK_PATH"
fi

# 4. Exportar APK con Godot headless (Gradle release + firma)
echo "-> Exportando APK con Godot..."
if [ ! -f "$GODOT_BIN" ]; then
    echo "¡Error! No se encuentra el binario de Godot en: $GODOT_BIN"
    echo "Exporta la variable GODOT_BIN con la ruta correcta."
    exit 1
fi

mkdir -p "$OUTPUT_DIR"
rm -f "$APK_PATH"

# Godot puede devolver un código distinto de 0 por avisos de ADB aunque el APK
# se haya generado; la comprobación real es que el archivo exista.
"$GODOT_BIN" --path "$PROJECT_PATH" --headless --export-release "Android" "$APK_PATH" || true

if [ ! -f "$APK_PATH" ]; then
    echo "¡Error! No se generó el APK. Revisa la salida de Godot."
    exit 1
fi

echo "=========================================="
echo " ¡Build completada con éxito!"
echo " APK generado en: $APK_PATH"
echo " Tamaño: $(du -h "$APK_PATH" | cut -f1)"
echo "=========================================="
