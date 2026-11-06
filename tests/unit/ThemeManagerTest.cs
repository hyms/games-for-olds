using System.IO;
using System.Reflection;
using ClasicosMenteActiva.core;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 3 §3.2 — ThemeManager (CORE-THEME-01 a CORE-THEME-03).
///     Cambio de tema, tamaño de texto y persistencia.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class ThemeManagerTest
{
    private const string TestSavePath = "user://test_theme_save.json";
    private const string TestBackupPath = "user://test_theme_save.json.bak";

    private ThemeManager _theme;

    [BeforeTest]
    public void BeforeTest()
    {
        DeleteTestFiles();
        _theme = new ThemeManager();
        _theme.SetTheme("default");
        _theme.SetTextSize(ThemeManager.SizeNormal);
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
        _theme.ThemeChanged += t => emitted = t;

        _theme.SetTheme("dark_contrast");

        AssertString(emitted).IsEqual("dark_contrast");
        AssertString(_theme.CurrentTheme).IsEqual("dark_contrast");
    }

    [TestCase]
    public void CambioDeTamanoTextoSinCambiarViewport() // CORE-THEME-02
    {
        _theme.SetTextSize(ThemeManager.SizeLarge);

        AssertString(_theme.CurrentTextSize).IsEqual(ThemeManager.SizeLarge);
        AssertInt(_theme.CurrentFontSize).IsEqual(ThemeManager.FontLarge);

        _theme.SetTextSize(ThemeManager.SizeXLarge);
        AssertInt(_theme.CurrentFontSize).IsEqual(ThemeManager.FontXLarge);

        _theme.SetTextSize(ThemeManager.SizeNormal);
        AssertInt(_theme.CurrentFontSize).IsEqual(ThemeManager.FontNormal);
    }

    [TestCase]
    public void TamanoTextoPersisteEntreReinicios() // CORE-THEME-03
    {
        _theme.SetTextSize(ThemeManager.SizeXLarge);

        var save = NewSaveManager();
        save.Settings.TextSize = ThemeManager.SizeXLarge;
        save.Settings.Theme = "dark_contrast";
        save.SaveToFile();
        save.Free();

        var save2 = NewSaveManager();
        save2.LoadData();

        AssertString(save2.Settings.TextSize).IsEqual(ThemeManager.SizeXLarge);
        AssertString(save2.Settings.Theme).IsEqual("dark_contrast");
        save2.Free();
    }

    /// <summary>Crea un SaveManager apuntando a los archivos de prueba.</summary>
    private static SaveManager NewSaveManager()
    {
        var save = new SaveManager();
        SetPath(save, "_savePath", TestSavePath);
        SetPath(save, "_backupPath", TestBackupPath);
        return save;
    }

    private static void SetPath(SaveManager save, string fieldName, string godotPath)
    {
        typeof(SaveManager)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(save, ProjectSettings.GlobalizePath(godotPath));
    }

    private static void DeleteTestFiles()
    {
        var savePath = ProjectSettings.GlobalizePath(TestSavePath);
        var backupPath = ProjectSettings.GlobalizePath(TestBackupPath);
        if (File.Exists(savePath)) File.Delete(savePath);

        if (File.Exists(backupPath)) File.Delete(backupPath);
    }
}