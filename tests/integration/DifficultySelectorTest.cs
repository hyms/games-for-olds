using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 2 §2.3 — Selector de dificultad: un botón por nivel del catálogo, con
/// los datos del recurso y tamaño táctil suficiente (spec §3 y §4.1).
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class DifficultySelectorTest
{
    [TestCase]
    public void CreaUnBotonPorNivelConLosDatosDelCatalogo()
    {
        WordSearchSession.Reset();
        ISceneRunner runner = ISceneRunner.Load(DifficultySelector.ScenePath, true);
        var scene = (DifficultySelector)runner.Scene();
        List<DifficultyConfig> levels = DifficultyCatalog.LoadAll();

        AssertInt(scene.LevelButtons.Count).
            OverrideFailureMessage("el selector no tiene un botón por nivel").
            IsEqual(levels.Count);

        for (int i = 0; i < levels.Count; i++)
        {
            BigButton button = scene.LevelButtons[i];
            AssertString(button.Name.ToString()).IsEqual($"Level_{levels[i].Id}");
            AssertString(button.Text).IsEqual(levels[i].MenuLabel);
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
