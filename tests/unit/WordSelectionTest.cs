using System.Collections.Generic;
using GdUnit4;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 2 §2.2/§2.3 — Mezcla de categorías y sorteo de palabras (spec §5).
///     La Shuffle Bag entrega los grupos según la dificultad (1/1/2/4) y de ellos
///     salen las palabras del objetivo, todas A–Z y sin repetir.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSelectionTest
{
    [TestCase]
    public void LaDificultadDecidecuantasCategoriasSeMezclan()
    {
        AssertInt(PickCount("facil", 1)).IsEqual(1);
        AssertInt(PickCount("medio", 2)).IsEqual(1);
        AssertInt(PickCount("dificil", 3)).IsEqual(2);
        AssertInt(PickCount("maestro", 4)).IsEqual(4);
    }

    [TestCase]
    public void LasCategoriasSonDistintasYDelBanco()
    {
        var bankIds = WordBank.Ids();
        foreach (var id in new[] { "facil", "dificil", "maestro" })
        {
            var config = DifficultyCatalog.LoadById(id);
            var bag = new ShuffleBagSystem(bankIds, 99);
            var selection = WordSelection.Pick(config, WordBank.LoadAll(), bag, 8);

            var seen = new HashSet<string>();
            foreach (var categoryId in selection.CategoryIds)
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
        foreach (var config in DifficultyCatalog.LoadAll())
        {
            var bag = new ShuffleBagSystem(WordBank.Ids(), 1234);
            var selection = WordSelection.Pick(config, WordBank.LoadAll(), bag, 21);

            AssertInt(selection.Words.Count)
                .OverrideFailureMessage($"{config.Id}: se esperaban {config.TargetWords} palabras")
                .IsEqual(config.TargetWords);

            var seen = new HashSet<string>();
            foreach (var word in selection.Words)
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
        var easy = DifficultyCatalog.LoadById("facil");
        var before = bag.Remaining();

        var first = WordSelection.Pick(easy, WordBank.LoadAll(), bag, 1);
        var second = WordSelection.Pick(easy, WordBank.LoadAll(), bag, 2);

        AssertInt(first.Words.Count).IsEqual(easy.TargetWords);
        AssertInt(second.Words.Count).IsEqual(easy.TargetWords);
        AssertInt(bag.Remaining()).IsLess(before);
    }

    [TestCase]
    public void SinBolsaUsaElOrdenDelCatalogo()
    {
        var config = DifficultyCatalog.LoadById("dificil");
        var selection = WordSelection.Pick(config, WordBank.LoadAll(), null, 5);

        var allCategories = WordBank.LoadAll();
        AssertInt(selection.CategoryIds.Count).IsEqual(2);
        AssertString(selection.CategoryIds[0]).IsEqual(allCategories[0].CategoryId);
        AssertString(selection.CategoryIds[1]).IsEqual(allCategories[1].CategoryId);
    }

    private static int PickCount(string id, long seed)
    {
        var config = DifficultyCatalog.LoadById(id);
        var bag = new ShuffleBagSystem(WordBank.Ids(), seed);
        return WordSelection.Pick(config, WordBank.LoadAll(), bag, seed).CategoryIds.Count;
    }
}