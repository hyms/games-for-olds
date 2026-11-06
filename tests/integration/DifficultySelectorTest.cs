using System;
using GdUnit4;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 2 §2.3 — Selector de dificultad: un botón por nivel del catálogo, con
///     los datos del recurso y tamaño táctil suficiente (spec §3 y §4.1).
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class DifficultySelectorTest
{
    [TestCase]
    public void CreaUnBotonPorNivelConLosDatosDelCatalogo()
    {
        WordSearchSession.Reset();
        var runner = ISceneRunner.Load(DifficultySelector.ScenePath, true);
        var scene = (DifficultySelector)runner.Scene();
        var levels = DifficultyCatalog.LoadAll();

        AssertInt(scene.LevelButtons.Count).OverrideFailureMessage("el selector no tiene un botón por nivel")
            .IsEqual(levels.Count);

        for (var i = 0; i < levels.Count; i++)
        {
            var button = scene.LevelButtons[i];
            AssertString(button.Name.ToString()).IsEqual($"Level_{levels[i].Id}");
            AssertBool(button.Text.Contains(levels[i].MenuLabel, StringComparison.Ordinal))
                .OverrideFailureMessage($"«{button.Text}» debe contener «{levels[i].MenuLabel}»")
                .IsTrue();
            AssertBool(button.Text.Contains("/20", StringComparison.Ordinal))
                .OverrideFailureMessage($"«{button.Text}» debe mostrar el progreso X/20")
                .IsTrue();
            AssertBool(button.CustomMinimumSize.Y >= 56.0f)
                .OverrideFailureMessage($"{button.Name}: alto {button.CustomMinimumSize.Y} px")
                .IsTrue();
            AssertInt(button.GetThemeFontSize("font_size")).IsEqual(WordSearchVisuals.LevelButtonFontSize);
        }

        AssertObject(scene.GetViewport().GuiGetFocusOwner())
            .OverrideFailureMessage("el selector debe arrancar con foco para teclado/mando")
            .IsNotNull();
    }
}