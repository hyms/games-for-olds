using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace ClasicosMenteActiva.core;

public partial class SaveManager : Node
{
    public const int SaveVersion = 1;
    public const string SaveFileName = "save_data.json";
    public const string BackupFileName = "save_data.json.bak";

    /// <summary>Id del nivel inicial por defecto (Fácil).</summary>
    public const string DefaultLevelId = Constants.DefaultDifficultyId;

    /// <summary>Victorias necesarias para desbloquear el siguiente nivel.</summary>
    public const int WinsToUnlockNextLevel = 20;

    public static readonly string[] KnownGames =
    {
        "word_search",
        "tangram",
        "blocks"
    };

    private string _backupPath;

    private string _savePath;

    public SaveData Data { get; private set; } = new() { Version = SaveVersion };

    public UserSettings Settings => Data.Settings;

    public override void _Ready()
    {
        _savePath = ProjectSettings.GlobalizePath("user://" + SaveFileName);
        _backupPath = ProjectSettings.GlobalizePath("user://" + BackupFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(_savePath));
        LoadData();
    }

    public void SaveProgress(string levelId, int completedGames)
    {
        var path = ProjectSettings.GlobalizePath("user://wordsearch_progress.json");
        var progress = new WordSearchProgressData { LevelId = levelId, Completed = completedGames };
        var json = JsonSerializer.Serialize(progress, AppJsonContext.Default.WordSearchProgressData);
        File.WriteAllText(path, json);
    }

    public int LoadProgress(string levelId)
    {
        var path = ProjectSettings.GlobalizePath("user://wordsearch_progress.json");
        if (!File.Exists(path)) return 0;
        try
        {
            var json = File.ReadAllText(path);
            var progress = JsonSerializer.Deserialize(json, AppJsonContext.Default.WordSearchProgressData);
            if (progress != null && progress.LevelId == levelId)
                return progress.Completed;
        }
        catch
        {
        }

        return 0;
    }

    public void LoadData()
    {
        Data = new SaveData { Version = SaveVersion };

        if (!File.Exists(_savePath))
        {
            Data.InitializeDefaults();
            SaveToFile();
            return;
        }

        try
        {
            var json = File.ReadAllText(_savePath);
            var loaded = JsonSerializer.Deserialize(json, AppJsonContext.Default.SaveData);
            if (loaded != null)
            {
                Data = loaded;
                Migrate(Data);
                Data.EnsureDefaults();
                GD.Print($"[SaveManager] cargado: v{Data.Version}, {Data.Stats.Count} juegos");
            }
            else
            {
                throw new InvalidDataException("JSON nulo");
            }
        }
        catch (Exception e)
        {
            GD.PushError($"[SaveManager] archivo corrupto, restaurando respaldo o por defecto: {e.Message}");
            RestoreFromBackupOrDefaults();
        }
    }

    public void SaveToFile()
    {
        if (Data == null) return;

        var tmpPath = _savePath + ".tmp";
        var json = JsonSerializer.Serialize(Data, AppJsonContext.Default.SaveData);

        try
        {
            if (File.Exists(_savePath)) File.Copy(_savePath, _backupPath, true);

            File.WriteAllText(tmpPath, json);
            File.Move(tmpPath, _savePath, true);
        }
        catch (Exception e)
        {
            GD.PushError($"[SaveManager] error al guardar: {e.Message}");
            if (File.Exists(tmpPath))
                try
                {
                    File.Delete(tmpPath);
                }
                catch
                {
                }
        }
    }

    public void RegisterResult(string gameId, string levelId, bool won, float timeSeconds, int score)
    {
        if (string.IsNullOrWhiteSpace(gameId)) return;

        Data.EnsureGameStats(gameId);
        var stats = Data.Stats[gameId];

        stats.Plays++;

        if (won)
        {
            stats.Wins++;

            // Desbloquear siguiente nivel si se alcanzan 20 victorias (Fase 7)
            UnlockNextLevelIfNeeded(gameId, levelId);

            if (stats.BestTimeSeconds == null || timeSeconds < stats.BestTimeSeconds.Value)
                stats.BestTimeSeconds = timeSeconds;

            if (score > stats.BestScore) stats.BestScore = score;

            stats.CurrentLevelId = NextLevelId(levelId) ?? stats.CurrentLevelId ?? levelId;
        }
        else
        {
            if (score > stats.BestScore) stats.BestScore = score;
        }

        SaveToFile();
    }

    public void ResetStats()
    {
        Data = new SaveData { Version = SaveVersion };
        Data.InitializeDefaults();
        SaveToFile();
    }

    public GameStats GetStats(string gameId)
    {
        GameStats stats = null;
        Data?.Stats.TryGetValue(gameId, out stats);
        return stats;
    }

    public void SaveSettings()
    {
        SaveToFile();
    }

    private void RestoreFromBackupOrDefaults()
    {
        if (File.Exists(_backupPath))
            try
            {
                var json = File.ReadAllText(_backupPath);
                var restored = JsonSerializer.Deserialize(json, AppJsonContext.Default.SaveData);
                if (restored != null)
                {
                    Data = restored;
                    Migrate(Data);
                    Data.EnsureDefaults();
                    SaveToFile();
                    return;
                }
            }
            catch
            {
            }

        Data = new SaveData { Version = SaveVersion };
        Data.InitializeDefaults();
        SaveToFile();
    }

    private static void Migrate(SaveData data)
    {
        if (data.Version >= SaveVersion) return;

        data.Version = SaveVersion;
        GD.Print($"[SaveManager] migrado a v{SaveVersion}");
    }

    private static string NextLevelId(string currentLevelId)
    {
        return currentLevelId switch
        {
            "facil" => "medio",
            "medio" => "dificil",
            "dificil" => "maestro",
            _ => null
        };
    }

    private void UnlockNextLevelIfNeeded(string gameId, string levelId)
    {
        if (!Data.Stats.TryGetValue(gameId, out var stats) || stats.Wins < WinsToUnlockNextLevel) return;

        var nextLevelId = NextLevelId(levelId);
        if (string.IsNullOrEmpty(nextLevelId)) return;

        stats.CurrentLevelId = nextLevelId;
        GD.Print($"[SaveLevelUnlock] Nivel {levelId} completado con "
                 + $"{WinsToUnlockNextLevel}+ victorias, desbloqueando {nextLevelId}");
    }
}

public class SaveData
{
    public int Version { get; set; }
    public UserSettings Settings { get; set; } = new();
    public Dictionary<string, GameStats> Stats { get; set; } = new();

    public void InitializeDefaults()
    {
        Settings = new UserSettings();
        Stats = new Dictionary<string, GameStats>();
        foreach (var gameId in SaveManager.KnownGames)
            Stats[gameId] = new GameStats { CurrentLevelId = SaveManager.DefaultLevelId };
    }

    public void EnsureDefaults()
    {
        if (Settings == null) Settings = new UserSettings();

        if (Stats == null) Stats = new Dictionary<string, GameStats>();

        foreach (var gameId in SaveManager.KnownGames)
            if (!Stats.ContainsKey(gameId))
                Stats[gameId] = new GameStats { CurrentLevelId = SaveManager.DefaultLevelId };
    }

    public void EnsureGameStats(string gameId)
    {
        if (!Stats.ContainsKey(gameId)) Stats[gameId] = new GameStats { CurrentLevelId = SaveManager.DefaultLevelId };
    }
}

public class UserSettings
{
    public string TextSize { get; set; } = "normal";
    public string Theme { get; set; } = "default";
    public bool AudioEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
}

public class GameStats
{
    public string CurrentLevelId { get; set; } = SaveManager.DefaultLevelId;
    public int Plays { get; set; }
    public int Wins { get; set; }
    public float? BestTimeSeconds { get; set; }
    public int BestScore { get; set; }
}