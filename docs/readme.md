# Clásicos Mente Activa 2D

> Colección modular de minijuegos clásicos desarrollada en **Godot Engine 4.x (C# / .NET)**, aplicando patrones de arquitectura limpia, bajo acoplamiento y estética retro de **Pixel Art Puro**.

---

## 🛠️ Stack Tecnológico y Requisitos
- **Motor de Videojuegos:** Godot Engine 4.x (Edición .NET / C#)
- **Lenguaje:** C# (.NET 6.0 / 8.0 SDK)
- **Gestor de Paquetes / Dependencias:** NuGet (integrado en el ecosistema .NET)

---

## 🏛️ Arquitectura de Software y Patrones de Diseño

El proyecto sigue una arquitectura modular orientada a componentes y dominios desacoplados:

1. **Singleton Pattern (`ThemeManager.cs`):**
   - Administra los tokens globales de color (`ThemeData`) e inyecta actualizaciones en tiempo real a todas las vistas suscritas mediante eventos `event Action`.
2. **Factory Pattern (`WordGridFactory.cs`):**
   - Encargado de la instanciación y distribución procedural de las celdas del tablero según la dificultad del minijuego.
3. **Strategy Pattern (`IWordPlacementStrategy.cs`):**
   - Desacopla los algoritmos de colocación de palabras (horizontal, vertical, diagonal, inverso) cumpliendo con el principio abierto/cerrado (OCP).
4. **Observer Pattern (Eventos C# `event Action`):**
   - Comunicación unidireccional y desacoplada para señales de juego (aciertos de palabras, uso de pistas, finalización de niveles y menú de pausa).

### Principios de Arquitectura:
- **SOLID:** Especial foco en *Single Responsibility Principle (SRP)* y *Open/Closed Principle (OCP)*.
- **DRY & KISS:** Centralización de temas/estilos y código conciso sin sobre-ingeniería.
- **YAGNI:** Implementación estricta de lo requerido por la especificación.

---

## 🎨 Estética Visual y Reglas de Arte (Pixel Art Puro)

- **Cero Suavizado:** Todo elemento visual (`StyleBoxFlat`, fuentes, bordes) se configura estrictamente con `AntiAliasing = false` y `CornerRadius = 0`.
- **Garantía Anti-Fantasma (Opacidad 100%):** Prohibido el uso de canales alfa reducidos (`< 1.0`) o transparencias en celdas activas o encontradas.
- **Jerarquía Tipográfica Oficial:**
  - `PressStart2P.ttf`: Títulos principales y banners de cabecera (`PixelBanner`).
  - `VCR_OSD_Mono.ttf`: HUD de control, cronómetro (`00:50`), contadores, menú de pausa y botones (`PixelButton`).
  - `PixelOperator.ttf`: Celdas del tablero (`PixelCell`) y lista de palabras pendientes en el panel inferior (`WordsPanel`).

---

## 📂 Organización y Estructura del Proyecto (Godot 4 C#)

```text
res://
│
├── core/
│   ├── ThemeData.cs              # Clases de datos para tokens de color globales
│   ├── ThemeManager.cs           # Singleton de gestión y cambio de temas en tiempo real
│   └── AccessibleTheme.tres      # Recursos base de tema Godot
│
├── scenes/
│   ├── ui/                       # UI Global y transversal reutilizable
│   │   ├── components/
│   │   │   ├── PixelButton.cs / .tscn   # Botón táctil estándar con profundidad
│   │   │   ├── PixelBanner.cs / .tscn   # Encabezados con marco retro de 3px
│   │   │   ├── PixelHeaderHUD.cs / .tscn# Barra superior (Atrás, Pausa, Cronómetro)
│   │   │   └── PixelConfettiEmitter.cs  # Partículas CpuParticles2D para victorias
│   │   └── menus/
│   │       └── PauseMenu.cs / .tscn     # Menú de pausa global superpuesto
│   │
│   └── minigames/
│       └── word_search/          # Módulo específico del minijuego Sopa de Letras
│           ├── WordsPanel.cs     # Panel inferior: palabras pendientes y tachados
│           ├── WordSearchMain.cs # Coordinador principal de lógica y eventos
│           ├── WordGridFactory.cs# Factoría de construcción de tableros
│           └── strategies/       # Algoritmos de colocación de palabras
│
├── doc/
│   └── word_search/
│       └── spec.md               # Especificación técnica detallada del minijuego
│
└── README.md
```

---

## ⚙️ Compilación y Ejecución

### 1. Prerrequisitos
- Tener instalado **Godot 4.x (Edición .NET)**.
- Tener instalado el **.NET SDK** compatible (6.0 o superior).

### 2. Compilación de C# (.NET CLI)
Puedes verificar y compilar la solución C# desde la terminal raíz con:
```bash
dotnet build --configuration Debug
```
*(Debe compilar con `0 Error(s)` y `0 Warning(s)`)*.

### 3. Ejecución en Godot
- Abre el **Godot Editor (versión .NET)**.
- Importa el proyecto seleccionando el archivo `project.godot`.
- Presiona **F5** para ejecutar la escena principal.