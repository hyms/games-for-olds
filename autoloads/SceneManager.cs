using Godot;

/// <summary>
/// Autoload: gestiona los cambios de escena y crea los servicios compartidos
/// (SaveManager, ThemeManager, AudioManager, AdManager).
/// Se registra como <c>SceneManager</c> en Project Settings → Globals → Autoloads.
/// Equivalente C# de <c>autoloads/SceneManager.gd</c>.
/// </summary>
public partial class SceneManager : Node
{
    /// <summary>Se emite antes de iniciar el cambio de escena.</summary>
    [Signal]
    public delegate void SceneChangingEventHandler(string fromPath, string toPath);

    /// <summary>Se emite cuando la nueva escena ya está cargada.</summary>
    [Signal]
    public delegate void SceneChangedEventHandler(string toPath);

    /// <summary>Ruta de la escena actual (vacía si aún no hay ninguna).</summary>
    public string CurrentScenePath { get; private set; } = string.Empty;

    /// <summary>Servicio de persistencia y estadísticas.</summary>
    public SaveManager Save { get; private set; }

    /// <summary>Servicio de temas y accesibilidad visual.</summary>
    public ThemeManager Theme { get; private set; }

    /// <summary>Servicio de audio.</summary>
    public AudioManager Audio { get; private set; }

    /// <summary>Servicio de publicidad (no-op en MVP).</summary>
    public AdManager Ads { get; private set; }

    public override void _Ready()
    {
        Node current = GetTree().CurrentScene;
        if (GodotObject.IsInstanceValid(current) && !string.IsNullOrEmpty(current.SceneFilePath))
        {
            CurrentScenePath = current.SceneFilePath;
        }

        CreateServices();
    }

    /// <summary>Crea los servicios compartidos una sola vez como hijos de SceneManager.</summary>
    private void CreateServices()
    {
        Save = GetNodeOrNull<SaveManager>("SaveManager") ?? new SaveManager { Name = "SaveManager" };
        Theme = GetNodeOrNull<ThemeManager>("ThemeManager") ?? new ThemeManager { Name = "ThemeManager" };
        Audio = GetNodeOrNull<AudioManager>("AudioManager") ?? new AudioManager { Name = "AudioManager" };
        Ads = GetNodeOrNull<AdManager>("AdManager") ?? new AdManager { Name = "AdManager" };

        if (!GodotObject.IsInstanceValid(Save.GetParent()))
        {
            AddChild(Save);
        }
        if (!GodotObject.IsInstanceValid(Theme.GetParent()))
        {
            AddChild(Theme);
        }
        if (!GodotObject.IsInstanceValid(Audio.GetParent()))
        {
            AddChild(Audio);
        }
        if (!GodotObject.IsInstanceValid(Ads.GetParent()))
        {
            AddChild(Ads);
        }

        GD.Print("[SceneManager] servicios core creados: SaveManager, ThemeManager, AudioManager, AdManager");
    }

    /// <summary>Cambia a otra escena por ruta <c>res://…</c>.</summary>
    public async void GoTo(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            GD.PushWarning("[SceneManager] Ruta vacía. Ignorando cambio de escena.");
            return;
        }

        if (CurrentScenePath == path)
        {
            GD.PushWarning($"[SceneManager] Ya estás en la misma escena: {path}");
            return;
        }

        SceneTree tree = GetTree();
        EmitSignal(SignalName.SceneChanging, CurrentScenePath, path);
        await ToSignal(tree, SceneTree.SignalName.ProcessFrame); // respiro para los listeners

        Error error = tree.ChangeSceneToFile(path);
        if (error != Error.Ok)
        {
            GD.PushError($"[SceneManager] Error al cargar escena '{path}' (code: {(int)error})");
            return;
        }

        CurrentScenePath = path;
        EmitSignal(SignalName.SceneChanged, CurrentScenePath);
    }

    /// <summary>Reinicia la escena actual.</summary>
    public void ReloadCurrent()
    {
        if (string.IsNullOrEmpty(CurrentScenePath))
        {
            return;
        }

        GoTo(CurrentScenePath);
    }

    /// <summary>Vuelve al menú principal.</summary>
    public void GoToMainMenu() => GoTo(Constants.SceneMainMenu);
}
