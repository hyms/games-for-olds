using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClasicosMenteActiva.core;

/// <summary>
///     Contexto de serialización JSON para tipos del juego.
///     Usar este contexto en lugar de JsonSerializerOptions elimina los warnings
///     IL3050 (RequiresDynamicCodeAttribute) al compilar con Native AOT para Android.
/// </summary>
[JsonSerializable(typeof(SaveData))]
[JsonSerializable(typeof(UserSettings))]
[JsonSerializable(typeof(GameStats))]
[JsonSerializable(typeof(Dictionary<string, GameStats>))]
[JsonSerializable(typeof(WordSearchProgressData))]
public partial class AppJsonContext : JsonSerializerContext
{
}

/// <summary>
///     Tipo POCO para el progreso de WordSearch (evita Godot.Collections.Dictionary
///     que no es compatible con Native AOT / trimming).
/// </summary>
public class WordSearchProgressData
{
    public string LevelId { get; set; } = "";
    public int Completed { get; set; }
}