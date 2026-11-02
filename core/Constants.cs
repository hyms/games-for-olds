using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
/// Rutas, grupos y medidas compartidas del proyecto.
/// Equivalente C# de <c>core/Constants.gd</c>.
/// </summary>
public static class Constants
{
    /// <summary>Escena del menú principal.</summary>
    public const string SceneMainMenu = "res://ui/main_menu/MainMenu.tscn";

    /// <summary>Escena de estadísticas.</summary>
    public const string SceneStatistics = "res://ui/stats/StatsScreen.tscn";

    /// <summary>Escena de ajustes.</summary>
    public const string SceneSettings = "res://ui/settings/SettingsScreen.tscn";

    /// <summary>Grupo de controles con área táctil ampliada.</summary>
    public const string GroupTouchable = "touchable";

    /// <summary>Grupo de controles navegables por teclado/mando.</summary>
    public const string GroupUiFocusable = "ui_focusable";

    /// <summary>Nombre del autoload que gestiona las escenas.</summary>
    public const string AutoloadSceneManager = "SceneManager";

    /// <summary>Tamaño mínimo táctil recomendado en Android (px lógicos).</summary>
    public static readonly Vector2 MinTouchSize = new(56, 56);

    // --- Sopa de Letras ------------------------------------------------------

    /// <summary>Id del nivel de dificultad por defecto (Fácil, C6).</summary>
    public const string DefaultDifficultyId = "facil";

    /// <summary>Ruta de la fuente en negrita para celdas encontradas.</summary>
    public const string FontBoldPath = "res://assets/fonts/AtkinsonHyperlegible-Bold.ttf";

    // --- Audio (valores por defecto) ----------------------------------------

    /// <summary>Volumen por defecto de la música de fondo (0.0-1.0).</summary>
    public const float DefaultBgmVolume = 0.5f;

    /// <summary>Volumen por defecto de efectos de sonido (0.0-1.0).</summary>
    public const float DefaultSfxVolume = 0.7f;

    // --- Publicidad (ubicaciones permitidas en el MVP) ----------------------

    /// <summary>Ubicación permitida: menú principal.</summary>
    public const string AdLocationMainMenu = "main_menu";

    /// <summary>Ubicación permitida: pantalla de estadísticas.</summary>
    public const string AdLocationStatistics = "statistics";
}