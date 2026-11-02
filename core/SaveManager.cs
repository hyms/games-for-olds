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

    public static readonly string[] KnownGames =
    {
        "word_search",
        "tangram",
        "blocks",
    };

    /// <summary>Id del nivel inicial por defecto (Fácil).</summary>
    public const string DefaultLevelId = Constants.DefaultDifficultyId;

    private string _savePath;
    private string _backupPath;
    private SaveData _data = new() { Version = SaveVersion };

    public SaveData Data => _data;

    public override void _Ready()
    {
        _savePath = ProjectSettings.GlobalizePath("user://" + SaveFileName);
        _backupPath = ProjectSettings.GlobalizePath("user://" + BackupFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(_savePath));
        LoadData();
    }

    public void SaveProgress(string levelId, int completedGames)
    {
        string path = ProjectSettings.GlobalizePath("user://wordsearch_progress.json");
        var progress = new WordSearchProgressData { LevelId = levelId, Completed = completedGames };
        string json = JsonSerializer.Serialize(progress, AppJsonContext.Default.WordSearchProgressData);
        File.WriteAllText(path, json);
    }

    public int LoadProgress(string levelId)
    {
        string path = ProjectSettings.GlobalizePath("user://wordsearch_progress.json");
        if (!File.Exists(path)) return 0;
        try
        {
            string json = File.ReadAllText(path);
            var progress = JsonSerializer.Deserialize<WordSearchProgressData>(json, AppJsonContext.Default.WordSearchProgressData);
            if (progress != null && progress.LevelId == levelId)
                return progress.Completed;
        }
        catch { }
        return 0;
    }

    public void LoadData()
    {
        _data = new SaveData { Version = SaveVersion };

        if (!File.Exists(_savePath))
        {
            _data.InitializeDefaults();
            SaveToFile();
            return;
        }

        try
        {
            string json = File.ReadAllText(_savePath);
            var loaded = JsonSerializer.Deserialize(json, AppJsonContext.Default.SaveData) as SaveData;
            if (loaded != null)
            {
                _data = loaded;
                Migrate(_data);
                _data.EnsureDefaults();
                GD.Print($"[SaveManager] cargado: v{_data.Version}, {_data.Stats.Count} juegos");
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
        if (_data == null)
        {
            return;
        }

        string tmpPath = _savePath + ".tmp";
        string json = JsonSerializer.Serialize(_data, AppJsonContext.Default.SaveData);

        try
        {
            if (File.Exists(_savePath))
            {
                File.Copy(_savePath, _backupPath, overwrite: true);
            }

            File.WriteAllText(tmpPath, json);
            File.Move(tmpPath, _savePath, overwrite: true);
        }
        catch (Exception e)
        {
            GD.PushError($"[SaveManager] error al guardar: {e.Message}");
            if (File.Exists(tmpPath))
            {
                try { File.Delete(tmpPath); } catch { }
            }
        }
    }

    public void RegisterResult(string gameId, string levelId, bool won, float timeSeconds, int score)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            return;
        }

        _data.EnsureGameStats(gameId);
        GameStats stats = _data.Stats[gameId];

        stats.Plays++;

        if (won)
        {
            stats.Wins++;

            if (stats.BestTimeSeconds == null || timeSeconds < stats.BestTimeSeconds.Value)
            {
                stats.BestTimeSeconds = timeSeconds;
            }

            if (score > stats.BestScore)
            {
                stats.BestScore = score;
            }

            if (won)
            {
                stats.CurrentLevelId = NextLevelId(levelId) ?? stats.CurrentLevelId ?? levelId;
            }
        }

        SaveToFile();
    }

    public void ResetStats()
    {
        _data = new SaveData { Version = SaveVersion };
        _data.InitializeDefaults();
        SaveToFile();
    }

    public GameStats GetStats(string gameId)
    {
        GameStats stats = null;
        _data?.Stats.TryGetValue(gameId, out stats);
        return stats;
    }

    public UserSettings Settings => _data.Settings;

    public void SaveSettings() => SaveToFile();

    private void RestoreFromBackupOrDefaults()
    {
        if (File.Exists(_backupPath))
        {
            try
            {
                string json = File.ReadAllText(_backupPath);
                var restored = JsonSerializer.Deserialize(json, AppJsonContext.Default.SaveData) as SaveData;
                if (restored != null)
                {
                    _data = restored;
                    Migrate(_data);
                    _data.EnsureDefaults();
                    SaveToFile();
                    return;
                }
            }
            catch
            {
            }
        }

        _data = new SaveData { Version = SaveVersion };
        _data.InitializeDefaults();
        SaveToFile();
    }

    private static void Migrate(SaveData data)
    {
        if (data.Version >= SaveVersion)
        {
            return;
        }

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
            _ => null,
        };
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
        foreach (string gameId in SaveManager.KnownGames)
        {
            Stats[gameId] = new GameStats { CurrentLevelId = SaveManager.DefaultLevelId };
        }
    }

    public void EnsureDefaults()
    {
        if (Settings == null)
        {
            Settings = new UserSettings();
        }

        if (Stats == null)
        {
            Stats = new Dictionary<string, GameStats>();
        }

        foreach (string gameId in SaveManager.KnownGames)
        {
            if (!Stats.ContainsKey(gameId))
            {
                Stats[gameId] = new GameStats { CurrentLevelId = SaveManager.DefaultLevelId };
            }
        }
    }

    public void EnsureGameStats(string gameId)
    {
        if (!Stats.ContainsKey(gameId))
        {
            Stats[gameId] = new GameStats { CurrentLevelId = SaveManager.DefaultLevelId };
        }
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