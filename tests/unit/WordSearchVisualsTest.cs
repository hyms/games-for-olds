using ClasicosMenteActiva.core;
using ClasicosMenteActiva.core.ui;
using ClasicosMenteActiva.minigames.word_search;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
///     Fase 4 — Visual constants del tablero (WS-VIS-01 a WS-VIS-04).
///     Valida la métrica estática de la grilla de referencia
///     (docs/word_search/art_direction.md §3.1) y los límites del zoom
///     (§4). No levanta escenas: <see cref="WordSearchVisuals" /> es una clase
///     estática pura, así que las pruebas son deterministas y rápidas.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchVisualsTest
{
    /// <summary>WS-VIS-01: el baseline de referencia es 8×8 / 30×30 px / 6 px gap.</summary>
    [TestCase]
    public void BaselineFacilEsTreintaPorTreintaConGapSeis()
    {
        var (cellSize, gap) = WordSearchVisuals.BoardMetricsFor(8, 8);

        AssertInt(cellSize).IsEqual(30);
        AssertInt(gap).IsEqual(6);
    }

    /// <summary>WS-VIS-01b: una grilla 10×10 hereda el mismo baseline (fidelidad).</summary>
    [TestCase]
    public void BaselineMedioComparteMetricaDeReferencia()
    {
        var (cellSize, gap) = WordSearchVisuals.BoardMetricsFor(10, 10);

        AssertInt(cellSize).IsEqual(30);
        AssertInt(gap).IsEqual(6);
    }

    /// <summary>WS-VIS-02: las variantes por dificultad siguen la tabla del spec.</summary>
    [TestCase]
    public void VariantesPorDificultadSiguenLaSpec()
    {
        AssertInt(WordSearchVisuals.BoardMetricsFor(12, 12).CellSize).IsEqual(24);
        AssertInt(WordSearchVisuals.BoardMetricsFor(15, 15).CellSize).IsEqual(20);
    }

    /// <summary>WS-VIS-03: el piso de seguridad evita celdas de 0 px.</summary>
    [TestCase]
    public void MinCellSizeEvitaCeldasDeCero()
    {
        AssertInt(WordSearchVisuals.MinCellSize).IsGreaterEqual(16);
    }

    /// <summary>WS-VIS-04: los límites del zoom son 1.0–2.5 con paso 1.2 (spec §4).</summary>
    [TestCase]
    public void LimitesDeZoomSonUnoACincoConPasoUnoDos()
    {
        AssertFloat(WordSearchVisuals.MinZoom).IsEqual(1.0f);
        AssertFloat(WordSearchVisuals.MaxZoom).IsEqual(2.5f);
        AssertFloat(WordSearchVisuals.ZoomStep).IsEqual(1.2f);
    }

    /// <summary>WS-VIS-05: el confeti usa partículas cuadradas de pixel art.</summary>
    [TestCase]
    public void ConfetiEsCuadradoPixelArtSinSuavizado()
    {
        AssertInt(PixelConfettiEmitter.PixelSize).IsGreaterEqual(4);
        AssertFloat(PixelConfettiEmitter.DefaultLifetime).IsGreater(0.0f);
        AssertInt(PixelConfettiEmitter.DefaultAmount).IsGreater(0);
    }
}