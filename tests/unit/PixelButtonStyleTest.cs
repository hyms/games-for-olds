using ClasicosMenteActiva.core;
using ClasicosMenteActiva.scenes.ui.components;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 0.6 — Contrato visual de los botones y celdas pixel art.
///     Verifica que todos los estados comparten esquinas rectas, borde de 2 px,
///     alfa sólido y contraste WCAG, y que el relieve se invierte y el contenido
///     baja 2 px al pulsar (docs/art_direction.md §3 y §5.3).
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class PixelButtonStyleTest
{
    private static readonly ThemeData Theme = new();

    private static float Ratio(Color a, Color b) => PixelStyles.ContrastRatio(a, b);

    [TestCase]
    public void TodosLosEstadosSonSolidosYCuadrados()
    {
        foreach (var variant in PixelButtonStyle.AllVariants)
        foreach (var state in PixelButtonStyle.AllStates)
        {
            var visual = PixelButtonStyle.For(variant, state, Theme);
            var style = visual.Style;
            var label = $"{variant}/{state}";

            AssertBool(style.AntiAliasing).OverrideFailureMessage($"{label}: anti-aliasing").IsFalse();
            AssertInt(style.CornerRadiusTopLeft).OverrideFailureMessage(label).IsEqual(PixelStyles.Radius);
            AssertInt(style.CornerRadiusBottomRight).OverrideFailureMessage(label).IsEqual(PixelStyles.Radius);
            AssertInt(style.BorderWidthLeft).OverrideFailureMessage(label)
                .IsEqual(PixelReliefPainter.BorderWidth);
            AssertInt(style.BorderWidthTop).OverrideFailureMessage(label)
                .IsEqual(PixelReliefPainter.BorderWidth);

            if (state == PixelButtonState.Focus)
            {
                // El foco es un anillo superpuesto: centro transparente y 2 px fuera.
                AssertBool(style.DrawCenter).IsFalse();
                AssertFloat(style.ExpandMarginLeft).IsEqual(2.0f);
            }
            else
            {
                AssertFloat(style.BgColor.A).OverrideFailureMessage($"{label}: alfa").IsEqual(1.0f);
            }
        }
    }

    [TestCase]
    public void ElRelieveSeInvierteYElContenidoBajaAlPulsar()
    {
        var normal = PixelButtonStyle.For(PixelButtonVariant.Action, PixelButtonState.Normal, Theme);
        var hover = PixelButtonStyle.For(PixelButtonVariant.Action, PixelButtonState.Hover, Theme);
        var pressed = PixelButtonStyle.For(PixelButtonVariant.Action, PixelButtonState.Pressed, Theme);
        var disabled = PixelButtonStyle.For(PixelButtonVariant.Action, PixelButtonState.Disabled, Theme);

        AssertBool(normal.Raised).IsTrue();
        AssertBool(normal.DrawRelief).IsTrue();
        AssertBool(hover.Raised).IsFalse();
        AssertBool(hover.DrawRelief).IsTrue();
        AssertBool(pressed.Raised).IsFalse();
        AssertBool(pressed.DrawRelief).IsTrue();
        AssertBool(disabled.DrawRelief).IsFalse();

        // Hover hunde el bisel sin mover el contenido; pulsado lo hunde y baja 2 px.
        AssertFloat(hover.Style.ContentMarginTop).IsEqual(normal.Style.ContentMarginTop);
        AssertFloat(pressed.Style.ContentMarginTop - normal.Style.ContentMarginTop)
            .IsEqual((float)PixelButtonStyle.PressShift);
        AssertFloat(normal.Style.ContentMarginBottom - pressed.Style.ContentMarginBottom)
            .IsEqual((float)PixelButtonStyle.PressShift);
        AssertFloat(pressed.Style.ContentMarginTop + pressed.Style.ContentMarginBottom)
            .IsEqual(normal.Style.ContentMarginTop + normal.Style.ContentMarginBottom);

        AssertInt(hover.Style.ShadowSize).IsEqual(0);
        AssertInt(pressed.Style.ShadowSize).IsEqual(0);
        AssertBool(normal.Style.ShadowSize > 0).IsTrue();
    }

    [TestCase]
    public void TextoEIconoDeCadaEstadoCumplenContraste()
    {
        foreach (var variant in PixelButtonStyle.AllVariants)
        foreach (var state in new[]
                 {
                     PixelButtonState.Normal, PixelButtonState.Hover,
                     PixelButtonState.Pressed, PixelButtonState.Disabled
                 })
        {
            var visual = PixelButtonStyle.For(variant, state, Theme);
            var background = visual.Style.BgColor;
            var ratio = Ratio(visual.TextColor, background);
            AssertBool(ratio >= 4.5f)
                .OverrideFailureMessage($"{variant}/{state}: {visual.TextColor.ToHtml()} sobre " +
                                        $"{background.ToHtml()} = {ratio:F2} (mín. 4.5)")
                .IsTrue();
            AssertBool(visual.IconColor.IsEqualApprox(visual.TextColor)).IsTrue();
        }
    }

    [TestCase]
    public void FocoUsaAnilloExteriorYNoCambiaElRelleno()
    {
        var focus = PixelButtonStyle.For(PixelButtonVariant.Menu, PixelButtonState.Focus, Theme);
        AssertBool(focus.Style.DrawCenter).IsFalse();
        AssertFloat(focus.Style.ExpandMarginLeft).IsEqual(2.0f);
        AssertBool(Ratio(focus.Style.BorderColor, PixelStyles.Arena) >= 3.0f).IsTrue();
        AssertBool(focus.Style.BgColor.A < 0.01f).IsTrue();
    }

    [TestCase]
    public void CeldasElevanReposoYPistaYHundenSeleccionYEncontrada()
    {
        var idle = PixelCellStyle.For(PixelCellState.Idle, Theme);
        var hint = PixelCellStyle.For(PixelCellState.Hint, Theme);
        var selected = PixelCellStyle.For(PixelCellState.Selected, Theme);
        var found = PixelCellStyle.For(PixelCellState.Found, Theme);

        AssertBool(idle.Raised).IsTrue();
        AssertBool(hint.Raised).IsTrue();
        AssertBool(selected.Raised).IsFalse();
        AssertBool(found.Raised).IsFalse();

        AssertBool(idle.Style.BgColor.IsEqualApprox(Theme.CellIdleBackgroundColor)).IsTrue();
        AssertBool(idle.Style.BorderColor.IsEqualApprox(Theme.CellIdleBorderColor)).IsTrue();
        // Selected usa el acento turquesa oficial (#099C92), no el azul anterior.
        AssertBool(selected.Style.BgColor.IsEqualApprox(PixelStyles.Turquesa))
            .OverrideFailureMessage($"selected fill = {selected.Style.BgColor.ToHtml()}")
            .IsTrue();
        AssertBool(found.Style.BgColor.IsEqualApprox(PixelStyles.Turquesa)).IsTrue();

        AssertBool(selected.Style.BorderColor.IsEqualApprox(PixelStyles.EncontradaBorde))
            .OverrideFailureMessage($"selected border = {selected.Style.BorderColor.ToHtml()}")
            .IsTrue();
        AssertBool(selected.TextColor.IsEqualApprox(Colors.White))
            .OverrideFailureMessage($"selected text = {selected.TextColor.ToHtml()}")
            .IsTrue();

        AssertInt(found.Style.CornerRadiusTopLeft).IsEqual(PixelStyles.Radius);

        foreach (var visual in new[] { idle, hint, selected, found })
        {
            AssertInt(visual.Style.BorderWidthLeft).IsEqual(PixelReliefPainter.BorderWidth);
            AssertBool(visual.DrawRelief).IsTrue();
            AssertFloat(visual.Style.BgColor.A).IsEqual(1.0f);
        }

        // Turquesa + blanco: gran texto → umbral 3:1 es suficiente.
        AssertBool(Ratio(selected.TextColor, selected.Style.BgColor) >= 3.0f).IsTrue();
        AssertBool(Ratio(found.TextColor, found.Style.BgColor) >= 3.0f).IsTrue();
        // Hint mantiene ≥4.5:1 por ser un estado temporal más sutil.
        AssertBool(Ratio(hint.TextColor, hint.Style.BgColor) >= 4.5f).IsTrue();
    }

    [TestCase]
    public void BigButtonHeredaDePixelButtonYUsaVarianteMenu()
    {
        AssertBool(typeof(PixelButton).IsAssignableFrom(typeof(BigButton))).IsTrue();
        var scene = GD.Load<PackedScene>("res://ui/components/BigButton.tscn");
        var button = scene.Instantiate<BigButton>();
        AssertInt((int)button.ButtonVariant).IsEqual((int)PixelButtonVariant.Menu);
        button.Free();
    }
}
