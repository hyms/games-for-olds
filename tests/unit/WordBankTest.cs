using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 1 — Banco de palabras (spec §5 y §5.1 · plan Fase 1 · UT-WS-07).
/// Carga los 10 recursos y verifica: ids y nombres de la spec, ≥60 palabras
/// válidas por categoría (4–15 letras A–Z, sin duplicar), ≥600 en total y
/// ninguna palabra repetida entre categorías (C3: partidas de 2 o 4 grupos).
/// Equivalente C# de <c>tests/unit/WordBankTest.gd</c>.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordBankTest
{
    private static readonly string[] ExpectedIds =
    {
        "animales", "plantas", "frutas_verduras", "objetos", "profesiones",
        "paises_ciudades", "comidas", "instrumentos", "herramientas", "ropa",
    };

    private static readonly string[] ExpectedNames =
    {
        "Animales y Naturaleza", "Plantas y Flores", "Frutas y Verduras",
        "Cosas del Hogar", "Profesiones y Oficios", "Países y Geografía",
        "Comidas y Postres", "Instrumentos Musicales", "Herramientas y Bricolaje",
        "Ropa y Costura",
    };

    [TestCase]
    public void BancoCargaLasDiezCategoriasDeLaSpec()
    {
        List<WordSearchTheme> categories = WordBank.LoadAll();
        AssertInt(categories.Count).IsEqual(10);

        var ids = new List<string>();
        var names = new List<string>();
        foreach (WordSearchTheme category in categories)
        {
            ids.Add(category.CategoryId);
            names.Add(category.ThemeName);
        }

        AssertThat(ids).ContainsExactly(ExpectedIds);
        AssertThat(names).ContainsExactly(ExpectedNames);
    }

    [TestCase]
    public void CadaCategoriaPasaSuValidacion()
    {
        foreach (WordSearchTheme category in WordBank.LoadAll())
        {
            string[] errors = category.Validate();
            AssertBool(errors.Length == 0)
                .OverrideFailureMessage($"{category.CategoryId}: {string.Join(", ", errors)}")
                .IsTrue();
        }
    }

    [TestCase]
    public void CadaCategoriaTieneAlMenos60PalabrasValidas()
    {
        foreach (WordSearchTheme category in WordBank.LoadAll())
        {
            int valid = 0;
            foreach (string word in category.Words)
            {
                string normalized = WordSearchTheme.NormalizeWord(word);
                if (normalized == word && normalized.Length >= 4 && normalized.Length <= 15)
                {
                    valid++;
                }
            }

            AssertInt(valid)
                .OverrideFailureMessage($"{category.CategoryId}: solo {valid} palabras válidas (mínimo 60)")
                .IsGreaterEqual(60);
        }
    }

    [TestCase]
    public void TotalDelBancoSupera600Palabras()
    {
        int total = 0;
        foreach (WordSearchTheme category in WordBank.LoadAll())
        {
            total += category.Words.Count;
        }

        AssertInt(total).OverrideFailureMessage($"total del banco = {total} (mínimo 600)").IsGreaterEqual(600);
    }

    [TestCase]
    public void NingunaPalabraSeRepiteEntreCategorias()
    {
        var ownerByWord = new Dictionary<string, string>();
        foreach (WordSearchTheme category in WordBank.LoadAll())
        {
            foreach (string word in category.Words)
            {
                AssertBool(ownerByWord.ContainsKey(word))
                    .OverrideFailureMessage(
                        $"'{word}' está en {(ownerByWord.TryGetValue(word, out string owner) ? owner : "?")} y en {category.CategoryId}")
                    .IsFalse();
                ownerByWord[word] = category.CategoryId;
            }
        }

        AssertInt(ownerByWord.Count).IsGreaterEqual(600);
    }

    [TestCase]
    public void LoadByIdDevuelveLaCategoriaPedida()
    {
        WordSearchTheme category = WordBank.LoadById("instrumentos");
        AssertBool(category != null).IsTrue();
        AssertString(category.ThemeName).IsEqual("Instrumentos Musicales");
        AssertBool(WordBank.LoadById("no_existe") == null).IsTrue();
    }
}
