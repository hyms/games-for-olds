using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 2 §2.2/§2.3 — Mezcla de categorías y sorteo de palabras (spec §5).
/// La Shuffle Bag entrega los grupos según la dificultad (1/1/2/4) y de ellos
/// salen las palabras del objetivo, todas A–Z y sin repetir.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSelectionTest
{
    [TestCase]
    public void LaDificultadDecidecuantasCategoriasSeMezclan()
    {
        AssertInt(PickCount("facil", seed: 1)).IsEqual(1);
        AssertInt(PickCount("medio", seed: 2)).IsEqual(1);
        AssertInt(PickCount("dificil", seed: 3)).IsEqual(2);
        AssertInt(PickCount("maestro", seed: 4)).IsEqual(4);
    }

    [TestCase]
    public void LasCategoriasSonDistintasYDelBanco()
    {
        List<string> bankIds = WordBank.Ids();
        foreach (string id in new[] { "facil", "dificil", "maestro" })
        {
            DifficultyConfig config = DifficultyCatalog.LoadById(id);
            var bag = new ShuffleBagSystem(bankIds, 99);
            WordSelectionResult selection = WordSelection.Pick(config, WordBank.LoadAll(), bag, seed: 8);

            var seen = new HashSet<string>();
            foreach (string categoryId in selection.CategoryIds)
            {
                AssertBool(bankIds.Contains(categoryId))
                    .OverrideFailureMessage($"{id}: categoría desconocida {categoryId}")
                    .IsTrue();
                AssertBool(seen.Add(categoryId))
                    .OverrideFailureMessage($"{id}: categoría repetida {categoryId}")
                    .IsTrue();
            }

            AssertInt(selection.CategoryIds.Count).IsEqual(config.CategoryCount);
        }
    }

    [TestCase]
    public void DevuelveElObjetivoDePalabrasNormalizadasYSinRepetir()
    {
        foreach (DifficultyConfig config in DifficultyCatalog.LoadAll())
        {
            var bag = new ShuffleBagSystem(WordBank.Ids(), 1234);
            WordSelectionResult selection = WordSelection.Pick(config, WordBank.LoadAll(), bag, seed: 21);

            AssertInt(selection.Words.Count)
                .OverrideFailureMessage($"{config.Id}: se esperaban {config.TargetWords} palabras")
                .IsEqual(config.TargetWords);

            var seen = new HashSet<string>();
            foreach (string word in selection.Words)
            {
                AssertString(WordSearchTheme.NormalizeWord(word)).IsEqual(word);
                AssertBool(seen.Add(word))
                    .OverrideFailureMessage($"{config.Id}: palabra repetida {word}")
                    .IsTrue();
                AssertBool(word.Length >= WordSearchTheme.MinBankWordLength && word.Length <= config.GridSize)
                    .OverrideFailureMessage($"{config.Id}: longitud inválida {word} ({word.Length})")
                    .IsTrue();
            }
        }
    }

    [TestCase]
    public void LaBolsaAvanzaEntrePartidas()
    {
        var bag = new ShuffleBagSystem(WordBank.Ids(), 77);
        DifficultyConfig easy = DifficultyCatalog.LoadById("facil");
        int before = bag.Remaining();

        WordSelectionResult first = WordSelection.Pick(easy, WordBank.LoadAll(), bag, seed: 1);
        WordSelectionResult second = WordSelection.Pick(easy, WordBank.LoadAll(), bag, seed: 2);

        AssertInt(first.Words.Count).IsEqual(easy.TargetWords);
        AssertInt(second.Words.Count).IsEqual(easy.TargetWords);
        AssertInt(bag.Remaining()).IsLess(before);
    }

    [TestCase]
    public void SinBolsaUsaElOrdenDelCatalogo()
    {
        DifficultyConfig config = DifficultyCatalog.LoadById("dificil");
        WordSelectionResult selection = WordSelection.Pick(config, WordBank.LoadAll(), null, seed: 5);

        AssertThat(selection.CategoryIds).ContainsExactly(
            WordBank.LoadAll()[0].CategoryId,
            WordBank.LoadAll()[1].CategoryId);
    }

    private static int PickCount(string id, long seed)
    {
        DifficultyConfig config = DifficultyCatalog.LoadById(id);
        var bag = new ShuffleBagSystem(WordBank.Ids(), seed);
        return WordSelection.Pick(config, WordBank.LoadAll(), bag, seed).CategoryIds.Count;
    }
}
