using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WordSearchSessionTest
{
    [Before]
    public void Before()
    {
        WordSearchSession.Reset();
        DifficultyCatalog.InvalidateCache();
    }

    [After]
    public void After() => WordSearchSession.Reset();

    [TestCase]
    public void PorDefectoResuelveFacilYComparteBolsa()
    {
        AssertString(WordSearchSession.SelectedId).IsEqual(DifficultyCatalog.DefaultId);
        AssertObject(WordSearchSession.SelectedDifficulty).IsNull();

        var resolved = WordSearchSession.ResolveDifficulty();
        AssertObject(resolved).IsNotNull();
        AssertString(resolved.Id).IsEqual("facil");
        AssertObject(WordSearchSession.SelectedDifficulty).IsNotNull();

        var bag1 = WordSearchSession.Bag;
        var bag2 = WordSearchSession.Bag;
        AssertObject(bag1).IsNotNull();
        AssertBool(ReferenceEquals(bag1, bag2)).IsTrue();
    }

    [TestCase]
    public void SelectFijaNivelYResetLimpiaEstado()
    {
        var medio = DifficultyCatalog.LoadById("medio");
        AssertObject(medio).IsNotNull();
        WordSearchSession.Select(medio);
        AssertString(WordSearchSession.SelectedId).IsEqual("medio");
        AssertObject(WordSearchSession.ResolveDifficulty()).IsEqual(medio);

        WordSearchSession.ResetBag(123);
        var remaining = WordSearchSession.Bag.Remaining();
        AssertInt(remaining).IsEqual(20);

        WordSearchSession.Reset();
        AssertObject(WordSearchSession.SelectedDifficulty).IsNull();
        AssertString(WordSearchSession.SelectedId).IsEqual(DifficultyCatalog.DefaultId);
        AssertInt(WordSearchSession.Bag.Remaining()).IsEqual(20);
    }

    [TestCase]
    public void ResolveDifficultyConservaSeleccionExistente()
    {
        var dificil = DifficultyCatalog.LoadById("dificil");
        WordSearchSession.SelectedDifficulty = dificil;
        var resolved = WordSearchSession.ResolveDifficulty();
        AssertObject(resolved).IsEqual(dificil);
        AssertString(WordSearchSession.SelectedId).IsEqual("dificil");
    }
}
