using System.IO;
using System.Reflection;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 §3.2 — ThemeManager (CORE-THEME-01 a CORE-THEME-03).
/// Cambio de tema, tamaño de texto y persistencia.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class ThemeManagerTest
{
    private const string TestSavePath = "user://test_theme_save.json";
    private const string TestBackupPath = "user://test_theme_save.json.bak";

    private ClasicosMenteActiva.core.ThemeManager _theme;

    [BeforeTest]
    public void BeforeTest()
    {
        DeleteTestFiles();
        _theme = new ClasicosMenteActiva.core.ThemeManager();
        _theme.SetTheme("default");
        _theme.SetTextSize(ClasicosMenteActiva.core.ThemeManager.SizeNormal);
    }

    [AfterTest]
    public void AfterTest()
    {
        _theme?.Free();
        DeleteTestFiles();
    }

    [TestCase]
    public void CambioDeTemaEmiteSenal() // CORE-THEME-01
    {
        string emitted = null;
        _theme.ThemeChanged += (t) => emitted = t;

        _theme.SetTheme("dark_contrast");

        AssertString(emitted).IsEqual("dark_contrast");
        AssertString(_theme.CurrentTheme).IsEqual("dark_contrast");
    }

    [TestCase]
    public void CambioDeTamanoTextoSinCambiarViewport() // CORE-THEME-02
    {
        _theme.SetTextSize(ClasicosMenteActiva.core.ThemeManager.SizeLarge);

        AssertString(_theme.CurrentTextSize).IsEqual(ClasicosMenteActiva.core.ThemeManager.SizeLarge);
        AssertInt(_theme.CurrentFontSize).IsEqual(ClasicosMenteActiva.core.ThemeManager.FontLarge);

        _theme.SetTextSize(ClasicosMenteActiva.core.ThemeManager.SizeXLarge);
        AssertInt(_theme.CurrentFontSize).IsEqual(ClasicosMenteActiva.core.ThemeManager.FontXLarge);

        _theme.SetTextSize(ClasicosMenteActiva.core.ThemeManager.SizeNormal);
        AssertInt(_theme.CurrentFontSize).IsEqual(ClasicosMenteActiva.core.ThemeManager.FontNormal);
    }

    [TestCase]
    public void TamanoTextoPersisteEntreReinicios() // CORE-THEME-03
    {
        _theme.SetTextSize(ClasicosMenteActiva.core.ThemeManager.SizeXLarge);

        ClasicosMenteActiva.core.SaveManager save = NewSaveManager();
        save.Settings.TextSize = ClasicosMenteActiva.core.ThemeManager.SizeXLarge;
        save.Settings.Theme = "dark_contrast";
        save.SaveToFile();
        save.Free();

        ClasicosMenteActiva.core.SaveManager save2 = NewSaveManager();
        save2.LoadData();

        AssertString(save2.Settings.TextSize).IsEqual(ClasicosMenteActiva.core.ThemeManager.SizeXLarge);
        AssertString(save2.Settings.Theme).IsEqual("dark_contrast");
        save2.Free();
    }

    /// <summary>Crea un SaveManager apuntando a los archivos de prueba.</summary>
    private static ClasicosMenteActiva.core.SaveManager NewSaveManager()
    {
        var save = new ClasicosMenteActiva.core.SaveManager();
        SetPath(save, "_savePath", TestSavePath);
        SetPath(save, "_backupPath", TestBackupPath);
        return save;
    }

    private static void SetPath(ClasicosMenteActiva.core.SaveManager save, string fieldName, string godotPath)
    {
        typeof(ClasicosMenteActiva.core.SaveManager)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(save, ProjectSettings.GlobalizePath(godotPath));
    }

    private static void DeleteTestFiles()
    {
        string savePath = ProjectSettings.GlobalizePath(TestSavePath);
        string backupPath = ProjectSettings.GlobalizePath(TestBackupPath);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }

        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
        }
    }
}
