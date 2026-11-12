using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class HintSystemTest
{
    [TestCase]
    public void DevuelvePistasPorNivelYLimites()
    {
        AssertInt(HintSystem.GetHintCount("facil")).IsEqual(3);
        AssertInt(HintSystem.GetHintCount("medio")).IsEqual(3);
        AssertInt(HintSystem.GetHintCount("dificil")).IsEqual(3);
        AssertInt(HintSystem.GetHintCount("maestro")).IsEqual(3);
        AssertInt(HintSystem.GetHintCount("")).IsEqual(0);
        AssertInt(HintSystem.GetHintCount("desconocido")).IsEqual(0);

        var hints = HintSystem.GetHints("facil");
        AssertInt(hints.Length).IsEqual(3);
        AssertBool(!string.IsNullOrWhiteSpace(hints[0])).IsTrue();
        AssertString(HintSystem.GetHint("facil", 0)).IsEqual(hints[0]);
        AssertString(HintSystem.GetHint("facil", 99)).IsEqual("Pista no disponible en este nivel.");
        AssertString(HintSystem.GetHint(null, 0)).IsEqual("Sin pista disponible.");

        var none = HintSystem.GetHints("desconocido");
        AssertInt(none.Length).IsEqual(1);
        AssertString(none[0]).IsEqual("Sin pistas disponibles.");
    }
}
