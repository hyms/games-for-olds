using System.Collections.Generic;
using System.IO;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 §3.1 — SaveManager (CORE-SAVE-01 a CORE-SAVE-06).
/// Persistencia, estadísticas por juego, corrupción, escritura segura,
/// migración y reinicio.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class SaveManagerTest
{
    private const string TestSavePath = "user://test_save_data.json";
    private const string TestBackupPath = "user://test_save_data.json.bak";

    private ClasicosMenteActiva.core.SaveManager _save;

    [BeforeTest]
    public void BeforeTest()
    {
        _save = new ClasicosMenteActiva.core.SaveManager();
        // Forzar ruta de prueba para no tocar el save real.
        var pathField = typeof(ClasicosMenteActiva.core.SaveManager).GetField("_savePath",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        pathField?.SetValue(_save, ProjectSettings.GlobalizePath(TestSavePath));

        var backupField = typeof(ClasicosMenteActiva.core.SaveManager).GetField("_backupPath",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backupField?.SetValue(_save, ProjectSettings.GlobalizePath(TestBackupPath));

        // Limpiar archivos de prueba previos.
        string saveFile = ProjectSettings.GlobalizePath(TestSavePath);
        string backupFile = ProjectSettings.GlobalizePath(TestBackupPath);
        if (File.Exists(saveFile)) File.Delete(saveFile);
        if (File.Exists(backupFile)) File.Delete(backupFile);
    }

    [AfterTest]
    public void AfterTest()
    {
        string saveFile = ProjectSettings.GlobalizePath(TestSavePath);
        string backupFile = ProjectSettings.GlobalizePath(TestBackupPath);
        if (File.Exists(saveFile)) File.Delete(saveFile);
        if (File.Exists(backupFile)) File.Delete(backupFile);
    }

    [TestCase]
    public void LoadDataCreaArchivoPorDefecto() // CORE-SAVE-01
    {
        _save.LoadData();

        AssertObject(_save.Data).IsNotNull();
        AssertInt(_save.Data.Version).IsEqual(ClasicosMenteActiva.core.SaveManager.SaveVersion);
        AssertInt(_save.Data.Stats.Count).IsEqual(3);
        AssertBool(File.Exists(ProjectSettings.GlobalizePath(TestSavePath))).IsTrue();
    }

    [TestCase]
    public void RegistrarResultadoAvanzaNivelYMejoraEstadisticas() // CORE-SAVE-02
    {
        _save.LoadData();

        _save.RegisterResult("word_search", "medio", true, 120f, 500);
        _save.RegisterResult("word_search", "medio", true, 100f, 700); // más rápido, más puntaje

        var stats = _save.GetStats("word_search");
        AssertObject(stats).IsNotNull();
        AssertInt(stats.Plays).IsEqual(2);
        AssertInt(stats.Wins).IsEqual(2);
        AssertFloat(stats.BestTimeSeconds.Value).IsEqual(100f);
        AssertInt(stats.BestScore).IsEqual(700);
        AssertString(stats.CurrentLevelId).IsEqual("dificil"); // avanzó de medio
    }

    [TestCase]
    public void ResultadoMasLentoNoEmpeoraMejorTiempo() // CORE-SAVE-02
    {
        _save.LoadData();

        _save.RegisterResult("word_search", "medio", true, 100f, 500);
        _save.RegisterResult("word_search", "medio", true, 150f, 300); // más lento

        var stats = _save.GetStats("word_search");
        AssertFloat(stats.BestTimeSeconds.Value).IsEqual(100f); // se conserva el mejor
        AssertInt(stats.BestScore).IsEqual(500); // se conserva el mejor puntaje
    }

    [TestCase]
    public void JsonCorruptoRestauraValoresPorDefecto() // CORE-SAVE-03
    {
        _save.LoadData();
        _save.RegisterResult("word_search", "medio", true, 100f, 500);

        // Corromper el archivo.
        File.WriteAllText(ProjectSettings.GlobalizePath(TestSavePath), "{ esto no es json valido");

        _save.LoadData();

        var stats = _save.GetStats("word_search");
        AssertObject(stats).IsNotNull();
        AssertInt(stats.Plays).IsEqual(0);
        AssertInt(stats.Wins).IsEqual(0);
    }

    [TestCase]
    public void EscrituraSeguraNoDejaJsonAMedias() // CORE-SAVE-04
    {
        _save.LoadData();

        // Escribir directamente un archivo válido primero.
        _save.RegisterResult("word_search", "facil", true, 60f, 100);

        // Verificar que el archivo es JSON válido.
        string content = File.ReadAllText(ProjectSettings.GlobalizePath(TestSavePath));
        AssertBool(content.TrimStart().StartsWith("{")).IsTrue();

        // Guardar nuevamente y verificar que sigue siendo válido.
        _save.SaveToFile();
        content = File.ReadAllText(ProjectSettings.GlobalizePath(TestSavePath));
        AssertBool(content.TrimStart().StartsWith("{")).IsTrue();
    }

    [TestCase]
    public void MigracionDeVersionActualizaEsquema() // CORE-SAVE-05
    {
        _save.LoadData();
        _save.Data.Version = 0; // simular versión antigua
        _save.SaveToFile();

        _save.LoadData();

        AssertInt(_save.Data.Version).IsEqual(ClasicosMenteActiva.core.SaveManager.SaveVersion);
    }

    [TestCase]
    public void ResetStatsVuelveAValoresIniciales() // CORE-SAVE-06
    {
        _save.LoadData();
        _save.RegisterResult("word_search", "medio", true, 100f, 500);

        _save.ResetStats();

        var stats = _save.GetStats("word_search");
        AssertObject(stats).IsNotNull();
        AssertInt(stats.Plays).IsEqual(0);
        AssertInt(stats.Wins).IsEqual(0);
        AssertString(stats.CurrentLevelId).IsEqual("facil");
    }
}
