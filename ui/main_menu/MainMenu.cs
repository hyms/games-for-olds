using ClasicosMenteActiva.core;
using Godot;
using Godot.Collections;

/// <summary>
///     Menú principal: barra superior con salida (icono), carrusel horizontal
///     de juegos (arrastre o flechas) y botones secundarios compactos.
///     Desacoplado del resto de escenas mediante <see cref="SceneManager" />.
/// </summary>
public partial class MainMenu : Control
{
    private const int CompactHeight = 48;
    private const int CardWidth = 200;
    private const int CardHeight = 260;
    private const int RowSeparation = 14;

    private ScrollContainer _carousel;
    private HBoxContainer _row;
    private Control _spacerLeft;
    private Control _spacerRight;
    private GameCard _cardWordSearch;
    private GameCard _cardTangram;
    private GameCard _cardBlocks;
    private BigButton _btnQuit;
    private BigButton _btnStats;
    private BigButton _btnSettings;
    private Label _title;
    private Label _subtitle;

    private SceneManager _sceneManager;
    private readonly Array<GameCard> _cards = new();
    private int _activeIndex;
    private GameCard _activeCard;
    private float _lastScroll;

    public override void _Ready()
    {
        _sceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");
        _carousel = GetNodeOrNull<ScrollContainer>("Margin/VBox/Carousel");
        _row = GetNodeOrNull<HBoxContainer>("Margin/VBox/Carousel/Row");
        _spacerLeft = GetNodeOrNull<Control>("Margin/VBox/Carousel/Row/SpacerLeft");
        _spacerRight = GetNodeOrNull<Control>("Margin/VBox/Carousel/Row/SpacerRight");
        _cardWordSearch = GetNodeOrNull<GameCard>("Margin/VBox/Carousel/Row/GameCardWordSearch");
        _cardTangram = GetNodeOrNull<GameCard>("Margin/VBox/Carousel/Row/GameCardTangram");
        _cardBlocks = GetNodeOrNull<GameCard>("Margin/VBox/Carousel/Row/GameCardBlocks");
        _btnQuit = GetNodeOrNull<BigButton>("Margin/VBox/TopBar/BtnQuit");
        _btnStats = GetNodeOrNull<BigButton>("Margin/VBox/SecondaryRow/BtnStats");
        _btnSettings = GetNodeOrNull<BigButton>("Margin/VBox/SecondaryRow/BtnSettings");
        _title = GetNodeOrNull<Label>("Margin/VBox/TopBar/Title");
        _subtitle = GetNodeOrNull<Label>("Margin/VBox/Subtitle");

        if (_cardWordSearch != null) _cards.Add(_cardWordSearch);
        if (_cardTangram != null) _cards.Add(_cardTangram);
        if (_cardBlocks != null) _cards.Add(_cardBlocks);

        if (_cardWordSearch != null) _cardWordSearch.Configure("word_search", "Sopa de Letras",
            GD.Load<Texture2D>("res://assets/icons/magnifier.png"), true);
        if (_cardTangram != null) _cardTangram.Configure("tangram", "Tangram / Puzles",
            GD.Load<Texture2D>("res://assets/icons/star.png"), false);
        if (_cardBlocks != null) _cardBlocks.Configure("blocks", "Lógica de Bloques",
            GD.Load<Texture2D>("res://assets/icons/check.png"), false);

        if (_cardWordSearch != null) _cardWordSearch.Pressed += OnWordSearchPressed;
        if (_cardTangram != null) _cardTangram.Pressed += OnTangramPressed;
        if (_cardBlocks != null) _cardBlocks.Pressed += OnBlocksPressed;
        if (_btnStats != null) _btnStats.Pressed += OnStatsPressed;
        if (_btnSettings != null) _btnSettings.Pressed += OnSettingsPressed;
        if (_btnQuit != null) _btnQuit.Pressed += OnQuitPressed;

        if (_btnStats != null)
            _btnStats.CustomMinimumSize = new Vector2(_btnStats.CustomMinimumSize.X, CompactHeight);
        if (_btnSettings != null)
            _btnSettings.CustomMinimumSize = new Vector2(_btnSettings.CustomMinimumSize.X, CompactHeight);

        // Botón de salida cuadrado 56x56 con icono centrado (evita clamp Hud a 88 px de ancho).
        if (_btnQuit != null)
        {
            _btnQuit.CustomMinimumSize = new Vector2(56, 56);
            _btnQuit.Size = new Vector2(56, 56);
        }

        ApplyVisuals();

        // Oculta barras de desplazamiento; el arrastre sobre el contenido sigue funcionando.
        if (_carousel != null)
        {
            _carousel.GetHScrollBar().Visible = false;
            _carousel.GetVScrollBar().Visible = false;
            _carousel.Resized += RecomputeCarousel;
        }

        ApplyVisuals();

        // Paleta cálida: salida madera (WSFondo), secundarias crema (WSCremaSuave),
        // bordes WSMaderaBorde, sin colores hardcodeados en escena.
        ApplyWarmButtonStyle(_btnQuit, PixelStyles.WSFondo, PixelStyles.WSMaderaBorde, PixelStyles.TextLight, 0);
        ApplyWarmButtonStyle(_btnStats, PixelStyles.WSCremaSuave, PixelStyles.WSMaderaBorde, PixelStyles.TextDark, 4);
        ApplyWarmButtonStyle(_btnSettings, PixelStyles.WSCremaSuave, PixelStyles.WSMaderaBorde, PixelStyles.TextDark, 4);

        CallDeferred(MethodName.InitializeCarousel);
        CallDeferred(MethodName.EnsureExitIconCentered);

        if (_cards.Count > 0)
        {
            _activeIndex = 0;
            ActivateCard(_activeIndex);
            _activeCard.GrabFocus();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            if (key.Keycode == Key.Left)
            {
                MoveCarousel(-1);
                AcceptEvent();
                return;
            }
            if (key.Keycode == Key.Right)
            {
                MoveCarousel(1);
                AcceptEvent();
                return;
            }
        }
    }

    public override void _Process(double _delta)
    {
        if (_carousel == null || _cards.Count == 0) return;
        if (_carousel.Size.X <= 0) return;
        var scroll = _carousel.ScrollHorizontal;
        if (Mathf.Abs(scroll - _lastScroll) > 1f)
        {
            _lastScroll = scroll;
            UpdateActiveByScroll();
        }
    }

    private void InitializeCarousel()
    {
        if (_carousel == null || _activeCard == null) return;
        RecomputeCarousel();
        CenterOn(_activeCard);
        _lastScroll = _carousel.ScrollHorizontal;
    }

    private void RecomputeCarousel()
    {
        if (_carousel == null || _carousel.Size.X <= 0 || _cards.Count == 0) return;

        // Espaciadores laterales para que la primera y última tarjeta puedan centrarse.
        var peek = Mathf.Max(0f, (_carousel.Size.X - CardWidth) / 2f - RowSeparation);
        if (_spacerLeft != null) _spacerLeft.CustomMinimumSize = new Vector2(peek, 0);
        if (_spacerRight != null) _spacerRight.CustomMinimumSize = new Vector2(peek, 0);

        CenterOn(_activeCard);
        _lastScroll = _carousel.ScrollHorizontal;
        HideScrollbars();

        CallDeferred(MethodName.EnsureExitIconCentered);
    }

    private void HideScrollbars()
    {
        if (_carousel == null) return;
        var h = _carousel.GetHScrollBar();
        if (h != null) h.Visible = false;
        var v = _carousel.GetVScrollBar();
        if (v != null) v.Visible = false;
    }

    private void EnsureExitIconCentered()
    {
        if (_btnQuit != null)
            _btnQuit.IconAlignment = HorizontalAlignment.Center;
    }

    private void ActivateCard(int index)
    {
        index = Wrap(index, _cards.Count);
        if (_activeCard != null) _activeCard.SetActive(false);
        _activeIndex = index;
        _activeCard = _cards[index];
        _activeCard.SetActive(true);
    }

    private void MoveCarousel(int direction)
    {
        ActivateCard(_activeIndex + direction);
        CenterOn(_activeCard);
        _activeCard.GrabFocus();
    }

    private void UpdateActiveByScroll()
    {
        if (_cards.Count == 0 || _carousel.Size.X <= 0) return;

        var viewportCenterX = _carousel.ScrollHorizontal + _carousel.Size.X / 2f;
        GameCard nearest = null;
        var bestDist = float.MaxValue;

        foreach (var card in _cards)
        {
            if (card == null) continue;
            var rect = card.GetGlobalRect();
            var centerX = rect.Position.X + rect.Size.X / 2f;
            var dist = Mathf.Abs(centerX - viewportCenterX);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = card;
            }
        }

        if (nearest == null || nearest == _activeCard) return;
        _activeCard?.SetActive(false);
        _activeCard = nearest;
        _activeIndex = _cards.IndexOf(nearest);
        _activeCard.SetActive(true);
    }

    private void CenterOn(GameCard card)
    {
        if (card == null || _carousel == null || _carousel.Size.X <= 0 || _row == null || _row.Size.X <= 0) return;

        // Coordenadas de contenido (relativas a Row), sin depender de GetGlobalRect.
        var cardRect = card.GetRect();
        var targetScroll = cardRect.Position.X + cardRect.Size.X / 2f
            - _carousel.Size.X / 2f;

        var maxScroll = Mathf.Max(0f, _row.Size.X - _carousel.Size.X);
        _carousel.ScrollHorizontal = (int)Mathf.Clamp(targetScroll, 0f, maxScroll);
        _lastScroll = _carousel.ScrollHorizontal;
    }

    private static int Wrap(int value, int count) => ((value % count) + count) % count;

    /// <summary>
    ///     Aplica un estilo de botón plano cálido sobre los estilos base de
    ///     <see cref="PixelButton" />: relleno/borde con tokens de paleta (alpha 1.0),
    ///     esquina controlada y colores de texto/icono con contraste legible.
    ///     No hardcodea colores en escena; solo redefine styleboxes ya creados.
    /// </summary>
    private static void ApplyWarmButtonStyle(
        BigButton btn,
        Color fill,
        Color border,
        Color text,
        float cornerRadius)
    {
        if (btn == null) return;

        var opaqueFill = PixelStyles.Opaque(fill);
        var opaqueBorder = PixelStyles.Opaque(border);
        var textColor = PixelStyles.TextColor(fill);
        var disabledText = PixelStyles.TextColor(PixelStyles.Arena);

        var normal = new StyleBoxFlat
        {
            BgColor = opaqueFill,
            BorderColor = opaqueBorder,
            AntiAliasing = false,
            CornerDetail = 1,
            DrawCenter = true
        };
        normal.SetBorderWidthAll(PixelReliefPainter.BorderWidth);
        normal.SetCornerRadiusAll((int)cornerRadius);

        var hover = (StyleBoxFlat)normal.Duplicate();
        hover.BgColor = PixelStyles.Opaque(fill.Darkened(0.15f));
        var pressed = (StyleBoxFlat)normal.Duplicate();
        pressed.BgColor = PixelStyles.Opaque(fill.Darkened(0.3f));
        var focus = (StyleBoxFlat)normal.Duplicate();
        focus.BgColor = new Color(0, 0, 0, 0);
        focus.BorderColor = opaqueBorder;
        var disabled = (StyleBoxFlat)normal.Duplicate();
        disabled.BgColor = PixelStyles.Opaque(PixelStyles.Arena);

        btn.AddThemeStyleboxOverride("normal", normal);
        btn.AddThemeStyleboxOverride("hover", hover);
        btn.AddThemeStyleboxOverride("pressed", pressed);
        btn.AddThemeStyleboxOverride("hover_pressed", pressed);
        btn.AddThemeStyleboxOverride("focus", focus);
        btn.AddThemeStyleboxOverride("disabled", disabled);

        btn.AddThemeColorOverride("font_color", textColor);
        btn.AddThemeColorOverride("font_hover_color", textColor);
        btn.AddThemeColorOverride("font_pressed_color", textColor);
        btn.AddThemeColorOverride("font_focus_color", textColor);
        btn.AddThemeColorOverride("font_disabled_color", disabledText);

        btn.AddThemeColorOverride("icon_normal_color", textColor);
        btn.AddThemeColorOverride("icon_hover_color", textColor);
        btn.AddThemeColorOverride("icon_pressed_color", textColor);
        btn.AddThemeColorOverride("icon_focus_color", textColor);
        btn.AddThemeColorOverride("icon_disabled_color", disabledText);
    }

    private void ApplyVisuals()
    {
        if (GetNodeOrNull<ColorRect>("Background") is ColorRect bg)
            bg.Color = PixelStyles.Placa;

        if (_title != null)
        {
            _title.AddThemeFontOverride("font", PixelStyles.DisplayFont);
            _title.AddThemeFontSizeOverride("font_size", 20);
            _title.AddThemeColorOverride("font_color", PixelStyles.WSCianTitulo);
            _title.AddThemeColorOverride("font_outline_color", PixelStyles.WSMaderaSombra);
            _title.AddThemeConstantOverride("outline_size", 4);
        }

        if (_subtitle != null)
        {
            _subtitle.AddThemeFontOverride("font", PixelStyles.HudFont);
            _subtitle.AddThemeFontSizeOverride("font_size", 14);
            _subtitle.AddThemeColorOverride("font_color", PixelStyles.Tinta);
        }
    }

    private void OnWordSearchPressed()
    {
        _sceneManager.GoTo(DifficultySelector.ScenePath);
    }

    private void OnTangramPressed()
    {
        GD.Print("[MainMenu] Tangram/Puzles de encaje aún no implementado.");
    }

    private void OnBlocksPressed()
    {
        GD.Print("[MainMenu] Lógica de Bloques (estilo Dr. Mario) aún no implementado.");
    }

    private void OnStatsPressed()
    {
        _sceneManager.GoTo(Constants.SceneStatistics);
    }

    private void OnSettingsPressed()
    {
        _sceneManager.GoTo(Constants.SceneSettings);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
