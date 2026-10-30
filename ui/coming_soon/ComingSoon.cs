using Godot;

/// <summary>
/// Pantalla provisional ("en construcción") para entradas cuyo destino real
/// llega en una fase posterior. Hoy: Sopa de Letras → selector de dificultad
/// (plan Fase 2 §2.3). Se elimina cuando exista ese selector.
/// </summary>
public partial class ComingSoon : Control
{
    /// <summary>Ruta de esta escena (la usa el menú principal).</summary>
    public const string ScenePath = "res://ui/coming_soon/ComingSoon.tscn";

    private BigButton _btnBack;

    public override void _Ready()
    {
        _btnBack = GetNode<BigButton>("%BtnBack");
        _btnBack.Pressed += OnBackPressed;
        _btnBack.GrabFocus();
    }

    private void OnBackPressed()
    {
        GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}").GoToMainMenu();
    }
}
