using System.Collections.Generic;
using Godot;

/// <summary>
/// Geometría de la Sopa de Letras: direcciones unitarias, líneas rectas y
/// rutas entre celdas. La usan el generador, la entrada por arrastre y la
/// validación de palabras (plan Fase 2 §2.2 y Fase 3 §3.2).
/// </summary>
public static class WordPath
{
    /// <summary>Las 8 direcciones de la matriz completa (spec §4.2, nivel Maestro).</summary>
    public static readonly Vector2I[] AllDirections =
    {
        new(1, 0), new(1, 1), new(0, 1), new(-1, 1),
        new(-1, 0), new(-1, -1), new(0, -1), new(1, -1),
    };

    /// <summary>Solo horizontal y vertical, ambas hacia delante (nivel Fácil).</summary>
    public static readonly Vector2I[] OrthogonalForward =
    {
        new(1, 0), new(0, 1),
    };

    /// <summary>¿Es una de las 8 direcciones unitarias de la matriz?</summary>
    public static bool IsUnitDirection(Vector2I direction)
    {
        return Mathf.Abs(direction.X) <= 1
            && Mathf.Abs(direction.Y) <= 1
            && direction != Vector2I.Zero;
    }

    /// <summary>
    /// Dirección unitaria que va de <paramref name="start"/> a
    /// <paramref name="end"/>. Devuelve <see cref="Vector2I.Zero"/> si los
    /// puntos no forman una línea recta de la matriz (o son el mismo).
    /// </summary>
    public static Vector2I Direction(Vector2I start, Vector2I end)
    {
        Vector2I delta = end - start;
        if (delta == Vector2I.Zero)
        {
            return Vector2I.Zero;
        }

        if (delta.X == 0)
        {
            return new Vector2I(0, Mathf.Sign(delta.Y));
        }

        if (delta.Y == 0)
        {
            return new Vector2I(Mathf.Sign(delta.X), 0);
        }

        if (Mathf.Abs(delta.X) != Mathf.Abs(delta.Y))
        {
            return Vector2I.Zero;
        }

        return new Vector2I(Mathf.Sign(delta.X), Mathf.Sign(delta.Y));
    }

    /// <summary>Pasos desde el inicio hasta el final (0 si no están alineados).</summary>
    public static int Steps(Vector2I start, Vector2I end)
    {
        Vector2I direction = Direction(start, end);
        if (direction == Vector2I.Zero)
        {
            return 0;
        }

        Vector2I delta = end - start;
        return Mathf.Abs(delta.X) + Mathf.Abs(delta.Y) > 0
            ? Mathf.Max(Mathf.Abs(delta.X), Mathf.Abs(delta.Y))
            : 0;
    }

    /// <summary>
    /// Ruta recta completa de <paramref name="start"/> a
    /// <paramref name="end"/> (extremos incluidos). Vacía si no están alineados.
    /// </summary>
    public static List<Vector2I> Build(Vector2I start, Vector2I end)
    {
        var path = new List<Vector2I>();
        Vector2I direction = Direction(start, end);
        if (direction == Vector2I.Zero)
        {
            return path;
        }

        int steps = Steps(start, end);
        for (int step = 0; step <= steps; step++)
        {
            path.Add(start + (direction * step));
        }

        return path;
    }

    /// <summary>Ruta de una palabra ya colocada (inicio y dirección conocidos).</summary>
    public static List<Vector2I> Build(Vector2I start, Vector2I direction, int length)
    {
        var path = new List<Vector2I>(length);
        for (int step = 0; step < length; step++)
        {
            path.Add(start + (direction * step));
        }

        return path;
    }
}
