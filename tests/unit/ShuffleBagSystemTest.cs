using System.Collections.Generic;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// UT-WS-06 — ShuffleBagSystem (spec §4.3 y §5 · test.md §2.2).
/// En 20 partidas cada uno de los 10 grupos temáticos aparece exactamente
/// 2 veces; la bolsa solo se rebaraja al agotarse; el estado es serializable
/// para SaveManager (Fase 3 global).
/// Equivalente C# de <c>tests/unit/ShuffleBagSystemTest.gd</c>.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class ShuffleBagSystemTest
{
    private static readonly string[] GroupIds =
    {
        "animales", "plantas", "frutas_verduras", "objetos", "profesiones",
        "paises_ciudades", "comidas", "instrumentos", "herramientas", "ropa",
    };

    [TestCase]
    public void En20PartidasCadaGrupoApareceDosVeces()
    {
        var bag = new ShuffleBagSystem(GroupIds, 42);
        var counts = new Dictionary<string, int>();
        for (int i = 0; i < 20; i++)
        {
            string group = bag.NextGroup();
            counts[group] = counts.GetValueOrDefault(group) + 1;
        }

        AssertInt(counts.Count).IsEqual(GroupIds.Length);
        foreach (string id in GroupIds)
        {
            int appearances = counts.GetValueOrDefault(id);
            AssertInt(appearances)
                .OverrideFailureMessage($"grupo {id}: {appearances} apariciones en 20 partidas (mínimo 2)")
                .IsEqual(2);
        }
    }

    [TestCase]
    public void CuarentaPartidasDanCuatroAparicionesPorGrupo()
    {
        // Dos bolsas completas: 2 copias × 2 vueltas = 4 apariciones por grupo.
        // (Dentro de una vuelta el mismo grupo puede repetirse: la spec §5 solo
        // garantiza las 2 apariciones por bolsa de 20.)
        var bag = new ShuffleBagSystem(GroupIds, 7);
        var counts = new Dictionary<string, int>();
        for (int i = 0; i < 40; i++)
        {
            string group = bag.NextGroup();
            counts[group] = counts.GetValueOrDefault(group) + 1;
        }

        AssertInt(bag.RefillCount).IsEqual(2); // inicial + 1 rebaraja (la 2.ª llega en la 41.ª)
        foreach (string id in GroupIds)
        {
            AssertInt(counts.GetValueOrDefault(id))
                .OverrideFailureMessage($"grupo {id}: {counts.GetValueOrDefault(id)} apariciones en 40 partidas")
                .IsEqual(4);
        }
    }

    [TestCase]
    public void LaBolsaSeRebarajaSoloAlAgotarse()
    {
        var bag = new ShuffleBagSystem(GroupIds, 3);
        AssertInt(bag.RefillCount).IsEqual(1); // la bolsa inicial
        for (int i = 0; i < 18; i++)
        {
            bag.NextGroup();
        }

        AssertInt(bag.RefillCount).IsEqual(1);
        AssertInt(bag.Remaining()).IsEqual(2);
        bag.NextGroup(); // 19.ª saca
        AssertInt(bag.Remaining()).IsEqual(1);
        bag.NextGroup(); // 20.ª saca: agota la bolsa sin rebarajar
        AssertInt(bag.RefillCount).IsEqual(1);
        AssertInt(bag.Remaining()).IsEqual(0);
        bag.NextGroup(); // 21.ª: aquí sí se rebaraja
        AssertInt(bag.RefillCount).IsEqual(2);
        AssertInt(bag.Remaining()).IsEqual(19);
    }

    [TestCase]
    public void MismaSemillaDaMismaSecuencia()
    {
        var a = new ShuffleBagSystem(GroupIds, 1234);
        var b = new ShuffleBagSystem(GroupIds, 1234);
        for (int i = 0; i < 20; i++)
        {
            AssertString(a.NextGroup()).IsEqual(b.NextGroup());
        }
    }

    [TestCase]
    public void EstadoSerializableRestauraLaMismaBolsa()
    {
        var bag = new ShuffleBagSystem(GroupIds, 99);
        for (int i = 0; i < 7; i++)
        {
            bag.NextGroup();
        }

        ShuffleBagSystem restored = ShuffleBagSystem.FromDictionary(bag.ToDictionary());
        AssertInt(restored.Remaining()).IsEqual(bag.Remaining());
        AssertInt(restored.RefillCount).IsEqual(bag.RefillCount);

        // La secuencia continúa idéntica: se restaura también el estado del RNG.
        for (int i = 0; i < 10; i++)
        {
            AssertString(restored.NextGroup()).IsEqual(bag.NextGroup());
        }
    }
}
