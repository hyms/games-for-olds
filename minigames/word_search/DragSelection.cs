using Godot;

/// <summary>
/// Lógica pura de la selección por arrastre (plan Fase 3 §3.2): inicio,
/// bloqueo de dirección, ajuste a la línea dominante y ruta recta final.
/// No conoce nodos ni píxeles, así que se prueba sin escenas.
/// </summary>
public sealed class DragSelection
{
    public bool IsActive { get; private set; }

    public Vector2I Start { get; private set; }

    /// <summary>Dirección bloqueada; <see cref="Vector2I.Zero"/> mientras no se haya fijado.</summary>
    public Vector2I Direction { get; private set; }

    /// <summary>Celda final de la ruta recta (igual a <see cref="Start"/> si no hay pasos).</summary>
    public Vector2I End { get; private set; }

    public bool IsDirectionLocked => Direction != Vector2I.Zero;

    /// <summary>Número de celdas de la selección (1 = una sola celda, inválida).</summary>
    public int Length => WordPath.Steps(Start, End) + 1;

    /// <summary>Empieza una selección en <paramref name="cell"/>.</summary>
    public bool Begin(Vector2I cell)
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        Start = cell;
        End = cell;
        Direction = Vector2I.Zero;
        return true;
    }

    /// <summary>
    /// Actualiza la selección con la celda bajo el dedo. La dirección se fija
    /// con el primer desplazamiento y ya no cambia (evita el zigzag).
    /// </summary>
    public bool Update(Vector2I cell, int rows, int columns)
    {
        if (!IsActive)
        {
            return false;
        }

        Vector2I delta = cell - Start;
        if (delta != Vector2I.Zero && !IsDirectionLocked)
        {
            Direction = SnapDirection(delta);
        }

        if (!IsDirectionLocked)
        {
            End = Start;
            return true;
        }

        int steps = Project(delta, Direction);
        steps = Mathf.Clamp(steps, 0, MaxSteps(Start, Direction, rows, columns));
        End = Start + (Direction * steps);
        return true;
    }

    /// <summary>Termina la selección; solo es válida si hay dirección y 2+ celdas.</summary>
    public bool TryComplete(out Vector2I start, out Vector2I end)
    {
        start = Start;
        end = End;
        bool valid = IsActive && IsDirectionLocked && End != Start;
        IsActive = false;
        Direction = Vector2I.Zero;
        return valid;
    }

    /// <summary>Cancela sin emitir selección (dedo fuera del tablero).</summary>
    public void Cancel()
    {
        IsActive = false;
        Direction = Vector2I.Zero;
        End = Start;
    }

    /// <summary>
    /// Ajusta un desplazamiento a la dirección dominante de la matriz: los ejes
    /// absorben desviaciones de hasta ~26° y las diagonales ocupan el resto.
    /// </summary>
    public static Vector2I SnapDirection(Vector2I delta)
    {
        if (delta == Vector2I.Zero)
        {
            return Vector2I.Zero;
        }

        int ax = Mathf.Abs(delta.X);
        int ay = Mathf.Abs(delta.Y);
        int signX = Mathf.Sign(delta.X);
        int signY = Mathf.Sign(delta.Y);

        if (ax >= 2 * ay)
        {
            return new Vector2I(signX, 0);
        }

        if (ay >= 2 * ax)
        {
            return new Vector2I(0, signY);
        }

        return new Vector2I(signX, signY);
    }

    /// <summary>Pasos hasta el borde del tablero en la dirección indicada.</summary>
    public static int MaxSteps(Vector2I start, Vector2I direction, int rows, int columns)
    {
        if (!WordPath.IsUnitDirection(direction))
        {
            return 0;
        }

        int stepsX = direction.X switch
        {
            > 0 => columns - 1 - start.X,
            < 0 => start.X,
            _ => int.MaxValue,
        };
        int stepsY = direction.Y switch
        {
            > 0 => rows - 1 - start.Y,
            < 0 => start.Y,
            _ => int.MaxValue,
        };

        return Mathf.Max(0, Mathf.Min(stepsX, stepsY));
    }

    /// <summary>
    /// Pasos del desplazamiento en una dirección unitaria. En los ejes es la
    /// componente correspondiente; en diagonal manda la componente dominante
    /// (la celda más cercana a la línea diagonal), no la suma de ambas.
    /// </summary>
    private static int Project(Vector2I delta, Vector2I direction)
    {
        if (direction == Vector2I.Zero)
        {
            return 0;
        }

        int stepsX = delta.X * direction.X;
        int stepsY = delta.Y * direction.Y;

        if (direction.X != 0 && direction.Y != 0)
        {
            return Mathf.Max(stepsX, stepsY);
        }

        return direction.X != 0 ? stepsX : stepsY;
    }
}
