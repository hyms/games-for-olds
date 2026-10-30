using System.Collections.Generic;
using Godot;

/// <summary>
/// Bolsa de grupos temáticos (Shuffle Bag, spec §5 · prueba UT-WS-06).
/// La bolsa contiene cada grupo exactamente <see cref="CopiesPerGroup"/> veces y
/// se baraja al llenarla: en 20 partidas cada uno de los 10 grupos aparece 2 veces.
/// Solo se rebaraja al agotarse. Estado serializable con <see cref="ToDictionary"/>/
/// <see cref="FromDictionary"/> para <c>SaveManager</c> (Fase 3 global).
/// Equivalente C# de <c>minigames/word_search/ShuffleBagSystem.gd</c>.
/// </summary>
public partial class ShuffleBagSystem : RefCounted
{
    /// <summary>Copias de cada grupo por bolsa: 10 grupos × 2 = 20 partidas (spec §4.3).</summary>
    public const int CopiesPerGroup = 2;

    /// <summary>Versión del estado serializado (para migrar guardados futuros).</summary>
    public const int DictionaryVersion = 1;

    private readonly List<string> _groupIds = new();
    private readonly List<string> _bag = new();
    private readonly RandomNumberGenerator _rng = new();

    /// <summary>Veces que se ha rebarajado la bolsa (1 = la inicial). Para tests.</summary>
    public int RefillCount { get; set; }

    /// <summary>Crea una bolsa vacía (Godot necesita un constructor sin parámetros).</summary>
    public ShuffleBagSystem()
    {
    }

    /// <summary>
    /// Crea la bolsa con los grupos indicados y la llena barajada.
    /// <paramref name="seedValue"/> ≥ 0 fija la semilla (tests deterministas); -1 = aleatoria.
    /// </summary>
    public ShuffleBagSystem(IEnumerable<string> groupIds, long seedValue = -1)
    {
        _groupIds.AddRange(groupIds);
        if (seedValue >= 0)
        {
            _rng.Seed = (ulong)seedValue;
        }
        else
        {
            _rng.Randomize();
        }

        Refill();
    }

    /// <summary>Siguiente grupo a jugar. Agota la bolsa actual y, solo entonces, la rellena.</summary>
    public string NextGroup()
    {
        if (_groupIds.Count == 0)
        {
            GD.PushWarning("[ShuffleBag] sin grupos: no se puede sortear nada");
            return string.Empty;
        }

        if (_bag.Count == 0)
        {
            Refill();
        }

        int index = _rng.RandiRange(0, _bag.Count - 1);
        string group = _bag[index];
        _bag.RemoveAt(index);
        return group;
    }

    /// <summary>Grupos que quedan en la bolsa (0 = agotada, la siguiente sacada rebaraja).</summary>
    public int Remaining() => _bag.Count;

    /// <summary>Grupos conocidos por la bolsa (los que se reparten entre copias).</summary>
    public List<string> KnownGroups() => new(_groupIds);

    /// <summary>Serializa el estado completo para guardarlo (SaveManager, Fase 3).</summary>
    public Godot.Collections.Dictionary ToDictionary()
    {
        return new Godot.Collections.Dictionary
        {
            { "version", DictionaryVersion },
            { "group_ids", new Godot.Collections.Array<string>(_groupIds) },
            { "bag", new Godot.Collections.Array<string>(_bag) },
            { "refill_count", RefillCount },
            { "rng_state", _rng.State },
        };
    }

    /// <summary>Reconstruye la bolsa desde el estado guardado (continúa la misma secuencia).</summary>
    public static ShuffleBagSystem FromDictionary(Godot.Collections.Dictionary data)
    {
        int version = data.TryGetValue("version", out Variant versionValue) ? versionValue.AsInt32() : 0;
        if (version != DictionaryVersion)
        {
            GD.PushWarning($"[ShuffleBag] versión de estado desconocida: {version}");
        }

        var ids = new List<string>();
        if (data.TryGetValue("group_ids", out Variant idsValue))
        {
            foreach (Variant id in idsValue.AsGodotArray())
            {
                ids.Add(id.AsString());
            }
        }

        var bag = new ShuffleBagSystem(ids, 0);
        bag._bag.Clear();
        if (data.TryGetValue("bag", out Variant bagValue))
        {
            foreach (Variant id in bagValue.AsGodotArray())
            {
                bag._bag.Add(id.AsString());
            }
        }

        bag.RefillCount = data.TryGetValue("refill_count", out Variant refillValue) ? refillValue.AsInt32() : 0;
        if (data.TryGetValue("rng_state", out Variant stateValue))
        {
            bag._rng.State = stateValue.AsUInt64();
        }

        return bag;
    }

    /// <summary>
    /// Llena la bolsa con <see cref="CopiesPerGroup"/> copias de cada grupo y la
    /// baraja con el RNG propio (una semilla determinista da secuencias repetibles).
    /// </summary>
    private void Refill()
    {
        _bag.Clear();
        foreach (string id in _groupIds)
        {
            for (int copy = 0; copy < CopiesPerGroup; copy++)
            {
                _bag.Add(id);
            }
        }

        for (int i = _bag.Count - 1; i > 0; i--)
        {
            int j = _rng.RandiRange(0, i);
            (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
        }

        RefillCount++;
    }
}
