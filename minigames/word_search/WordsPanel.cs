using System.Collections.Generic;
using ClasicosMenteActiva.core;
using Godot;

namespace ClasicosMenteActiva.minigames.word_search;

/// <summary>
///     Panel inferior de la Sopa de Letras: placa retro con marco de píxel de
///     2 px que muestra las palabras pendientes y las encontradas separadas por
///     <c> | </c>. Las encontradas se tachan (<c>[s]PALABRA[/s]</c>) en el color
///     turquesa del tema y, al completar una palabra, se dispara confeti píxel
///     (<see cref="PixelConfettiEmitter" />) sobre el punto indicado.
/// </summary>
/// <remarks>
///     El marco, el radio y la sombra dura provienen de
///     <see cref="PixelStyles.WordPanel" />; el tema solo aporta la paleta
///     (fondo y borde) para conservar la legibilidad en modo oscuro.
///     El <see cref="RichTextLabel" /> usa <c>FitContent = false</c> y ajusta la
///     altura mínima del panel al alto real del texto para que las listas de
///     varias líneas nunca se recorten.
/// </remarks>
public partial class WordsPanel : PanelContainer
{
    private const int WordFontSize = 16;
    private const int LineSeparation = 2;
    private const float MinPanelHeight = 70.0f;
    private const float MaxPanelHeight = 160.0f;

    private RichTextLabel _wordList;
    private StyleBoxFlat _stylePanel;
    private PixelConfettiEmitter _confetti;

    private ICollection<string> _lastPending;
    private IReadOnlyList<string> _lastFound;
    private ThemeData _theme;

    /// <summary>Texto de la lista (expuesto para inspección y pruebas).</summary>
    public string WordListText => _wordList?.Text ?? string.Empty;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        _stylePanel = PixelStyles.WordPanel();
        AddThemeStyleboxOverride("panel", _stylePanel);

        // Crear RichTextLabel explícitamente con el nombre correcto.
        _wordList = CreateWordList();
        AddChild(_wordList);
        _wordList.Finished += QueueHeightSync;

        _confetti ??= new PixelConfettiEmitter { Name = "Confetti" };
        AddChild(_confetti);

        CustomMinimumSize = new Vector2(CustomMinimumSize.X, MinPanelHeight);

        var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        ApplyTheme(theme?.CurrentThemeData);
        if (theme != null) theme.OnThemeChanged += OnThemeChanged;
    }

    public override void _ExitTree()
    {
        var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        if (theme != null) theme.OnThemeChanged -= OnThemeChanged;
        base._ExitTree();
    }

    private void OnThemeChanged(ThemeData themeData) => ApplyTheme(themeData);

    private void ApplyTheme(ThemeData theme)
    {
        if (theme == null) return;

        _theme = theme;

        if (_stylePanel != null)
        {
            _stylePanel.BgColor = theme.CellIdleBackgroundColor;
            _stylePanel.BorderColor = theme.CellIdleBorderColor;
            AddThemeStyleboxOverride("panel", _stylePanel);
        }

        if (_wordList != null)
        {
            _wordList.AddThemeColorOverride("default_color",
                PixelStyles.TextColor(theme.CellIdleBackgroundColor));

            // Re-renderiza para refrescar el color del tachado sin esperar a la
            // próxima palabra encontrada.
            if (_lastPending != null && _lastFound != null)
                _wordList.Text = WordSearchHud.BuildWordList(_lastPending, _lastFound, theme);
        }
    }

    /// <summary>
    ///     Construye el texto del panel: primero las pendientes en el orden del
    ///     tablero y al final las encontradas, tachadas (<c>[s]WORD[/s]</c>) y en
    ///     color turquesa. Separadas por <c> | </c>. El texto activo usa color
    ///     oscuro para máximo contraste sobre el fondo claro.
    /// </summary>
    public void SetWords(ICollection<string> pending, IReadOnlyList<string> found, ThemeData theme)
    {
        if (_wordList == null) return;

        _lastPending = pending;
        _lastFound = found;
        if (theme != null) _theme = theme;

        // Construir texto con formato explícito de color alto contraste.
        var text = BuildWordListWithFormatting(pending, found, _theme);
        _wordList.Text = text;
        QueueHeightSync();
    }

    private static string BuildWordListWithFormatting(ICollection<string> pending,
        IReadOnlyList<string> found, ThemeData theme)
    {
        var text = new System.Text.StringBuilder();
        var textColor = PixelStyles.TextColor(theme?.CellIdleBackgroundColor ?? Color.FromHtml("#EBF2F5"));
        var strikeColor = theme?.FooterStrikethroughColor ?? PixelStyles.Turquesa;
        var strikeHex = strikeColor.ToHtml(false);

        foreach (var word in pending)
        {
            if (text.Length > 0) text.Append(" | ");
            text.Append("[color=#");
            text.Append(textColor.ToHtml(false));
            text.Append("]");
            text.Append(word);
            text.Append("[/color]");
        }

        foreach (var word in found)
        {
            if (text.Length > 0) text.Append(" | ");
            text.Append("[color=#");
            text.Append(strikeHex);
            text.Append("][s]");
            text.Append(word);
            text.Append("[/s][/color]");
        }

        return text.ToString();
    }

    /// <summary>Dispara confeti píxel sobre una posición global (p. ej. una celda).</summary>
    public void TriggerConfetti(Vector2 globalPosition)
    {
        if (_confetti == null) return;

        // CpuParticles2D usa coordenadas locales del panel.
        var localPosition = GetGlobalTransform().AffineInverse() * globalPosition;
        _confetti.EmitAt(localPosition);
    }

    /// <summary>
    ///     Ajusta la altura mínima del panel al alto real del texto (más los
    ///     márgenes del marco), acotada para no comerse el tablero.
    /// </summary>
    private void QueueHeightSync()
    {
        Callable.From(SyncHeight).CallDeferred();
    }

    private void SyncHeight()
    {
        if (_wordList == null || _stylePanel == null) return;

        var contentHeight = _wordList.GetContentHeight();
        if (contentHeight <= 0) return;

        var padding = _stylePanel.ContentMarginTop + _stylePanel.ContentMarginBottom;
        var target = Mathf.Clamp(contentHeight + padding, MinPanelHeight, MaxPanelHeight);

        if (Mathf.IsEqualApprox(CustomMinimumSize.Y, target)) return;
        CustomMinimumSize = new Vector2(CustomMinimumSize.X, target);
    }

    private static RichTextLabel CreateWordList()
    {
        var label = new RichTextLabel
        {
            Name = "WordList",
            MouseFilter = MouseFilterEnum.Ignore,
            BbcodeEnabled = true,
            FitContent = false,
            ScrollActive = false,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (PixelStyles.CellFont != null)
            label.AddThemeFontOverride("normal_font", PixelStyles.CellFont);
        label.AddThemeFontSizeOverride("normal_font_size", WordFontSize);
        label.AddThemeColorOverride("default_color", PixelStyles.Tinta);
        label.AddThemeConstantOverride("line_separation", LineSeparation);
        return label;
    }
}

/// <summary>Emisor de confeti píxel basado en <see cref="CpuParticles2D" />.</summary>
public partial class PixelConfettiEmitter : CpuParticles2D
{
    public override void _Ready()
    {
        base._Ready();

        Lifetime = 0.6f;
        Amount = 30;
        OneShot = true;
        Explosiveness = 1.0f;
        SpeedScale = 1.0f;
        Gravity = new Vector2(0, 30);
        Color = PixelStyles.Turquesa;
        Emitting = false;
    }

    public void EmitAt(Vector2 position)
    {
        Position = position;
        Amount = 30;
        Restart();
    }
}
