using System.Collections.Generic;
using System.Threading.Tasks;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 — Integración de la selección por arrastre (IT-WS-01 a IT-WS-03).
/// Carga la escena real, simula el ratón sobre el tablero y comprueba la señal
/// <c>word_selected</c>, el desvanecido sin penalizar y el solape visual.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchDragTest
{
    private ISceneRunner _runner;
    private WordSearchMain _scene;

    [BeforeTest]
    public void BeforeTest()
    {
        WordSearchSession.Reset();
        _runner = ISceneRunner.Load(WordSearchMain.ScenePath, true);
        _scene = (WordSearchMain)_runner.Scene();
    }

    [TestCase]
    public async Task ArrastreRectoEmiteSeleccionYEncuentraLaPalabra() // IT-WS-01
    {
        (DifficultyConfig config, GenerationResult result) = BuildBoard("facil", seed: 21);
        _scene.StartGame(config, result);
        await _runner.SimulateFrames(3);

        AssertBool(_scene.Board.CellSize > 0)
            .OverrideFailureMessage($"el tablero no se ha maquetado (celda {_scene.Board.CellSize} px)")
            .IsTrue();

        Vector2I? emittedStart = null;
        Vector2I? emittedEnd = null;
        _scene.Board.WordSelected += (start, end) =>
        {
            emittedStart = start;
            emittedEnd = end;
        };

        WordPlacement target = result.Placements[0];
        await Drag(target.Start, target.End);

        AssertBool(emittedStart.HasValue)
            .OverrideFailureMessage("no se emitió word_selected")
            .IsTrue();
        AssertCoord(emittedStart.Value, target.Start);
        AssertCoord(emittedEnd.Value, target.End);
        AssertInt(_scene.WordsFound).IsEqual(1);
        AssertThat(_scene.FoundWords).Contains(target.Word);
    }

    [TestCase]
    public async Task SeleccionInvalidaSeDesvaneceSinPenalizar() // IT-WS-02
    {
        (DifficultyConfig config, GenerationResult result) = BuildBoard("facil", seed: 33);
        _scene.StartGame(config, result);
        await _runner.SimulateFrames(3);

        // Cualquier selección de 2 celdas es inválida: el banco no tiene palabras
        // de 2 letras, así que no puede coincidir con una palabra pendiente.
        var start = new Vector2I(0, 0);
        var end = new Vector2I(0, 1);
        await Drag(start, end);

        AssertInt(_scene.WordsFound).IsEqual(0);
        AssertInt(_scene.InvalidSelections).IsEqual(1);
        AssertInt((int)_scene.Board.GetCellState(start)).IsEqual((int)WordCellState.Selected);

        await _runner.AwaitMillis(800);

        AssertInt((int)_scene.Board.GetCellState(start))
            .OverrideFailureMessage("la celda inicial no volvió a reposo")
            .IsEqual((int)WordCellState.Normal);
        AssertInt((int)_scene.Board.GetCellState(end))
            .OverrideFailureMessage("la celda final no volvió a reposo")
            .IsEqual((int)WordCellState.Normal);
        AssertFloat(_scene.Board.GetCell(start).Modulate.A).IsEqual(1.0f);
        AssertInt(_scene.WordsFound).IsEqual(0);
    }

    [TestCase]
    public async Task LaUltimaPalabraEncontradaPrevaleceEnElCruce() // IT-WS-03
    {
        (DifficultyConfig config, GenerationResult result, WordPlacement first, WordPlacement second, Vector2I cross)? crossing
            = FindCrossingBoard("facil");
        AssertBool(crossing.HasValue)
            .OverrideFailureMessage("no se encontró un tablero con dos palabras cruzadas")
            .IsTrue();

        (DifficultyConfig config, GenerationResult result, WordPlacement first, WordPlacement second, Vector2I cross) = crossing.Value;
        _scene.StartGame(config, result);
        await _runner.SimulateFrames(3);

        await Drag(first.Start, first.End);
        AssertInt(_scene.WordsFound).IsEqual(1);
        AssertString(_scene.Board.GetCell(cross).FoundBy).IsEqual(first.Word);

        await Drag(second.Start, second.End);
        AssertInt(_scene.WordsFound).IsEqual(2);
        AssertBool(_scene.Board.GetCell(cross).IsFound).IsTrue();
        AssertString(_scene.Board.GetCell(cross).FoundBy)
            .OverrideFailureMessage("en el cruce debe prevalecer la última palabra encontrada")
            .IsEqual(second.Word);
    }

    // --- Utilidades ----------------------------------------------------------

    private async Task Drag(Vector2I start, Vector2I end)
    {
        Vector2 from = BoardPoint(start);
        Vector2 to = BoardPoint(end);

        _runner.SetMousePos(from);
        _runner.SimulateMouseButtonPress(MouseButton.Left);
        await _runner.SimulateFrames(1);
        _runner.SimulateMouseMove(to);
        await _runner.SimulateFrames(1);
        _runner.SimulateMouseButtonRelease(MouseButton.Left);
        await _runner.SimulateFrames(2);
    }

    private Vector2 BoardPoint(Vector2I cell)
    {
        return _scene.Board.GetGlobalTransformWithCanvas() * _scene.Board.GetCellCenter(cell);
    }

    private static (DifficultyConfig, GenerationResult) BuildBoard(string id, long seed)
    {
        DifficultyConfig config = DifficultyCatalog.LoadById(id);
        WordSelectionResult selection = WordSelection.Pick(config, WordBank.LoadAll(), null, seed + 100);
        GenerationResult result = new WordSearchGenerator().Generate(config, selection.Words, seed);
        return (config, result);
    }

    private static (DifficultyConfig, GenerationResult, WordPlacement, WordPlacement, Vector2I)?
        FindCrossingBoard(string id)
    {
        DifficultyConfig config = DifficultyCatalog.LoadById(id);
        WordSelectionResult selection = WordSelection.Pick(config, WordBank.LoadAll(), null, seed: 77);
        var generator = new WordSearchGenerator();

        for (long seed = 1; seed <= 80; seed++)
        {
            GenerationResult result = generator.Generate(config, selection.Words, seed);
            for (int i = 0; i < result.Placements.Count; i++)
            {
                for (int j = 0; j < result.Placements.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    foreach (Vector2I coord in result.Placements[i].Cells)
                    {
                        if (Contains(result.Placements[j].Cells, coord))
                        {
                            return (config, result, result.Placements[i], result.Placements[j], coord);
                        }
                    }
                }
            }
        }

        return null;
    }

    private static bool Contains(IReadOnlyList<Vector2I> cells, Vector2I coord)
    {
        foreach (Vector2I cell in cells)
        {
            if (cell == coord)
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertCoord(Vector2I actual, Vector2I expected)
    {
        AssertInt(actual.X).OverrideFailureMessage($"x de {actual}, se esperaba {expected}").IsEqual(expected.X);
        AssertInt(actual.Y).OverrideFailureMessage($"y de {actual}, se esperaba {expected}").IsEqual(expected.Y);
    }
}
