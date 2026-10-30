using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 §3.2 — Lógica de la selección por arrastre. Bloqueo de dirección,
/// ajuste a la línea dominante, límites del tablero, cancelación y ruta recta.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class DragSelectionTest
{
    [TestCase]
    public void ArrastreHorizontalConstruyeLaRutaCompleta()
    {
        var drag = new DragSelection();
        AssertBool(drag.Begin(new Vector2I(0, 0))).IsTrue();
        AssertBool(drag.Update(new Vector2I(3, 0), 8, 8)).IsTrue();
        AssertBool(drag.TryComplete(out Vector2I start, out Vector2I end)).IsTrue();

        AssertCoord(start, 0, 0);
        AssertCoord(end, 3, 0);
        AssertCoord(drag.Direction, 0, 0); // se limpia al completar
    }

    [TestCase]
    public void ArrastreDiagonalSeAjustaALaDiagonal()
    {
        var drag = new DragSelection();
        drag.Begin(new Vector2I(1, 1));
        drag.Update(new Vector2I(4, 4), 8, 8);

        AssertBool(drag.TryComplete(out _, out Vector2I end)).IsTrue();
        AssertCoord(end, 4, 4);
    }

    [TestCase]
    public void LaDesviacionSeAjustaALaLineaDominante()
    {
        var drag = new DragSelection();
        drag.Begin(new Vector2I(0, 0));
        drag.Update(new Vector2I(1, 3), 8, 8); // casi vertical
        AssertBool(drag.TryComplete(out _, out Vector2I verticalEnd)).IsTrue();
        AssertCoord(verticalEnd, 0, 3);

        var other = new DragSelection();
        other.Begin(new Vector2I(0, 0));
        other.Update(new Vector2I(3, 1), 8, 8); // casi horizontal
        AssertBool(other.TryComplete(out _, out Vector2I horizontalEnd)).IsTrue();
        AssertCoord(horizontalEnd, 3, 0);

        var diagonal = new DragSelection();
        diagonal.Begin(new Vector2I(0, 0));
        diagonal.Update(new Vector2I(3, 2), 8, 8); // zona diagonal
        AssertBool(diagonal.TryComplete(out _, out Vector2I diagonalEnd)).IsTrue();
        AssertCoord(diagonalEnd, 3, 3);
    }

    [TestCase]
    public void LaDireccionQuedaBloqueadaTrasElPrimerMovimiento()
    {
        var drag = new DragSelection();
        drag.Begin(new Vector2I(0, 0));
        drag.Update(new Vector2I(0, 2), 8, 8);
        drag.Update(new Vector2I(5, 2), 8, 8); // zigzag horizontal

        AssertBool(drag.TryComplete(out _, out Vector2I end)).IsTrue();
        AssertCoord(end, 0, 2);
    }

    [TestCase]
    public void LaRutaSeRecortaAlBordeDelTablero()
    {
        var drag = new DragSelection();
        drag.Begin(new Vector2I(6, 6));
        drag.Update(new Vector2I(9, 9), 8, 8);
        AssertBool(drag.TryComplete(out _, out Vector2I end)).IsTrue();
        AssertCoord(end, 7, 7);

        AssertInt(DragSelection.MaxSteps(new Vector2I(0, 0), new Vector2I(-1, 0), 8, 8)).IsEqual(0);
        AssertInt(DragSelection.MaxSteps(new Vector2I(6, 6), new Vector2I(1, 1), 8, 8)).IsEqual(1);
    }

    [TestCase]
    public void UnaSolaCeldaNoEsUnaSeleccionValida()
    {
        var drag = new DragSelection();
        drag.Begin(new Vector2I(2, 2));
        drag.Update(new Vector2I(2, 2), 8, 8);
        AssertBool(drag.TryComplete(out _, out _)).IsFalse();
    }

    [TestCase]
    public void CancelarDescartaLaSeleccion()
    {
        var drag = new DragSelection();
        drag.Begin(new Vector2I(0, 0));
        drag.Update(new Vector2I(0, 4), 8, 8);
        drag.Cancel();

        AssertBool(drag.IsActive).IsFalse();
        AssertBool(drag.TryComplete(out _, out _)).IsFalse();
    }

    [TestCase]
    public void SnapDirectionCubreLasOchoDirecciones()
    {
        AssertCoord(DragSelection.SnapDirection(new Vector2I(4, 0)), new Vector2I(1, 0));
        AssertCoord(DragSelection.SnapDirection(new Vector2I(-4, 0)), new Vector2I(-1, 0));
        AssertCoord(DragSelection.SnapDirection(new Vector2I(0, 4)), new Vector2I(0, 1));
        AssertCoord(DragSelection.SnapDirection(new Vector2I(0, -4)), new Vector2I(0, -1));
        AssertCoord(DragSelection.SnapDirection(new Vector2I(3, 3)), new Vector2I(1, 1));
        AssertCoord(DragSelection.SnapDirection(new Vector2I(-3, 3)), new Vector2I(-1, 1));
        AssertCoord(DragSelection.SnapDirection(new Vector2I(-3, -3)), new Vector2I(-1, -1));
        AssertCoord(DragSelection.SnapDirection(new Vector2I(3, -3)), new Vector2I(1, -1));
        AssertCoord(DragSelection.SnapDirection(Vector2I.Zero), Vector2I.Zero);
    }

    private static void AssertCoord(Vector2I actual, int x, int y)
    {
        AssertInt(actual.X).OverrideFailureMessage($"x de {actual}").IsEqual(x);
        AssertInt(actual.Y).OverrideFailureMessage($"y de {actual}").IsEqual(y);
    }

    private static void AssertCoord(Vector2I actual, Vector2I expected)
    {
        AssertCoord(actual, expected.X, expected.Y);
    }
}
