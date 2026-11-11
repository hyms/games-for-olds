using System;
using Godot;

namespace ClasicosMenteActiva.core.ui;

/// <summary>
///     Modal de finalización de partida reutilizable y orientado al estado.
///     Soporta <see cref="CompletionState.Victory" /> (confeti + métricas de
///     éxito) y <see cref="CompletionState.Defeat" /> (sin confeti, color de
///     error, acciones reintentar/salir). No contiene dependencias específicas
///     del Sopa de Letras; la lógica de juego (confeti, reinicio, menú) se
///     inyecta mediante callbacks.
/// </summary>
public partial class CompletionModal : PanelContainer
{
    /// <summary>Estado de finalización que determina el comportamiento del modal.</summary>
    public enum CompletionState
    {
        Victory,
        Defeat
    }

    /// <summary>Emitido cuando se debe lanzar el estallido de confeti (solo Victory).</summary>
    [Signal]
    public delegate void ConfettiRequestedEventHandler(Vector2 position);

    /// <summary>Emitido cuando el modal se oculta.</summary>
    [Signal]
    public delegate void ClosedEventHandler();

    private Label _title;
    private Label _message;
    private Label _score;
    private Label _time;
    private BigButton _btnRetry;
    private BigButton _btnQuit;

    private Action _onRetry;
    private Action _onQuit;

    public override void _Ready()
    {
        base._Ready();

        _title = GetNodeOrNull<Label>("%WinTitle");
        _message = GetNodeOrNull<Label>("%WinMessage");
        _score = GetNodeOrNull<Label>("%WinScore");
        _time = GetNodeOrNull<Label>("%WinTime");
        _btnRetry = GetNodeOrNull<BigButton>("%BtnNextGame");
        _btnQuit = GetNodeOrNull<BigButton>("%BtnWinMenu");

        if (_btnRetry != null) _btnRetry.Pressed += OnRetryPressed;
        if (_btnQuit != null) _btnQuit.Pressed += OnQuitPressed;

        Visible = false;
    }

    /// <summary>
    ///     Muestra el modal con el estado dado. Los callbacks de reintento y
    ///     salida se ejecutan tras cerrar el modal.
    /// </summary>
    public void Show(
        CompletionState state,
        string title,
        string message,
        string scoreText,
        string timeText,
        Vector2? confettiPosition = null,
        Action onRetry = null,
        Action onQuit = null)
    {
        _onRetry = onRetry;
        _onQuit = onQuit;

        if (_title != null) _title.Text = title;
        if (_message != null) _message.Text = message;
        if (_score != null) _score.Text = scoreText;
        if (_time != null) _time.Text = timeText;

        // Estilo consistente con el menú de pausa (docs/art_direction.md §2).
        AddThemeStyleboxOverride("panel", PixelStyles.ModalBackdrop());
        var card = GetNodeOrNull<PanelContainer>("%WinCard");
        if (card != null)
            card.AddThemeStyleboxOverride("panel", PixelStyles.ModalCard());

        if (state == CompletionState.Victory)
        {
            ApplyVictoryStyling();
            if (confettiPosition.HasValue)
                EmitSignal(SignalName.ConfettiRequested, confettiPosition.Value);
            SetButtonVisibility(_btnRetry, true);
            SetButtonVisibility(_btnQuit, true);
        }
        else
        {
            ApplyDefeatStyling();
            SetButtonVisibility(_btnRetry, true);
            SetButtonVisibility(_btnQuit, true);
        }

        Visible = true;
        _btnRetry?.GrabFocus();
    }

    /// <summary>Oculta el modal y emite <see cref="Closed" />.</summary>
    public void Close()
    {
        Visible = false;
        EmitSignal(SignalName.Closed);
    }

    /// <summary>
    ///     Emite <see cref="ConfettiRequested" /> sin mostrar el modal.
    ///     Útil para lanzar el confeti antes de que el modal aparezca
    ///     (spec §4.3: el confeti debe ser visible antes del bloqueo).
    /// </summary>
    public void EmitConfetti(Vector2 position)
    {
        EmitSignal(SignalName.ConfettiRequested, position);
    }

    private void OnRetryPressed()
    {
        Close();
        _onRetry?.Invoke();
    }

    private void OnQuitPressed()
    {
        Close();
        _onQuit?.Invoke();
    }

    private void SetButtonVisibility(BigButton button, bool visible)
    {
        if (button != null) button.Visible = visible;
    }

    private void ApplyVictoryStyling()
    {
        if (_title != null)
            _title.AddThemeColorOverride("font_color", PixelStyles.Turquesa);
    }

    private void ApplyDefeatStyling()
    {
        if (_title != null)
            _title.AddThemeColorOverride("font_color", new Color(0.82f, 0.2f, 0.2f, 1.0f));
    }
}
