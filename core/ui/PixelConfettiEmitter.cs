using Godot;

namespace ClasicosMenteActiva.core.ui;

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
        ParticleTexture = GetPixelTexture();
        ScaleMin = new Vector2(PixelSize, PixelSize);
        ScaleMax = new Vector2(PixelSize, PixelSize);

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
    ///     Construye la rampa de colores del confeti con los tokens de la paleta
    ///     global: <see cref="PixelStyles.Turquesa" />, <see cref="PixelStyles.Amarillo" />,
    ///     <see cref="PixelStyles.Cian" /> y <see cref="PixelStyles.CelebrationRed" />.
    ///     No se introducen colores fuera de la matriz global (docs/art_direction.md §4).
    /// </summary>
    private static Gradient BuildColorRamp()
    {
        var ramp = new Gradient();
        ramp.SetColor(0, PixelStyles.Turquesa);
        ramp.SetColor(1, PixelStyles.Amarillo);
        ramp.SetColor(2, PixelStyles.Cian);
        ramp.SetColor(3, PixelStyles.CelebrationRed);
        return ramp;
    }

    /// <summary>
    ///     Crea (una sola vez) la textura 1×1 blanca con filtro Nearest. Se cachea
    ///     estáticamente porque es idéntica para todos los emisores y no cambia.
    /// </summary>
    private static Texture2D GetPixelTexture()
    {
        if (_pixelTexture != null) return _pixelTexture;

        var image = Image.Create(1, 1, false, Image.Format.Rgba8);
        image.SetPixel(0, 0, Colors.White);

        _pixelTexture = ImageTexture.CreateFromImage(image);
        _pixelTexture.Filter = Texture.FilterEnum.Nearest;
        return _pixelTexture;
    }
}