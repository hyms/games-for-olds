using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 — Estado de la partida (SRP). Validación de selecciones sin escenas:
/// acierto, palabra repetida, selección inválida, una sola celda, sentido
/// invertido y fin de partida.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchGameTest
{
    [TestCase]
    public void EncuentraUnaPalabraYPasaALaListaDeEncontradas()
    {
        WordSearchGame game = NewGame();
        SelectionResult selection = game.Select(new Vector2I(0, 0), new Vector2I(3, 0));

        AssertBool(selection.IsFound).IsTrue();
        AssertString(selection.Word).IsEqual("CASA");
        AssertInt(selection.Cells.Count).IsEqual(4);
        AssertInt(game.FoundCount).IsEqual(1);
        AssertInt(game.PendingWords.Count).IsEqual(1);
        AssertInt(game.InvalidSelections).IsEqual(0);
        AssertBool(game.IsComplete).IsFalse();
    }

    [TestCase]
    public void UnaSeleccionQueNoEsPalabraSumaUnFalloYNoCambiaElEstado()
    {
        WordSearchGame game = NewGame();
        SelectionResult selection = game.Select(new Vector2I(1, 0), new Vector2I(1, 1));

        AssertInt((int)selection.Outcome).IsEqual((int)SelectionOutcome.Invalid);
        AssertInt(game.FoundCount).IsEqual(0);
        AssertInt(game.InvalidSelections).IsEqual(1);
    }

    [TestCase]
    public void UnaSolaCeldaNoEsUnaSeleccionValida()
    {
        WordSearchGame game = NewGame();
        SelectionResult selection = game.Select(new Vector2I(0, 0), new Vector2I(0, 0));

        AssertInt((int)selection.Outcome).IsEqual((int)SelectionOutcome.Invalid);
        AssertInt(game.InvalidSelections).IsEqual(1);
    }

    [TestCase]
    public void RepetirUnaPalabraEncontradaNoSumaFallos()
    {
        WordSearchGame game = NewGame();
        game.Select(new Vector2I(0, 0), new Vector2I(3, 0));

        SelectionResult repeated = game.Select(new Vector2I(0, 0), new Vector2I(3, 0));
        AssertBool(repeated.IsRepeated).IsTrue();
        AssertInt(game.FoundCount).IsEqual(1);
        AssertInt(game.InvalidSelections).IsEqual(0);
    }

    [TestCase]
    public void LaPalabraSoloValeEnElSentidoEnQueSeColoco()
    {
        WordSearchGame game = NewGame();
        SelectionResult backwards = game.Select(new Vector2I(3, 0), new Vector2I(0, 0));

        AssertString(backwards.Word).IsEqual("ASAC");
        AssertInt((int)backwards.Outcome).IsEqual((int)SelectionOutcome.Invalid);
        AssertInt(game.FoundCount).IsEqual(0);
    }

    [TestCase]
    public void LaPartidaTerminaAlEncontrarTodasLasPalabras()
    {
        WordSearchGame game = NewGame();
        game.Select(new Vector2I(0, 0), new Vector2I(3, 0));
        game.Select(new Vector2I(0, 1), new Vector2I(0, 4));

        AssertBool(game.IsComplete).IsTrue();
        AssertInt(game.PendingWords.Count).IsEqual(0);
        AssertThat(game.FoundWords).ContainsExactly("CASA", "MESA");
        AssertThat(game.Order).ContainsExactly("CASA", "MESA");
    }

    /// <summary>Tablero 5×5 con CASA horizontal y MESA vertical en la primera columna.</summary>
    private static WordSearchGame NewGame()
    {
        char[,] grid = WordSearchGenerator.CreateEmptyGrid(5, 5);
        grid[0, 0] = 'C';
        grid[0, 1] = 'A';
        grid[0, 2] = 'S';
        grid[0, 3] = 'A';
        grid[1, 0] = 'M';
        grid[2, 0] = 'E';
        grid[3, 0] = 'S';
        grid[4, 0] = 'A';
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (grid[y, x] == '\0')
                {
                    grid[y, x] = 'X';
                }
            }
        }

        var placements = new List<WordPlacement>
        {
            new("CASA", new Vector2I(0, 0), new Vector2I(1, 0),
                WordPath.Build(new Vector2I(0, 0), new Vector2I(1, 0), 4)),
            new("MESA", new Vector2I(0, 1), new Vector2I(0, 1),
                WordPath.Build(new Vector2I(0, 1), new Vector2I(0, 1), 4)),
        };

        var result = new GenerationResult(grid, placements, new[] { "CASA", "MESA" }, 2, 1);
        return new WordSearchGame(result);
    }
}
