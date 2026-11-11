using Godot;

/// <summary>
///     Pinta el bisel duro compartido por botones y celdas pixel art.
///     El relleno y el borde exterior de 2 px los dibuja <see cref="StyleBoxFlat" />;
///     este pintor añade la luz y la sombra interiores que dan el relieve 3D.
///     Las celdas usan un bisel asimétrico (1 px de luz arriba/izquierda,
///     2 px de sombra abajo/derecha) para lograr el efecto de plantilla
///     (<c>docs/word_search/wathersoup.jpg</c>). Los botones heredan el bisel
///     simétrico clásico de 1 px. Todos los trazos usan coordenadas enteras
///     y <c>AntiAliasing = false</c> (docs/art_direction.md §1 y §3).
/// </summary>
public static class PixelReliefPainter
{
    /// <summary>Grosor del borde exterior de los componentes pixel art.</summary>
    public const int BorderWidth = 2;

    /// <summary>
    ///     Dibuja el bisel interior. En estado elevado la luz va arriba/izquierda
    ///     y la sombra abajo/derecha; en estado hundido se invierten ambos.
    /// </summary>
    /// <param name="canvas">Nodo que dibuja (botón, celda o panel).</param>
    /// <param name="size">Tamaño actual del nodo en píxeles.</param>
    /// <param name="raised">True para relieve elevado; false para relieve hundido.</param>
    /// <param name="highlightWidth">
    ///     Ancho en px de la banda de luz (arriba/izquierda). Por defecto 1 px.
    ///     Las celdas del tablero usan 1 px; los botones también 1 px.
    /// </param>
    /// <param name="shadowWidth">
    ///     Ancho en px de la banda de sombra (abajo/derecha). Por defecto 1 px.
    ///     Las celdas del tablero usan 2 px para igualar la plantilla de diseño.
    /// </param>
    public static void Draw(CanvasItem canvas, Vector2 size, bool raised,
        int highlightWidth = 1, int shadowWidth = 1)
    {
        var width = Mathf.RoundToInt(size.X);
        var height = Mathf.RoundToInt(size.Y);
        var innerWidth = width - BorderWidth * 2;
        var innerHeight = height - BorderWidth * 2;
        if (innerWidth <= 0 || innerHeight <= 0) return;
        if (highlightWidth < 0 || shadowWidth < 0) return;

        // En estado elevado: luz (hw px) arriba/izquierda y sombra (sw px)
        // abajo/derecha. En estado hundido se invierten COLORES y GROSORES: la
        // sombra ancha pasa a arriba/izquierda y la luz fina a abajo/derecha,
        // como en una tecla mecánica presionada.
        var light = raised ? PixelStyles.BiselClaro : PixelStyles.BiselOscuro;
        var dark = raised ? PixelStyles.BiselOscuro : PixelStyles.BiselClaro;
        var lightW = Mathf.Min(raised ? highlightWidth : shadowWidth, innerWidth);
        var darkW = Mathf.Min(raised ? shadowWidth : highlightWidth, innerHeight);
        var b = BorderWidth;

        // Bandas claras arriba/izquierda; se acortan donde empieza la sombra
        // para no pisar las esquinas opuestas.
        var lightTopW = innerWidth - darkW;
        if (lightTopW > 0)
            canvas.DrawRect(new Rect2(b, b, lightTopW, lightW), light);
        var lightLeftH = innerHeight - darkW;
        if (lightLeftH > 0)
            canvas.DrawRect(new Rect2(b, b, lightW, lightLeftH), light);

        // Bandas oscuras abajo/derecha; cubren las esquinas restantes al
        // extenderse por todo el ancho/alto interior.
        canvas.DrawRect(new Rect2(b, height - b - darkW, innerWidth, darkW), dark);
        canvas.DrawRect(new Rect2(width - b - darkW, b, darkW, innerHeight), dark);
    }
}
