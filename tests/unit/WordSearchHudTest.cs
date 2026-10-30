using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 — Formato del footer de palabras (HUD sin escenas): pendientes en el
/// orden del tablero y encontradas al final, tachadas y en el color de
/// «encontrada».
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchHudTest
{
    [TestCase]
    public void ListaLasPendientesYDejaLasEncontradasAlFinalTachadas()
    {
        List<string> order = new() { "CASA", "MESA", "LUNA" };
        var pending = new HashSet<string> { "MESA", "LUNA" };
        string[] lines = WordSearchHud.BuildWordList(order, pending, new[] { "CASA" }).Split('\n');

        AssertInt(lines.Length).IsEqual(3);
        AssertString(lines[0]).IsEqual("MESA");
        AssertString(lines[1]).IsEqual("LUNA");
        AssertString(lines[2]).Contains("[s]CASA[/s]");
        AssertBool(lines[2].Contains(PixelStyles.EncontradaBorde.ToHtml(false)))
            .OverrideFailureMessage($"falta el color de encontrada en «{lines[2]}»")
            .IsTrue();
    }

    [TestCase]
    public void SinPalabrasEncontradasDevuelveSoloElOrdenDelTablero()
    {
        List<string> order = new() { "CASA", "MESA" };
        var pending = new HashSet<string> { "CASA", "MESA" };
        string text = WordSearchHud.BuildWordList(order, pending, new List<string>());

        AssertString(text).IsEqual("CASA\nMESA");
    }

    [TestCase]
    public void SinPalabrasDevuelveTextoVacio()
    {
        string text = WordSearchHud.BuildWordList(
            new List<string>(),
            new HashSet<string>(),
            new List<string>());

        AssertString(text).IsEmpty();
    }
}
