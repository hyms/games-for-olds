using Godot;

namespace ClasicosMenteActiva.core.ui;

/// <summary>
///     Extension methods for Godot 4 compatibility.
/// </summary>
internal static class GodotExtensions
{
    /// <summary>Gets the center of a Vector2 (Size / 2).</summary>
    public static Vector2 Center(this Vector2 self) => self / 2.0f;
}

/// <summary>
///     Emisor de confeti píxel basado en <see cref="CpuParticles2D" />. VIVE en
///     <c>core/ui/</c> porque es un componente base reutilizable (docs/art_direction.md
///     §5): cualquier minijuego que necesite partículas de pixel art la instanciará.
///     Los colores salen de <see cref="PixelStyles" /> (paleta global inmutable,
///     docs/art_direction.md §4); el emisor no hard-codea hex.
/// </summary>
public partial class PixelConfettiEmitter : CpuParticles2D
{
    /// <summary>Tamaño lógico de cada partícula cuadrada (4×4 o 6×6 px, sin suavizado).</summary>
    public const int PixelSize = 6;

    /// <summary>Duración de vida de cada partícula en segundos.</summary>
    public const float DefaultLifetime = 0.8f;

    /// <summary>Cantidad de partículas por estallido.</summary>
    public const int DefaultAmount = 40;

    /// <summary>
    ///     Textura de partícula blanca 1×1 con filtro Nearest (sin mipmaps, sin AA).
    ///     Escalada por <see cref="ScaleMin" />/<see cref="ScaleMax" /> a
    ///     <see cref="PixelSize" /> produce cuadrados duros de pixel art puro.
    /// </summary>
    private static Texture2D _pixelTexture;

    public override void _Ready()
    {
        base._Ready();

        // Partículas cuadradas de pixel art: textura 1×1 blanca, filtro Nearest,
        // escalada al tamaño de pixel. Sin anti-aliasing ni mipmaps (regla de oro).
        Texture = GetPixelTexture();
        // En Godot 4, ScaleAmountMin/Max controlan la escala de las partículas (float).
        ScaleAmountMin = PixelSize;
        ScaleAmountMax = PixelSize;
        // Filtro Nearest en el CanvasItem (hereda de Node2D) para pixel art nítido.
        TextureFilter = TextureFilterEnum.Nearest;

        Lifetime = DefaultLifetime;
        Amount = DefaultAmount;
        OneShot = true;
        Explosiveness = 1.0f;
        SpeedScale = 1.0f;
        Gravity = new Vector2(0, 30);
        ColorRamp = BuildColorRamp();
        Emitting = false;
    }

    /// <summary>
    ///     Dispara un estallido de confeti en la posición local indicada.
    ///     Las coordenadas son locales al emisor (el llamador debe convertir
    ///     <see cref="Vector2.GlobalPosition" /> si es necesario).
    /// </summary>
    public void EmitAt(Vector2 localPosition, int? amount = null)
    {
        Position = localPosition;
        if (amount.HasValue) Amount = amount.Value;
        Restart();
    }

    /// <summary>
    ///     Construye la rampa de colores del confeti con los tokens cálidos de la
    ///     paleta de Word Search: <see cref="PixelStyles.WSAcento" /> (coral),
    ///     <see cref="PixelStyles.CelebrationRed" />, <see cref="PixelStyles.WSCrema" />
    ///     y <see cref="PixelStyles.Amarillo" />. Todos provienen de la paleta
    ///     aprobada (docs/art_direction.md §8.1); no se introducen colores fuera
    ///     de la matriz global ni acentos fríos.
    /// </summary>
    private static Gradient BuildColorRamp()
    {
        var ramp = new Gradient();
        ramp.AddPoint(0f, PixelStyles.WSAcento);
        ramp.AddPoint(1f / 3f, PixelStyles.CelebrationRed);
        ramp.AddPoint(2f / 3f, PixelStyles.WSCrema);
        ramp.AddPoint(1f, PixelStyles.Amarillo);
        return ramp;
    }

    /// <summary>
    ///     Crea (una sola vez) la textura 1×1 blanca con filtro Nearest. Se cachea
    ///     estáticamente porque es idéntica para todos los emisores y no cambia.
    /// </summary>
    private static Texture2D GetPixelTexture()
    {
        if (_pixelTexture != null) return _pixelTexture;

        var image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        image.SetPixel(0, 0, Colors.White);

        _pixelTexture = ImageTexture.CreateFromImage(image);
        return _pixelTexture;
    }
}