using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 2 §2.1 — Datos de dificultad (spec §4.1 y §4.2).
/// Carga los cuatro recursos <c>.tres</c> y verifica tablero, objetivo,
/// mínimo del 80 %, categorías, direcciones, par y encadenado de niveles.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class DifficultyConfigTest
{
    [TestCase]
    public void CatalogoCargaLosCuatroNivelesEnOrden()
    {
        List<DifficultyConfig> levels = DifficultyCatalog.LoadAll();
        AssertBool(levels.Count == 4)
            .OverrideFailureMessage($"se esperaban 4 niveles, hay {levels.Count}")
            .IsTrue();
        AssertString(levels[0].Id).IsEqual("facil");
        AssertString(levels[1].Id).IsEqual("medio");
        AssertString(levels[2].Id).IsEqual("dificil");
        AssertString(levels[3].Id).IsEqual("maestro");
    }

    [TestCase]
    public void FacilCumpleLaFilaDeLaSpec()
    {
        AssertLevel(DifficultyCatalog.LoadById("facil"), "Fácil", 8, 5, 4, 1, 240, string.Empty,
            new Vector2I(1, 0), new Vector2I(0, 1));
    }

    [TestCase]
    public void MedioCumpleLaFilaDeLaSpec()
    {
        AssertLevel(DifficultyCatalog.LoadById("medio"), "Medio", 10, 8, 7, 1, 420, "facil",
            new Vector2I(1, 0), new Vector2I(0, 1), new Vector2I(1, 1), new Vector2I(-1, 1));
    }

    [TestCase]
    public void DificilCumpleLaFilaDeLaSpec()
    {
        AssertLevel(DifficultyCatalog.LoadById("dificil"), "Difícil", 12, 12, 10, 2, 600, "medio",
            WordPath.AllDirections);
    }

    [TestCase]
    public void MaestroCumpleLaFilaDeLaSpec()
    {
        AssertLevel(DifficultyCatalog.LoadById("maestro"), "Maestro", 15, 20, 16, 4, 720, "dificil",
            WordPath.AllDirections);
    }

    [TestCase]
    public void FacilNoAdmiteDiagonalesNiReversos()
    {
        DifficultyConfig easy = DifficultyCatalog.LoadById("facil");
        AssertInt(easy.Directions.Count).IsEqual(2);
        AssertBool(easy.AllowsDirection(new Vector2I(1, 0))).IsTrue();
        AssertBool(easy.AllowsDirection(new Vector2I(0, 1))).IsTrue();
        AssertBool(easy.AllowsDirection(new Vector2I(1, 1))).IsFalse();
        AssertBool(easy.AllowsDirection(new Vector2I(-1, 0))).IsFalse();
        AssertBool(easy.AllowsDirection(new Vector2I(0, -1))).IsFalse();
    }

    [TestCase]
    public void LosNivelesSeEncadenanSinHuecos()
    {
        List<DifficultyConfig> levels = DifficultyCatalog.LoadAll();
        AssertString(levels[0].PreviousLevelId).IsEmpty();
        for (int i = 1; i < levels.Count; i++)
        {
            AssertString(levels[i].PreviousLevelId).IsEqual(levels[i - 1].Id);
        }
    }

    [TestCase]
    public void MinimumForAplicaLosOchentaPorCiento()
    {
        AssertInt(DifficultyConfig.MinimumFor(5, 4)).IsEqual(4);
        AssertInt(DifficultyConfig.MinimumFor(8, 7)).IsEqual(7);
        AssertInt(DifficultyConfig.MinimumFor(12, 10)).IsEqual(10);
        AssertInt(DifficultyConfig.MinimumFor(20, 16)).IsEqual(16);
        AssertInt(DifficultyConfig.MinimumFor(20, 15)).IsEqual(16); // nunca por debajo del 80 %
        AssertInt(DifficultyConfig.MinimumFor(20, 30)).IsEqual(20); // nunca por encima del objetivo
        AssertInt(DifficultyConfig.EightyPercent(20)).IsEqual(16);
    }

    [TestCase]
    public void EffectiveMinimumCoincideConLaReglaDelOchentaPorCiento()
    {
        foreach (DifficultyConfig level in DifficultyCatalog.LoadAll())
        {
            AssertInt(level.EffectiveMinimum)
                .OverrideFailureMessage(level.Id)
                .IsEqual(DifficultyConfig.MinimumFor(level.TargetWords, level.MinimumWords));
        }
    }

    [TestCase]
    public void ValidateDetectaDatosIncoherentes()
    {
        var broken = new DifficultyConfig
        {
            Id = string.Empty,
            DisplayName = string.Empty,
            GridSize = 99,
            TargetWords = 0,
            MinimumWords = 0,
            CategoryCount = 0,
            ParSeconds = 0,
        };
        broken.Directions.Clear();
        broken.Directions.Add(new Vector2I(2, 0));

        string[] errors = broken.Validate();
        AssertBool(errors.Length >= 5)
            .OverrideFailureMessage($"se esperaban varios errores, hay {errors.Length}: {string.Join(" | ", errors)}")
            .IsTrue();
    }

    private static void AssertLevel(
        DifficultyConfig level,
        string name,
        int gridSize,
        int target,
        int minimum,
        int categories,
        int parSeconds,
        string previous,
        params Vector2I[] directions)
    {
        AssertObject(level).IsNotNull();
        AssertString(level.DisplayName).IsEqual(name);
        AssertInt(level.GridSize).IsEqual(gridSize);
        AssertInt(level.TargetWords).IsEqual(target);
        AssertInt(level.MinimumWords).IsEqual(minimum);
        AssertInt(level.CategoryCount).IsEqual(categories);
        AssertInt(level.ParSeconds).IsEqual(parSeconds);
        AssertString(level.PreviousLevelId).IsEqual(previous);

        string[] errors = level.Validate();
        AssertBool(errors.Length == 0)
            .OverrideFailureMessage($"{level.Id}: {string.Join(", ", errors)}")
            .IsTrue();

        AssertInt(level.Directions.Count).IsEqual(directions.Length);
        foreach (Vector2I direction in directions)
        {
            AssertBool(level.AllowsDirection(direction))
                .OverrideFailureMessage($"{level.Id} no permite {direction}")
                .IsTrue();
        }
    }
}
