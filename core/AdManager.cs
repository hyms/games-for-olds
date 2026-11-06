using System;
using ClasicosMenteActiva.core;
using Godot;

/// <summary>
///     Gestor de publicidad (plan Fase 3 §3.4 · spec §8.9).
///     En el MVP el proveedor es <b>no-op</b>: la publicidad real entra en la Fase 8.
///     Solo banners en menú principal y estadísticas, nunca en partida.
/// </summary>
public partial class AdManager : Node
{
    [Signal]
    public delegate void BannerFailedEventHandler(string location);

    private static readonly string[] AllowedLocations =
    {
        Constants.AdLocationMainMenu,
        Constants.AdLocationStatistics
    };

    public bool IsEnabled { get; private set; }
    public bool ConsentGranted { get; private set; }

    public override void _Ready()
    {
        GD.Print("[AdManager] modo no-op (MVP). Sin anuncios cargados.");
    }

    public void ShowBanner(string location)
    {
        if (!IsEnabled || !ConsentGranted || !IsLocationAllowed(location)) return;

        GD.Print($"[AdManager] banner solicitado en '{location}' (no-op)");
    }

    public void HideBanner(string location)
    {
        if (!IsLocationAllowed(location)) return;

        GD.Print($"[AdManager] banner ocultado en '{location}' (no-op)");
    }

    public void GrantConsent()
    {
        ConsentGranted = true;
    }

    public bool IsLocationAllowed(string location)
    {
        foreach (var allowed in AllowedLocations)
            if (string.Equals(location, allowed, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    public void SimulateBannerFailed(string location)
    {
        if (IsLocationAllowed(location)) EmitSignal(SignalName.BannerFailed, location);
    }
}