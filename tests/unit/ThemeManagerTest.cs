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
    private ThemeManager _theme;

    [BeforeTest]
    public void BeforeTest()
    {
        _theme = new ThemeManager();
        _theme.SetTheme("default");
        _theme.SetTextSize(ThemeManager.SizeNormal);
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

        var save = new SaveManager();
        save.Settings.TextSize = ThemeManager.SizeXLarge;
        save.Settings.Theme = "dark_contrast";
        save.SaveToFile();

        var save2 = new SaveManager();
        save2.LoadData();

        AssertString(save2.Settings.TextSize).IsEqual(ThemeManager.SizeXLarge);
        AssertString(save2.Settings.Theme).IsEqual("dark_contrast");
    }
}
