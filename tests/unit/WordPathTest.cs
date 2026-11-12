using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WordPathTest
{
    [TestCase]
    public void IsUnitDirectionAceptaLasOchoDirecciones()
    {
        foreach (var dir in WordPath.AllDirections)
            AssertBool(WordPath.IsUnitDirection(dir))
                .OverrideFailureMessage($"debería ser unitaria: {dir}")
                .IsTrue();

        AssertBool(WordPath.IsUnitDirection(Vector2I.Zero)).IsFalse();
        AssertBool(WordPath.IsUnitDirection(new Vector2I(2, 0))).IsFalse();
        AssertBool(WordPath.IsUnitDirection(new Vector2I(1, 2))).IsFalse();
    }

    [TestCase]
    public void DirectionCalculaRectasYRechazaNoAlineadas()
    {
        AssertCoord(WordPath.Direction(new Vector2I(0, 0), new Vector2I(3, 0)), 1, 0);
        AssertCoord(WordPath.Direction(new Vector2I(3, 0), new Vector2I(0, 0)), -1, 0);
        AssertCoord(WordPath.Direction(new Vector2I(0, 0), new Vector2I(0, 4)), 0, 1);
        AssertCoord(WordPath.Direction(new Vector2I(0, 4), new Vector2I(0, 1)), 0, -1);
        AssertCoord(WordPath.Direction(new Vector2I(1, 1), new Vector2I(4, 4)), 1, 1);
        AssertCoord(WordPath.Direction(new Vector2I(4, 4), new Vector2I(1, 1)), -1, -1);
        AssertCoord(WordPath.Direction(new Vector2I(0, 3), new Vector2I(3, 0)), 1, -1);

        AssertCoord(WordPath.Direction(new Vector2I(2, 2), new Vector2I(2, 2)), 0, 0);
        AssertCoord(WordPath.Direction(new Vector2I(0, 0), new Vector2I(2, 1)), 0, 0);
    }

    [TestCase]
    public void StepsYBuildGeneranRutasCorrectas()
    {
        AssertInt(WordPath.Steps(new Vector2I(0, 0), new Vector2I(3, 0))).IsEqual(3);
        AssertInt(WordPath.Steps(new Vector2I(0, 0), new Vector2I(2, 1))).IsEqual(0);

        var path = WordPath.Build(new Vector2I(1, 1), new Vector2I(1, 4));
        AssertInt(path.Count).IsEqual(4);
        AssertCoord(path[0], 1, 1);
        AssertCoord(path[3], 1, 4);

        var diag = WordPath.Build(new Vector2I(0, 0), new Vector2I(-1, -1));
        AssertInt(diag.Count).IsEqual(2);
        AssertCoord(diag[0], 0, 0);
        AssertCoord(diag[1], -1, -1);

        var same = WordPath.Build(new Vector2I(2, 2), new Vector2I(2, 2));
        AssertInt(same.Count).IsEqual(0);

        var invalid = WordPath.Build(new Vector2I(0, 0), new Vector2I(2, 1));
        AssertInt(invalid.Count).IsEqual(0);
    }

    [TestCase]
    public void BuildPorInicioDireccionYLongitud()
    {
        var path = WordPath.Build(new Vector2I(2, 2), new Vector2I(1, -1), 3);
        AssertInt(path.Count).IsEqual(3);
        AssertCoord(path[0], 2, 2);
        AssertCoord(path[1], 3, 1);
        AssertCoord(path[2], 4, 0);

        var empty = WordPath.Build(new Vector2I(0, 0), new Vector2I(1, 0), 0);
        AssertInt(empty.Count).IsEqual(0);
    }

    [TestCase]
    public void ColeccionesDeDireccionesSonConsistentes()
    {
        AssertInt(WordPath.AllDirections.Length).IsEqual(8);
        AssertInt(WordPath.OrthogonalForward.Length).IsEqual(2);
        AssertBool(System.Array.IndexOf(WordPath.OrthogonalForward, new Vector2I(1, 0)) >= 0).IsTrue();
        AssertBool(System.Array.IndexOf(WordPath.OrthogonalForward, new Vector2I(0, 1)) >= 0).IsTrue();
    }

    private static void AssertCoord(Vector2I actual, int x, int y)
    {
        AssertInt(actual.X).OverrideFailureMessage($"x de {actual}").IsEqual(x);
        AssertInt(actual.Y).OverrideFailureMessage($"y de {actual}").IsEqual(y);
    }

    private static void AssertCoord(Vector2I actual, Vector2I expected) => AssertCoord(actual, expected.X, expected.Y);
}
