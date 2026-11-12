using System.Collections.Generic;
using ClasicosMenteActiva.core;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WordSearchHudExtraTest
{
    [TestCase]
    public void BindYRenderManejanNulosYValores()
    {
        var counter = new Label();
        var difficulty = new Label();
        var victory = new Label { Visible = false };
        var hud = new WordSearchHud(counter, difficulty, victory);

        hud.Bind(null);
        AssertString(difficulty.Text).IsEmpty();

        var config = DifficultyCatalog.LoadById("medio");
        hud.Bind(config);
        AssertBool(difficulty.Text.Contains("MEDIO")).IsTrue();
        AssertBool(difficulty.Text.Contains("NIVEL 2")).IsTrue();

        hud.Render(3, 8, false);
        AssertString(counter.Text).IsEqual("3/8 Palabras");
        AssertBool(victory.Visible).IsFalse();
        hud.Render(8, 8, true);
        AssertString(counter.Text).IsEqual("8/8 Palabras");
        AssertBool(victory.Visible).IsTrue();

        // tolera labels nulos
        var nullHud = new WordSearchHud(null, null, null);
        nullHud.Bind(config);
        nullHud.Render(1, 2, false);

        counter.Free();
        difficulty.Free();
        victory.Free();
    }

    [TestCase]
    public void BuildWordListConTemaUsaColorDeTachado()
    {
        var theme = new ThemeData();
        var pending = new List<string> { "PERRO" };
        var found = new List<string> { "GATO" };
        var text = WordSearchHud.BuildWordList(pending, found, theme);
        AssertBool(text.Contains("PERRO")).IsTrue();
        AssertBool(text.Contains("[s]GATO[/s]")).IsTrue();
        AssertBool(text.Contains(theme.FooterStrikethroughColor.ToHtml(false))).IsTrue();

        var text2 = WordSearchHud.BuildWordList(new HashSet<string>(), found, theme);
        AssertBool(text2.StartsWith("[color=#")).IsTrue();
    }
}
