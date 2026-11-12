using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WordSearchGeneratorExtraTest
{
    [TestCase]
    public void GenerationResultExponeMetadatosYLecturas()
    {
        var config = new DifficultyConfig
        {
            Id = "t", DisplayName = "T", GridSize = 8, TargetWords = 5, MinimumWords = 4,
            CategoryCount = 1, ParSeconds = 60
        };
        foreach (var d in WordPath.OrthogonalForward) config.Directions.Add(d);
        var gen = new WordSearchGenerator();
        var result = gen.Generate(config, new[] { "CASA", "MESA", "PERRO", "GATO", "LUNA" }, 7);

        AssertInt(result.Rows).IsEqual(8);
        AssertInt(result.Columns).IsEqual(8);
        AssertInt(result.PlacedCount).IsGreaterEqual(1);
        AssertBool(result.MeetsMinimum).IsTrue();
        AssertInt(result.Attempts >= 1 ? 1 : 0).IsEqual(1);
        AssertInt(gen.LastAttempts >= 1 ? 1 : 0).IsEqual(1);
        AssertString(result.ReadPath(new Vector2I(0, 0), new Vector2I(0, 0))).IsEmpty();
        AssertString(result.ReadPath(new Vector2I(-1, 0), new Vector2I(0, 0))).IsEmpty();
        AssertString(result.GetLetter(new Vector2I(-1, 0)).ToString()).IsEqual(" ");
        AssertInt(result.UnplacedWords.Count).IsEqual(result.RequestedWords.Count - result.PlacedCount);

        // Reproducibilidad con semilla
        var r2 = gen.Generate(config, new[] { "CASA", "MESA", "PERRO", "GATO", "LUNA" }, 7);
        AssertInt(r2.PlacedCount).IsEqual(result.PlacedCount);
        for (var y = 0; y < result.Rows; y++)
        for (var x = 0; x < result.Columns; x++)
            AssertString(r2.GetLetter(new Vector2I(x, y)).ToString()).IsEqual(result.GetLetter(new Vector2I(x, y)).ToString());
    }

    [TestCase]
    public void ManejaEntradasVaciasNulasYNoColocables()
    {
        var config = new DifficultyConfig
        {
            Id = "t", DisplayName = "T", GridSize = 5, TargetWords = 5, MinimumWords = 4,
            CategoryCount = 1, ParSeconds = 60
        };
        foreach (var d in WordPath.AllDirections) config.Directions.Add(d);
        var gen = new WordSearchGenerator();

        var empty = gen.Generate(config, new List<string>(), 1);
        AssertBool(empty.MeetsMinimum).IsFalse();
        AssertInt(empty.PlacedCount).IsEqual(0);
        AssertInt(empty.Rows).IsEqual(5);

        var nullish = gen.Generate(config, null, 1);
        AssertInt(nullish.Rows).IsEqual(5);

        var unplaceable = gen.Generate(config, new[] { "A", "BB", "CCC", "1234", "CASA" }, 2);
        // solo CASA válida para grid 5
        AssertBool(unplaceable.PlacedCount <= 1).IsTrue();

        var prepared = WordSearchGenerator.PrepareWords(null, 8);
        AssertInt(prepared.Count).IsEqual(0);
    }

    [TestCase]
    public void SinDireccionesValidasUsaAvancePorDefecto()
    {
        var config = new DifficultyConfig
        {
            Id = "t", DisplayName = "T", GridSize = 8, TargetWords = 3, MinimumWords = 3,
            CategoryCount = 1, ParSeconds = 60
        };
        // sin direcciones
        var gen = new WordSearchGenerator();
        var result = gen.Generate(config, new[] { "CASA", "MESA", "GATO" }, 9);
        AssertBool(result.PlacedCount >= 1).IsTrue();
        foreach (var p in result.Placements)
        {
            var ok = p.Direction == new Vector2I(1, 0) || p.Direction == new Vector2I(0, 1);
            AssertBool(ok).OverrideFailureMessage($"dir inesperada {p.Direction}").IsTrue();
        }
    }

    [TestCase]
    public void TryPlaceWordValidaEntradas()
    {
        var grid = WordSearchGenerator.CreateEmptyGrid(4, 4);
        AssertBool(WordSearchGenerator.TryPlaceWord(null, Vector2I.Zero, new Vector2I(1, 0), grid)).IsFalse();
        AssertBool(WordSearchGenerator.TryPlaceWord("CASA", Vector2I.Zero, new Vector2I(2, 0), grid)).IsFalse();
        AssertBool(WordSearchGenerator.TryPlaceWord("CASA", Vector2I.Zero, new Vector2I(1, 0), null)).IsFalse();
        AssertBool(WordSearchGenerator.TryPlaceWord("SOL", Vector2I.Zero, new Vector2I(1, 0), grid)).IsFalse();
        AssertBool(WordSearchGenerator.TryPlaceWord("CASA", Vector2I.Zero, new Vector2I(1, 0), grid)).IsTrue();
    }
}
