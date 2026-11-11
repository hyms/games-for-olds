/// <summary>
///     Estados visuales de una celda pixel art reutilizable (Sopa de Letras,
///     Tangram y grillas de lógica). Vive en <c>core/ui</c> para que la fábrica de
///     estilos y los componentes de juego compartan un único enum.
/// </summary>
public enum PixelCellState
{
    Idle,
    Pressed,
    Selected,
    Found,
    Hint
}
