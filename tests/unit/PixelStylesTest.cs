using ClasicosMenteActiva.core;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 0.6 — Dirección de arte: contraste, geometría y coherencia del tema.
///     Reglas de <c>docs/art_direction.md</c> §2.3 (WCAG 2.1): texto ≥ 4,5:1 y
///     bordes/estados ≥ 3:1. El test garantiza que la paleta no se rompe.
///     Equivalente C# de <c>tests/unit/PixelStylesTest.gd</c>.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class PixelStylesTest
{
    private const string ThemePath = "res://core/AccessibleTheme.tres";
    private const string BigButtonPath = "res://ui/components/BigButton.tscn";

    private static readonly string[] Families =
    {
        "TURQUESA", "CIAN",
        "CELDA", "PLACA", "ARENA", "CELDA_SELECCION"
    };

    private static Color Fill(string family)
    {
        return family switch
        {
            "TURQUESA" => PixelStyles.Turquesa,
            "CIAN" => PixelStyles.Cian,
            "CELDA" => PixelStyles.Celda,
            "PLACA" => PixelStyles.Placa,
            "ARENA" => PixelStyles.Arena,
            "CELDA_SELECCION" => PixelStyles.CeldaSeleccion,
            _ => PixelStyles.Arena
        };
    }

    private static float Ratio(Color a, Color b)
    {
        return PixelStyles.ContrastRatio(a, b);
    }

    [TestCase]
    public void TextoLegibleSobreCadaFamiliaDeColor()
    {
        foreach (var family in Families)
        {
            var fill = Fill(family);
            var text = PixelStyles.TextColor(fill);
            AssertBool(Ratio(text, fill) >= 4.5f)
                .OverrideFailureMessage($"{family}: texto sobre {fill.ToHtml()} = {Ratio(text, fill):F2} (mín. 4.5)")
                .IsTrue();
        }
    }

    [TestCase]
    public void BordeLegibleSobreCadaFamiliaYSobreFondo()
    {
        foreach (var family in Families)
        {
            var fill = Fill(family);
            var border = PixelStyles.BorderColor(fill);
            AssertBool(Ratio(border, fill) >= 3.0f)
                .OverrideFailureMessage(
                    $"{family}: borde {border.ToHtml()} sobre {fill.ToHtml()} = {Ratio(border, fill):F2} (mín. 3)")
                .IsTrue();
        }

        AssertBool(Ratio(PixelStyles.Borde, PixelStyles.Arena) >= 3.0f).IsTrue();
    }

    [TestCase]
    public void BordesDelTableroCumplenTresContraRellenoYFondo()
    {
        Color[] borders = { PixelStyles.Borde, PixelStyles.EncontradaBorde, PixelStyles.PistaBorde };
        foreach (var border in borders)
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
        // El trazo se dibuja sobre el fondo blanco del tablero y sobre las
        // celdas en reposo (crema): debe alcanzar 3:1 en ambos.
        AssertBool(Ratio(PixelStyles.Trazo, PixelStyles.Celda) >= 3.0f).IsTrue();
        AssertBool(Ratio(PixelStyles.Trazo, PixelStyles.Fondo) >= 3.0f)
            .OverrideFailureMessage(
                $"trazo {PixelStyles.Trazo.ToHtml()} sobre fondo = {Ratio(PixelStyles.Trazo, PixelStyles.Fondo):F2}")
            .IsTrue();

        // La celda seleccionada usa el acento turquesa con letras blancas
        // (texto grande: umbral 3:1).
        AssertBool(Ratio(PixelStyles.TextLight, PixelStyles.Turquesa) >= 3.0f)
            .OverrideFailureMessage(
                $"letras blancas sobre turquesa = {Ratio(PixelStyles.TextLight, PixelStyles.Turquesa):F2}")
            .IsTrue();
    }

    [TestCase]
    public void TextoSobreFondosClarosCumpleCuatroComaCinco()
    {
        Color[] light = { PixelStyles.Celda, PixelStyles.Placa, PixelStyles.Arena };
        foreach (var background in light)
        {
            var text = PixelStyles.TextColor(background);
            AssertBool(Ratio(text, background) >= 4.5f)
                .OverrideFailureMessage(
                    $"texto {text.ToHtml()} sobre {background.ToHtml()} = {Ratio(text, background):F2}")
                .IsTrue();
        }

        // Sobre la arena la tinta secundaria no llega a 4,5:1; el token oscuro sí.
        AssertBool(PixelStyles.TextColor(PixelStyles.Arena).IsEqualApprox(PixelStyles.TextDark)).IsTrue();
    }

    [TestCase]
    public void TemaGlobalCumpleContrasteYTipografia()
    {
        var theme = GD.Load<Theme>(ThemePath);
        AssertBool(theme != null).IsTrue();

        var normal = theme.GetStylebox("normal", "Button") as StyleBoxFlat;
        var fontColor = theme.GetColor("font_color", "Button");
        AssertBool(Ratio(fontColor, normal.BgColor) >= 4.5f).IsTrue();
        AssertInt(normal.CornerRadiusTopLeft).IsEqual(PixelStyles.Radius);
        AssertInt(normal.BorderWidthLeft).IsEqual(PixelReliefPainter.BorderWidth);

        var disabled = theme.GetStylebox("disabled", "Button") as StyleBoxFlat;
        var disabledColor = theme.GetColor("font_disabled_color", "Button");
        AssertBool(Ratio(disabledColor, disabled.BgColor) >= 4.5f).IsTrue();

        var labelColor = theme.GetColor("font_color", "Label");
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
        var minimum = button.CustomMinimumSize;
        AssertBool(minimum.X >= Constants.MinTouchSize.X && minimum.Y >= Constants.MinTouchSize.Y)
            .OverrideFailureMessage($"BigButton: {minimum} (mín. {Constants.MinTouchSize})")
            .IsTrue();
        button.Free();
    }

    /// <summary>WS-THEME-05: el token CelebrationRed (#B45252) vive en PixelStyles y es aditivo.</summary>
    [TestCase]
    public void CelebrationRedEsUnTokenAditivoDePixelArt()
    {
        AssertString(PixelStyles.CelebrationRed.ToHtml(false)).IsEqual("#b45252");

        // Aditivo: no redefine ningún token canónico de la matriz global.
        AssertBool(PixelStyles.Cian.ToHtml(false) != PixelStyles.CelebrationRed.ToHtml(false)).IsTrue();
        AssertBool(PixelStyles.Amarillo.ToHtml(false) != PixelStyles.CelebrationRed.ToHtml(false)).IsTrue();
        AssertBool(PixelStyles.Turquesa.ToHtml(false) != PixelStyles.CelebrationRed.ToHtml(false)).IsTrue();
    }

    /// <summary>WS-THEME-06: la rampa de confeti usa exclusivamente tokens de la paleta global.</summary>
    [TestCase]
    public void RampadeConfetiUsaSoloTokensDeLaPaletaGlobal()
    {
        // El emisor construye la rampa con los 4 tokens aprobados (spec §4.3).
        // Aquí se verifica que los colores fuente existen y son distintos.
        var colors = new[]
        {
            PixelStyles.Turquesa,
            PixelStyles.Amarillo,
            PixelStyles.Cian,
            PixelStyles.CelebrationRed
        };

        var set = new System.Collections.Generic.HashSet<string>();
        foreach (var color in colors)
            set.Add(color.ToHtml(false));

        AssertInt(set.Count).IsEqual(4);
    }
}