using System.Collections.Generic;
using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
///     Gestor de márgenes de seguridad (Safe Area / Notch / Acordeón).
///     Asegura que elementos críticos (título, HUD, panel de palabras) no se
///     queden ocultos detrás de la cámara frontal, el acordeón o los bordes
///     curvados del dispositivo. Consulta las inserciones reales del display
///     (<see cref="DisplayServer.GetDisplaySafeArea" />) y aplica al
///     <see cref="MarginContainer" /> objetivo el máximo entre el mínimo
///     configurado (<see cref="SafeArea" />) y el margen del dispositivo,
///     convertido a píxeles lógicos del viewport. Escucha
///     <see cref="Window.SignalName.SizeChanged" /> para rotaciones / acordeón.
/// </summary>
public partial class SafeAreaManager : Node
{
    /// <summary>Identificador de este panel dentro del mapeo de áreas seguras.</summary>
    public const string PanelIdMainMenu = "main_menu";

    /// <summary>Identificador de este panel dentro del mapeo de áreas seguras.</summary>
    public const string PanelIdWordSearch = "word_search";

    /// <summary>Panel al que aplica esta instancia del gestor.</summary>
    [Export] public string PanelId { get; set; } = PanelIdWordSearch;

    /// <summary>Trayectoria al <see cref="MarginContainer" /> que recibe los márgenes seguros.</summary>
    [Export] public NodePath TargetMarginPath { get; set; }

    /// <summary>Áreas seguras definidas por el usuario.</summary>
    private readonly Dictionary<string, SafeArea> _safeAreas = new();

    private MarginContainer _targetMargin;

    public override void _Ready()
    {
        Setup();

        _targetMargin = TargetMarginPath != null && !TargetMarginPath.IsEmpty
            ? GetNodeOrNull<MarginContainer>(TargetMarginPath)
            : null;

        ApplySafeAreas();

        // NOTIFICATION_WM_* solo llega al Window, no a nodos hijos; por eso se
        // suscribe a SizeChanged de la ventana (rotación, acordeón, notch).
        var window = GetWindow();
        if (window != null) window.SizeChanged += ApplySafeAreas;
    }

    public override void _ExitTree()
    {
        var window = GetWindow();
        if (window != null) window.SizeChanged -= ApplySafeAreas;
        base._ExitTree();
    }

    public void Setup()
    {
        _safeAreas[PanelIdMainMenu] = new SafeArea
        {
            MinTop = 8,
            MinBottom = 8,
            MinLeft = 8,
            MinRight = 8
        };

        _safeAreas[PanelIdWordSearch] = new SafeArea
        {
            MinTop = 32, // Espacio para HUD superior + título + notificación de acordeón
            MinBottom = 8,
            MinLeft = 8,
            MinRight = 8
        };
    }

    public void ApplySafeAreas()
    {
        if (_safeAreas.TryGetValue(PanelId, out var area)) ApplySafeArea(PanelId, area);
    }

    public void ApplySafeArea(string panelId, SafeArea area)
    {
        if (panelId != PanelId || _targetMargin == null) return;

        var insets = ReadDisplayInsets();
        _targetMargin.AddThemeConstantOverride("margin_top", Mathf.Max(area.MinTop, insets.Top));
        _targetMargin.AddThemeConstantOverride("margin_bottom", Mathf.Max(area.MinBottom, insets.Bottom));
        _targetMargin.AddThemeConstantOverride("margin_left", Mathf.Max(area.MinLeft, insets.Left));
        _targetMargin.AddThemeConstantOverride("margin_right", Mathf.Max(area.MinRight, insets.Right));
    }

    /// <summary>Inserciones del display en píxeles lógicos (0 si no hay notch/acordeón).</summary>
    private static RectInsets ReadDisplayInsets()
    {
        var insets = new RectInsets();

        // El rectángulo seguro es relativo a la pantalla física: en móvil la
        // ventana la cubre completa, así que la inserción se lee directamente.
        // En plataformas sin soporte devuelve el rectángulo usable (inserciones 0).
        var safe = DisplayServer.GetDisplaySafeArea();
        var screenSize = DisplayServer.ScreenGetSize();
        var scale = Mathf.Max(DisplayServer.ScreenGetScale(), 1.0f);

        static int Logical(float px, float scale)
        {
            return Mathf.Max(0, (int)Mathf.Round(px / scale));
        }

        insets.Top = Logical(safe.Position.Y, scale);
        insets.Bottom = Logical(screenSize.Y - (safe.Position.Y + safe.Size.Y), scale);
        insets.Left = Logical(safe.Position.X, scale);
        insets.Right = Logical(screenSize.X - (safe.Position.X + safe.Size.X), scale);
        return insets;
    }

    private struct RectInsets
    {
        public int Bottom;
        public int Left;
        public int Right;
        public int Top;
    }

    /// <summary>
    ///     Representa un área de seguridad para un panel.
    /// </summary>
    public class SafeArea
    {
        public int MinBottom = 8;
        public int MinLeft = 8;
        public int MinRight = 8;
        public int MinTop = 8;
    }
}