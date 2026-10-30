using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 2 §2.2 — Generador de tableros (UT-WS-01 a UT-WS-05).
/// Dimensiones por dificultad, direcciones, normalización, tolerancia del
/// 80 %, solape con letra coincidente, celdas rellenas y rendimiento en 15×15.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchGeneratorTest
{
    private static readonly string[] ShortWords =
    {
        "PERRO", "GATO", "MESA", "SILLA", "LIMA", "VACA", "PATO", "CASA",
        "SOL", "LUNA", "RIO", "MAR", "FLOR", "TREN", "BARCO", "AVION",
        "LIBRO", "CARTA", "PUERTA", "VENTANA",
    };

    private readonly WordSearchGenerator _generator = new();

    [TestCase]
    public void DimensionesPorDificultad() // UT-WS-01
    {
        foreach (DifficultyConfig config in DifficultyCatalog.LoadAll())
        {
            GenerationResult result = _generator.Generate(config, ShortWords, seed: 11);
            AssertInt(result.Rows)
                .OverrideFailureMessage($"{config.Id}: filas")
                .IsEqual(config.GridSize);
            AssertInt(result.Columns)
                .OverrideFailureMessage($"{config.Id}: columnas")
                .IsEqual(config.GridSize);
        }
    }

    [TestCase]
    public void FacilSoloUsaHorizontalYVerticalDirectas() // UT-WS-02
    {
        DifficultyConfig easy = DifficultyCatalog.LoadById("facil");
        GenerationResult result = _generator.Generate(easy, ShortWords, seed: 7);

        AssertInt(result.PlacedCount).IsGreaterEqual(easy.MinimumWords);
        foreach (WordPlacement placement in result.Placements)
        {
            bool allowed = placement.Direction == new Vector2I(1, 0)
                || placement.Direction == new Vector2I(0, 1);
            AssertBool(allowed)
                .OverrideFailureMessage($"{placement.Word} usa la dirección {placement.Direction}")
                .IsTrue();
            AssertString(ReadPlacement(result, placement)).IsEqual(placement.Word);
        }
    }

    [TestCase]
    public void DificilesYMaestroAdmitenReversosYDiagonales() // UT-WS-03
    {
        foreach (string id in new[] { "medio", "dificil", "maestro" })
        {
            DifficultyConfig config = DifficultyCatalog.LoadById(id);
            AssertBool(config.AllowsDirection(new Vector2I(1, 1)))
                .OverrideFailureMessage($"{id} debería admitir diagonal")
                .IsTrue();

            GenerationResult result = _generator.Generate(config, ShortWords, seed: 5);
            foreach (WordPlacement placement in result.Placements)
            {
                AssertBool(config.AllowsDirection(placement.Direction))
                    .OverrideFailureMessage($"{id}: {placement.Word} usa {placement.Direction}, no permitida")
                    .IsTrue();
            }
        }

        DifficultyConfig hard = DifficultyCatalog.LoadById("dificil");
        AssertBool(hard.AllowsDirection(new Vector2I(-1, 0))).IsTrue();
        AssertBool(hard.AllowsDirection(new Vector2I(0, -1))).IsTrue();
        AssertBool(hard.AllowsDirection(new Vector2I(-1, -1))).IsTrue();

        GenerationResult master = _generator.Generate(DifficultyCatalog.LoadById("maestro"), ShortWords, seed: 5);
        bool usesReverse = false;
        foreach (WordPlacement placement in master.Placements)
        {
            if (placement.Direction.X < 0 || placement.Direction.Y < 0)
            {
                usesReverse = true;
                break;
            }
        }

        AssertBool(usesReverse)
            .OverrideFailureMessage("Maestro no colocó ninguna palabra inversa en la semilla 5")
            .IsTrue();
    }

    [TestCase]
    public void PalabrasSeNormalizanYFiltran() // UT-WS-04
    {
        List<string> prepared = WordSearchGenerator.PrepareWords(
            new[] { "ESPAÑA", "ZANAHORIA", "ÁRBOL", "niño", "CASA", "CASA", "A", "EXTRAORDINARIAMENTE" },
            15);

        AssertThat(prepared).ContainsExactly("ESPANA", "ZANAHORIA", "ARBOL", "NINO", "CASA");
        AssertString(WordSearchTheme.NormalizeWord("ESPAÑA")).IsEqual("ESPANA");
        AssertString(WordSearchTheme.NormalizeWord("ZANAHORIA")).IsEqual("ZANAHORIA");
    }

    [TestCase]
    public void ToleraElOchentaPorCientoTrasReintentos() // UT-WS-05
    {
        // El Maestro pide 20 y acepta 16. Con un banco de solo 18 palabras no
        // puede llegar al 100 %: debe devolver 18 (>= 16) y MeetsMinimum true.
        var config = new DifficultyConfig
        {
            Id = "prueba-18",
            DisplayName = "Prueba",
            GridSize = 15,
            TargetWords = 20,
            MinimumWords = 16,
            CategoryCount = 1,
            ParSeconds = 720,
        };
        foreach (Vector2I direction in WordPath.AllDirections)
        {
            config.Directions.Add(direction);
        }

        var eighteen = new List<string>();
        for (int i = 0; i < 18; i++)
        {
            // Sin dígitos: NormalizeWord los descarta y las 18 palabras colapsarían.
            eighteen.Add($"PALABRA{(char)('A' + i)}");
        }

        GenerationResult result = _generator.Generate(config, eighteen, seed: 13);
        AssertInt(result.RequestedWords.Count).IsEqual(18);
        AssertInt(result.MinimumRequired).IsEqual(16);
        AssertInt(result.PlacedCount).IsGreaterEqual(16);
        AssertBool(result.MeetsMinimum).IsTrue();
    }

    [TestCase]
    public void FalloControladoSinTableroVacio()
    {
        var tiny = new DifficultyConfig
        {
            Id = "prueba-corta",
            DisplayName = "Prueba",
            GridSize = 5,
            TargetWords = 20,
            MinimumWords = 16,
            CategoryCount = 1,
            ParSeconds = 60,
        };
        foreach (Vector2I direction in WordPath.AllDirections)
        {
            tiny.Directions.Add(direction);
        }

        GenerationResult result = _generator.Generate(tiny, ShortWords, seed: 3);
        AssertBool(result.MeetsMinimum).IsFalse();
        AssertInt(result.Rows).IsEqual(5);

        bool emptyCell = false;
        for (int y = 0; y < result.Rows; y++)
        {
            for (int x = 0; x < result.Columns; x++)
            {
                char letter = result.Grid[y, x];
                if (letter < 'A' || letter > 'Z')
                {
                    emptyCell = true;
                }
            }
        }

        AssertBool(emptyCell).OverrideFailureMessage("quedaron celdas sin rellenar").IsFalse();
    }

    [TestCase]
    public void SolapeSoloConLaLetraCoincidente()
    {
        char[,] grid = WordSearchGenerator.CreateEmptyGrid(10, 10);
        AssertBool(WordSearchGenerator.TryPlaceWord("CASA", new Vector2I(0, 0), new Vector2I(1, 0), grid)).IsTrue();
        AssertBool(WordSearchGenerator.TryPlaceWord("SALA", new Vector2I(0, 2), new Vector2I(1, 0), grid)).IsTrue();
        AssertString(grid[0, 2].ToString()).IsEqual("S");
        AssertBool(WordSearchGenerator.TryPlaceWord("CAMA", new Vector2I(0, 0), new Vector2I(1, 0), grid)).IsFalse();
        AssertBool(WordSearchGenerator.TryPlaceWord("LARGO", new Vector2I(7, 5), new Vector2I(1, 0), grid)).IsFalse();
        AssertBool(WordSearchGenerator.TryPlaceWord("MALA", new Vector2I(0, 0), new Vector2I(2, 0), grid)).IsFalse();
    }

    [TestCase]
    public void GeneracionDeQuincePorQuinceBajoMedioSegundo()
    {
        DifficultyConfig maestro = DifficultyCatalog.LoadById("maestro");
        List<string> words = WordSearchGenerator.PrepareWords(WordBank.LoadAll()[0].Words, maestro.GridSize);
        AssertInt(words.Count).IsGreaterEqual(maestro.TargetWords);

        var stopwatch = Stopwatch.StartNew();
        GenerationResult result = _generator.Generate(maestro, words, seed: 42);
        stopwatch.Stop();

        GD.Print($"[UT-WS] 15×15 con {maestro.TargetWords} palabras: "
            + $"{result.PlacedCount} colocadas en {stopwatch.Elapsed.TotalMilliseconds:0.0} ms "
            + $"({result.Attempts} intento/s)");

        AssertBool(result.MeetsMinimum)
            .OverrideFailureMessage($"solo {result.PlacedCount} de {result.MinimumRequired} palabras")
            .IsTrue();
        AssertBool(stopwatch.Elapsed.TotalMilliseconds < 500.0)
            .OverrideFailureMessage($"15×15 tardó {stopwatch.Elapsed.TotalMilliseconds:0.0} ms (máx. 500)")
            .IsTrue();
    }

    private static string ReadPlacement(GenerationResult result, WordPlacement placement)
    {
        var text = new StringBuilder(placement.Word.Length);
        foreach (Vector2I coord in placement.Cells)
        {
            text.Append(result.GetLetter(coord));
        }

        return text.ToString();
    }
}
