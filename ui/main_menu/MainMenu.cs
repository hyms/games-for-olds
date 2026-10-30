using Godot;

/// <summary>
/// Menú principal: punto de entrada. Navega con <see cref="SceneManager"/>
/// (desacoplado del resto de escenas).
/// Equivalente C# de <c>ui/main_menu/MainMenu.gd</c>.
/// </summary>
public partial class MainMenu : Control
{
    private SceneManager _sceneManager;
    private BigButton _btnWordSearch;
    private BigButton _btnTangram;
    private BigButton _btnBlocks;
    private BigButton _btnQuit;

    public override void _Ready()
    {
        _sceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");

        _btnWordSearch = GetNode<BigButton>("%BtnWordSearch");
        _btnTangram = GetNode<BigButton>("%BtnTangram");
        _btnBlocks = GetNode<BigButton>("%BtnBlocks");
        _btnQuit = GetNode<BigButton>("%BtnQuit");

        _btnWordSearch.Pressed += OnWordSearchPressed;
        _btnTangram.Pressed += OnTangramPressed;
        _btnBlocks.Pressed += OnBlocksPressed;
        _btnQuit.Pressed += OnQuitPressed;

        // Enfoque inicial accesible (teclado/mando).
        _btnWordSearch.GrabFocus();
    }

    private void OnWordSearchPressed()
    {
        // Provisional hasta la Fase 2 §2.3: el submenú real es el selector de dificultad.
        _sceneManager.GoTo(ComingSoon.ScenePath);
    }

    private void OnTangramPressed()
    {
        GD.Print("[MainMenu] Tangram/Puzles de encaje aún no implementado.");
    }

    private void OnBlocksPressed()
    {
        GD.Print("[MainMenu] Lógica de Bloques (estilo Dr. Mario) aún no implementado.");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
