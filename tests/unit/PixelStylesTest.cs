using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 0.6 — Dirección de arte: contraste, geometría y coherencia del tema.
/// Reglas de <c>docs/art_direction.md</c> §2.3 (WCAG 2.1): texto ≥ 4,5:1 y
/// bordes/estados ≥ 3:1. El test garantiza que la paleta no se rompe.
/// Equivalente C# de <c>tests/unit/PixelStylesTest.gd</c>.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class PixelStylesTest
{
    private const string ThemePath = "res://core/AccessibleTheme.tres";
    private const string BigButtonPath = "res://ui/components/BigButton.tscn";

    private static readonly string[] Families =
    {
        "PURPURA", "PURPURA_PRESIONADO", "AMARILLO", "TURQUESA", "CIAN",
        "CELDA", "PLACA", "ARENA", "CELDA_SELECCION",
    };

    private static Color Fill(string family) => family switch
    {
        "PURPURA" => PixelStyles.Purpura,
        "PURPURA_PRESIONADO" => PixelStyles.PurpuraPresionado,
        "AMARILLO" => PixelStyles.Amarillo,
        "TURQUESA" => PixelStyles.Turquesa,
        "CIAN" => PixelStyles.Cian,
        "CELDA" => PixelStyles.Celda,
        "PLACA" => PixelStyles.Placa,
        "ARENA" => PixelStyles.Arena,
        "CELDA_SELECCION" => PixelStyles.CeldaSeleccion,
        _ => PixelStyles.Arena,
    };

    private static float Ratio(Color a, Color b) => PixelStyles.ContrastRatio(a, b);

    [TestCase]
    public void TextoLegibleSobreCadaFamiliaDeColor()
    {
        foreach (string family in Families)
        {
            Color fill = Fill(family);
            Color text = PixelStyles.TextColor(fill);
            AssertBool(Ratio(text, fill) >= 4.5f)
                .OverrideFailureMessage($"{family}: texto sobre {fill.ToHtml()} = {Ratio(text, fill):F2} (mín. 4.5)")
                .IsTrue();
        }
    }

    [TestCase]
    public void BordeTintaVisibleSobreCadaFamiliaYSobreFondo()
    {
        foreach (string family in Families)
        {
            Color fill = Fill(family);
            AssertBool(Ratio(PixelStyles.Tinta, fill) >= 3.0f)
                .OverrideFailureMessage($"{family}: borde interior = {Ratio(PixelStyles.Tinta, fill):F2} (mín. 3)")
                .IsTrue();
        }

        AssertBool(Ratio(PixelStyles.Tinta, PixelStyles.Arena) >= 3.0f).IsTrue();
        AssertBool(Ratio(PixelStyles.Borde, PixelStyles.Arena) >= 3.0f).IsTrue();
    }

    [TestCase]
    public void BordesDelTableroCumplenTresContraRellenoYFondo()
    {
        Color[] borders = { PixelStyles.Borde, PixelStyles.EncontradaBorde, PixelStyles.PistaBorde };
        foreach (Color border in borders)
        {
            AssertBool(Ratio(border, PixelStyles.Celda) >= 3.0f)
                .OverrideFailureMessage($"borde {border.ToHtml()} vs crema = {Ratio(border, PixelStyles.Celda):F2}")
                .IsTrue();
            AssertBool(Ratio(border, PixelStyles.Arena) >= 3.0f)
                .OverrideFailureMessage($"borde {border.ToHtml()} vs arena = {Ratio(border, PixelStyles.Arena):F2}")
                .IsTrue();
        }
    }

    [TestCase]
    public void TrazoDeSeleccionVisibleSobreLasCeldas()
    {
        AssertBool(Ratio(PixelStyles.Trazo, PixelStyles.Celda) >= 3.0f).IsTrue();
        AssertBool(Ratio(PixelStyles.Tinta, PixelStyles.CeldaSeleccion) >= 4.5f).IsTrue();
    }

    [TestCase]
    public void TextoSobreFondosClarosCumpleCuatroComaCinco()
    {
        Color[] light = { PixelStyles.Celda, PixelStyles.Placa, PixelStyles.Arena };
        foreach (Color background in light)
        {
            AssertBool(Ratio(PixelStyles.Tinta, background) >= 4.5f)
                .OverrideFailureMessage($"tinta sobre {background.ToHtml()} = {Ratio(PixelStyles.Tinta, background):F2}")
                .IsTrue();
        }
    }

    [TestCase]
    public void EstilosDelTableroUsanRadioYBordeEsperados()
    {
        StyleBoxFlat normal = PixelStyles.CellNormal();
        AssertInt(normal.CornerRadiusTopLeft).IsEqual(PixelStyles.Radius);
        AssertInt(normal.BorderWidthLeft).IsEqual(2);
        AssertBool(normal.BgColor.IsEqualApprox(PixelStyles.Celda)).IsTrue();
        AssertInt(PixelStyles.CellFound().BorderWidthLeft).IsEqual(3);
        AssertInt(PixelStyles.CellHint().BorderWidthLeft).IsEqual(3);
    }

    [TestCase]
    public void FocoYPresionadoCambianFormaNoSoloColor()
    {
        StyleBoxFlat focus = PixelStyles.ButtonFocus();
        AssertInt(focus.BorderWidthLeft).IsGreaterEqual(3);
        AssertFloat(focus.ExpandMarginLeft).IsGreaterEqual(3.0f);
        AssertBool(focus.DrawCenter).IsFalse();
        AssertBool(Ratio(focus.BorderColor, PixelStyles.Arena) >= 3.0f).IsTrue();

        StyleBoxFlat pressed = PixelStyles.ButtonPressed();
        AssertInt(pressed.BorderWidthLeft).IsEqual(3);
        AssertInt(pressed.ShadowSize).IsEqual(0);
    }

    [TestCase]
    public void TemaGlobalCumpleContrasteYTipografia()
    {
        var theme = GD.Load<Theme>(ThemePath);
        AssertBool(theme != null).IsTrue();

        var normal = theme.GetStylebox("normal", "Button") as StyleBoxFlat;
        Color fontColor = theme.GetColor("font_color", "Button");
        AssertBool(Ratio(fontColor, normal.BgColor) >= 4.5f).IsTrue();

        var disabled = theme.GetStylebox("disabled", "Button") as StyleBoxFlat;
        Color disabledColor = theme.GetColor("font_disabled_color", "Button");
        AssertBool(Ratio(disabledColor, disabled.BgColor) >= 4.5f).IsTrue();

        Color labelColor = theme.GetColor("font_color", "Label");
        AssertBool(Ratio(labelColor, PixelStyles.Arena) >= 4.5f).IsTrue();
        AssertInt(theme.DefaultFontSize).IsGreaterEqual(20);
        AssertBool(theme.DefaultFont != null).IsTrue();
    }

    [TestCase]
    public void BigButtonCumpleElTamanoMinimoTactil()
    {
        var scene = GD.Load<PackedScene>(BigButtonPath);
        AssertBool(scene != null).IsTrue();
        var button = scene.Instantiate<Button>();
        Vector2 minimum = button.CustomMinimumSize;
        AssertBool(minimum.X >= Constants.MinTouchSize.X && minimum.Y >= Constants.MinTouchSize.Y)
            .OverrideFailureMessage($"BigButton: {minimum} (mín. {Constants.MinTouchSize})")
            .IsTrue();
        button.Free();
    }
}
