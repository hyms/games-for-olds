using GdUnit4;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 3 §3.4 — AdManager (CORE-AD-01 a CORE-AD-04).
///     Modo no-op para el MVP: sin anuncios reales.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class AdManagerTest
{
    private AdManager _ads;

    [BeforeTest]
    public void BeforeTest()
    {
        _ads = new AdManager();
    }

    [TestCase]
    public void NoOpEnEditorSinErrores() // CORE-AD-01
    {
        // No debería lanzar ni cargar anuncio real.
        _ads.ShowBanner("main_menu");
        _ads.HideBanner("main_menu");

        AssertBool(_ads.IsEnabled).IsFalse();
        AssertBool(_ads.ConsentGranted).IsFalse();
    }

    [TestCase]
    public void BannerFailedSinRomperLayout() // CORE-AD-02
    {
        string failedLocation = null;
        _ads.BannerFailed += loc => failedLocation = loc;

        _ads.SimulateBannerFailed("main_menu");

        AssertString(failedLocation).IsEqual("main_menu");
    }

    [TestCase]
    public void SoloUbicacionesPermitidas() // CORE-AD-03
    {
        AssertBool(_ads.IsLocationAllowed("main_menu")).IsTrue();
        AssertBool(_ads.IsLocationAllowed("statistics")).IsTrue();
        AssertBool(_ads.IsLocationAllowed("gameplay")).IsFalse();
        AssertBool(_ads.IsLocationAllowed("in_game")).IsFalse();
    }

    [TestCase]
    public void SinAnunciosEnPartida() // CORE-AD-04
    {
        // En el MVP no hay contenedor publicitario en la partida.
        AssertBool(_ads.IsLocationAllowed("gameplay")).IsFalse();
        AssertBool(_ads.IsLocationAllowed("word_search_board")).IsFalse();
    }
}