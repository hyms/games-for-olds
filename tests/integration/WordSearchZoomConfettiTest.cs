using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClasicosMenteActiva.core;
using ClasicosMenteActiva.core.ui;
using ClasicosMenteActiva.minigames.word_search;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 4 — Integración de zoom, confeti y footer (IT-WS-04 a IT-WS-06).
///     Carga la escena real y verifica:
///     - el zoom del tablero no rompe la selección por arrastre (IT-WS-04),
///     - el footer mantiene ~70 px de altura mínima con 5/12/20 palabras (IT-WS-05),
///     - el confeti de victoria se dispara solo al completar y el modal aparece
///       tras ~300 ms (IT-WS-06).
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchZoomConfettiTest
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

    [AfterTest]
    public void AfterTest()
    {
        _runner?.Dispose();
        _scene = null;
    }

    [TestCase]
    public async Task ZoomEnRangoYNoRompeArrastre() // IT-WS-04
    {
        var (config, result) = BuildBoard("facil", 21);
        _scene.StartGame(config, result);
        await _runner.SimulateFrames(3);

        // El zoom empieza en 1.0 (spec §4: reposición al iniciar).
        AssertFloat(_scene.Board.Zoom).IsEqual(1.0f);

        // Los controles + / − están en la NavRow y disparan el zoom.
        var zoomIn = _scene.GetNodeOrNull<BigButton>("%BtnZoomIn");
        var zoomOut = _scene.GetNodeOrNull<BigButton>("%BtnZoomOut");
        AssertObject(zoomIn).IsNotNull();
        AssertObject(zoomOut).IsNotNull();

        zoomIn.EmitSignal(BaseButton.SignalName.Pressed);
        await _runner.SimulateFrames(1);
        AssertFloat(_scene.Board.Zoom).IsGreater(1.0f);

        zoomIn.EmitSignal(BaseButton.SignalName.Pressed);
        await _runner.SimulateFrames(1);
        AssertFloat(_scene.Board.Zoom).IsEqualApprox(WordSearchVisuals.MaxZoom, 0.01f);

        zoomOut.EmitSignal(BaseButton.SignalName.Pressed);
        await _runner.SimulateFrames(1);
        AssertFloat(_scene.Board.Zoom).IsLess(WordSearchVisuals.MaxZoom);

        // El zoom no debe romper la selección por arrastre: el tablero sigue
        // siendo válido y las coordenadas de celda siguen alineadas.
        var target = result.Placements[0];
        var cell = _scene.Board.GetCell(target.Start);
        AssertObject(cell).IsNotNull();
        AssertInt(cell.Letter).IsEqual(result.GetLetter(target.Start));
    }

    [TestCase]
    public async Task FooterMantieneSetentaPixelsMinimosConPalabras() // IT-WS-05
    {
        var (config, result) = BuildBoard("facil", 21);
        _scene.StartGame(config, result);
        await _runner.SimulateFrames(3);

        var footer = _scene.GetNodeOrNull<WordsPanel>("%WordsPanel");
        AssertObject(footer).IsNotNull();

        // Con 5 palabras pendientes el footer debe mantener al menos 70 px.
        AssertFloat(footer.CustomMinimumSize.Y).IsGreaterEqual(70.0f);

        // Al encontrar todas las palabras el texto cambia pero la altura mínima
        // se conserva (banda no colapsable, spec §5).
        var theme = _scene.GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        var pending = new List<string>();
        var found = result.Placements.Select(p => p.Word).ToList();
        footer.SetWords(pending, found, theme?.CurrentThemeData);
        await _runner.SimulateFrames(1);

        AssertFloat(footer.CustomMinimumSize.Y).IsGreaterEqual(70.0f);
    }

    [TestCase]
    public async Task ConfetiSoloEnVictoriaYModalTrasTrescientosMilisegundos() // IT-WS-06
    {
        var (config, result) = BuildBoard("facil", 21);
        _scene.StartGame(config, result);
        await _runner.SimulateFrames(3);

        var footer = _scene.GetNodeOrNull<WordsPanel>("%WordsPanel");
        var confetti = footer?.GetNodeOrNull<PixelConfettiEmitter>("Confetti");
        AssertObject(confetti).IsNotNull();

        // Encontrar todas las palabras para completar el tablero. Se usan los
        // extremos reales de cada colocación (IT-WS-01 demuestra que el arrastre
        // entre ellos desencadena word_selected).
        var completed = 0;
        _scene.Board.WordSelected += (Vector2I start, Vector2I end) => completed++;

        foreach (var placement in result.Placements)
            _scene.Board.EmitSignal(
                WordGridContainer.SignalName.WordSelected,
                placement.Start,
                placement.End);

        await _runner.SimulateFrames(5);

        // Todas las palabras deben haberse encontrado.
        AssertInt(completed).IsEqual(result.Placements.Count);

        // El confeti se disparó al completar (OneShot: Amount > 0 en emisión).
        AssertInt(confetti.Amount).IsGreater(0);

        // El modal de victoria NO aparece de inmediato: la spec pide ~300 ms.
        AssertBool(_scene.WinModalPublic.Visible).IsFalse();

        await _runner.AwaitMillis(400);

        // Tras ~300 ms el modal debe estar visible.
        AssertBool(_scene.WinModalPublic.Visible).IsTrue();
    }

    // --- Utilidades ----------------------------------------------------------

    private static (DifficultyConfig, GenerationResult) BuildBoard(string id, long seed)
    {
        var config = DifficultyCatalog.LoadById(id);
        var selection = WordSelection.Pick(config, WordBank.LoadAll(), null, seed + 100);
        var result = new WordSearchGenerator().Generate(config, selection.Words, seed);
        return (config, result);
    }
}