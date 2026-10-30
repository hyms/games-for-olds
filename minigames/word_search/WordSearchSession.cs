using Godot;

/// <summary>
/// Estado de sesión de la Sopa de Letras entre el selector de dificultad y el
/// tablero: nivel elegido y bolsa de grupos (plan Fase 2 §2.3).
/// Provisional hasta que <c>SaveManager</c> persista la bolsa y el progreso
/// (Fase 3 global y Fase 7); mientras tanto vive en memoria estática.
/// </summary>
public static class WordSearchSession
{
    private static ShuffleBagSystem _bag;

    /// <summary>Nivel elegido en el selector; null = usar el nivel por defecto.</summary>
    public static DifficultyConfig SelectedDifficulty { get; set; }

    /// <summary>Id del nivel elegido (Fácil si aún no se ha elegido ninguno).</summary>
    public static string SelectedId => SelectedDifficulty?.Id ?? DifficultyCatalog.DefaultId;

    /// <summary>
    /// Bolsa de grupos compartida entre partidas: en 20 partidas cada grupo
    /// temático aparece dos veces (spec §5 · UT-WS-06).
    /// </summary>
    public static ShuffleBagSystem Bag => _bag ??= new ShuffleBagSystem(WordBank.Ids());

    /// <summary>Fija el nivel que jugará la próxima partida.</summary>
    public static void Select(DifficultyConfig difficulty) => SelectedDifficulty = difficulty;

    /// <summary>Nivel actual; si no hay selección carga el Fácil del catálogo.</summary>
    public static DifficultyConfig ResolveDifficulty()
    {
        if (SelectedDifficulty != null)
        {
            return SelectedDifficulty;
        }

        SelectedDifficulty = DifficultyCatalog.LoadById(DifficultyCatalog.DefaultId) ?? DifficultyCatalog.First();
        return SelectedDifficulty;
    }

    /// <summary>Reinicia la bolsa (tests deterministas o partida nueva desde cero).</summary>
    public static void ResetBag(long seed = -1)
    {
        _bag = new ShuffleBagSystem(WordBank.Ids(), seed);
    }

    /// <summary>Olvida nivel y bolsa (tests y cierre de sesión).</summary>
    public static void Reset()
    {
        SelectedDifficulty = null;
        _bag = null;
    }
}
