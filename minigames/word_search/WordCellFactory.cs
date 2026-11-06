using Godot;

namespace ClasicosMenteActiva.minigames.word_search;

/// <summary>
///     Factory para instanciar celdas del tablero (patrón Factory · Single Responsibility).
///     Centraliza la creación de <see cref="WordCell" /> para que el tablero no dependa
///     de la construcción directa ni de detalles de configuración de fuente y estilo.
/// </summary>
public static class WordCellFactory
{
    /// <summary>Crea y configura una celda lista para añadirse a la escena.</summary>
    public static WordCell Create(Vector2I coords, char letter, int fontSize)
    {
        var cell = new WordCell();
        cell.Setup(coords, letter, fontSize);
        return cell;
    }
}