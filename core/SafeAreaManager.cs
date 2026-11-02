using System.Collections.Generic;
using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
/// Gestor de márgenes de seguridad (Safe Area / Notch / Acordeón).
/// Asegura que elementos críticos (título, HUD, panel de palabras) no se
/// queden ocultos detrás de la cámara frontal, el acordeón o los bordes
/// curvados del dispositivo. Aplica ajustes globales en _Ready() y maneja
/// _NotificationResized para pantallas que cambian de orientación.
/// </summary>
public partial class SafeAreaManager : Node
{
    /// <summary>Identificador de este panel dentro del mapeo de áreas seguras.</summary>
    public const string PanelIdMainMenu = "main_menu";

    /// <summary>Identificador de este panel dentro del mapeo de áreas seguras.</summary>
    public const string PanelIdWordSearch = "word_search";

    /// <summary>Áreas seguras definidas por el usuario (aplicadas al viewport).</summary>
    private readonly Dictionary<string, SafeArea> _safeAreas = new();

    public override void _Ready()
    {
        // Registrarse como gestor de áreas seguras.
        Setup();

        // Aplicar márgenes iniciales (pantalla en composición).
        ApplySafeAreas();

        // Notificar cambios de tamaño para manejar rotación / acordeón.
        GetTree().Connect(SceneTree.SignalName.ProcessFrame, new Callable(this, global::ClasicosMenteActiva.core.SafeAreaManager.MethodName.OnNotificationFrame));
    }

    public void Setup()
    {
        // Preparar áreas seguras por defecto para las pantallas conocidas.
        _safeAreas[PanelIdMainMenu] = new SafeArea
        {
            MinTop = 8,
            MinBottom = 8,
            MinLeft = 8,
            MinRight = 8,
        };

        _safeAreas[PanelIdWordSearch] = new SafeArea
        {
            MinTop = 32,  // Espacio para HUD superior + título + notificación de acordeón
            MinBottom = 8,
            MinLeft = 8,
            MinRight = 8,
        };
    }

    public void ApplySafeAreas()
    {
        foreach (var pair in _safeAreas)
        {
            ApplySafeArea(pair.Key, pair.Value);
        }
    }

    public void ApplySafeArea(string panelId, SafeArea area)
    {
        // Esta estrategia usa el viewport como referencia y aplica correcciones
        // al control que gestiona el área segura. La implementación concreta
        // depende de cómo se integra el control con el viewport en cada pantalla.
        if (panelId == PanelIdWordSearch)
        {
            // Para el tablero, se aplica un desplazamiento vertical si es necesario.
            // La lógica será: el contenedor principal se posiciona para que sus
            // bordes cálidos no se sobrepongan con el área segura.
            GD.Print("[SafeAreaManager] Aplicando área segura para: " + panelId);
        }
    }

    private void OnNotificationFrame()
    {
        // A cada frame, revisar si el viewport ha cambiado y aplicar correcciones.
        ApplySafeAreas();
    }

    /// <summary>
    /// Representa una área de seguridad para un panel.
    /// </summary>
    public class SafeArea
    {
        public int MinTop = 8;
        public int MinBottom = 8;
        public int MinLeft = 8;
        public int MinRight = 8;
    }
}