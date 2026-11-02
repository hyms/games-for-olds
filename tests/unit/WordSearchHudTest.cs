using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 — Formato del header de palabras (HUD sin escenas): flujo continuo
/// con auto-wrapping, pendientes en el orden del tablero y encontradas al
/// final, tachadas y en el color de «encontrada».
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchHudTest
{
    [TestCase]
    public void ListaLasPendientesYDejaLasEncontradasAlFinalTachadas()
    {
        var pending = new List<string> { "MESA", "LUNA" };
        string text = WordSearchHud.BuildWordList(pending, new[] { "CASA" });

        // Flujo continuo: una sola línea que el RichTextLabel ajusta solo.
        AssertBool(text.Contains('\n'))
            .OverrideFailureMessage($"el header no debe usar saltos de línea: «{text}»")
            .IsFalse();

        int mesa = text.IndexOf("MESA");
        int luna = text.IndexOf("LUNA");
        int casa = text.IndexOf("[s]CASA[/s]");
        AssertInt(mesa).IsEqual(0);
        AssertBool(luna > mesa)
            .OverrideFailureMessage($"«LUNA» debe seguir a «MESA» en «{text}»")
            .IsTrue();
        AssertBool(casa > luna)
            .OverrideFailureMessage($"la encontrada debe ir al final del flujo en «{text}»")
            .IsTrue();
        AssertBool(text.Contains(PixelStyles.EncontradaBorde.ToHtml(false)))
            .OverrideFailureMessage($"falta el color de encontrada en «{text}»")
            .IsTrue();
    }

    [TestCase]
    public void SinPalabrasEncontradasDevuelveSoloElOrdenDelTablero()
    {
        var pending = new List<string> { "CASA", "MESA" };
        string text = WordSearchHud.BuildWordList(pending, new List<string>());

        AssertString(text).IsEqual("CASA   MESA");
        AssertBool(text.Contains('\n')).IsFalse();
    }

    [TestCase]
    public void SinPalabrasDevuelveTextoVacio()
    {
        string text = WordSearchHud.BuildWordList(new HashSet<string>(), new List<string>());

        AssertString(text).IsEmpty();
    }
}
