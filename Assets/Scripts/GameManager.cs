using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentGameState { get; private set; } = GameState.Boot;
    public MenuScreen CurrentMenuState { get; private set; } = MenuScreen.MainMenuRoot;
    public static Action ResetGameSession;

    private MenuScreen previousMenuState;

    public event Action<GameState> GameStateChanged;
    public event Action<MenuScreen> MenuStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SetGameState(GameState.MainUI);
    }

    public void SetGameState(GameState state)
    {
        if (CurrentGameState == state) return;

        CurrentGameState = state;
        GameStateChanged?.Invoke(state);

        switch (state)
        {
            case GameState.MainUI:
                SetMenuState(MenuScreen.MainMenuRoot);
                break;
            case GameState.Office:
                SetMenuState(MenuScreen.None);
                break;
            case GameState.Dialogue:
                SetMenuState(MenuScreen.None);
                break;
            case GameState.Minigame:
                Debug.Log("Start minigame scene");
                SceneLoadManager.Instance.LoadScene("Scenario2DevScene 1");
                StartCoroutine(ReturnToMainScene());
                break;
            case GameState.GameOver:
                SetMenuState(MenuScreen.MainMenuRoot);
                ResetSession();
                break;
            case GameState.Quitting:
                QuitGame();
                break;
            default:
                SetMenuState(MenuScreen.None);
                break;
        }
    }

    public void ResetSession()
    {
        ResetGameSession?.Invoke();
    }

    private IEnumerator ReturnToMainScene()
    {
        yield return new WaitForSeconds(2f);
        SceneLoadManager.Instance.LoadScene("Main Menu");
        DialogManager.Instance.OnMinigameEnded();
    }

    public void SetMenuState(MenuScreen menuState)
    {
        if (CurrentMenuState == menuState)
            return;

        previousMenuState = CurrentMenuState;
        CurrentMenuState = menuState;
        MenuStateChanged?.Invoke(CurrentMenuState);
    }

    internal void StartNewGameFlow()
    {
        SetMenuState(MenuScreen.Character);
    }

    internal void SubmitCharacter()
    {
        SetMenuState(MenuScreen.Values);
    }

    internal void SubmitValuesAndPlay()
    {
        SetGameState(GameState.Office);
    }

    internal void Resume()
    {
        //SetGameState(GameState.Playing);
    }

    internal void ReturnToPreviousMenu()
    {
        SetMenuState(previousMenuState);
    }

    internal void QuitToMainMenu()
    {
        SetMenuState(MenuScreen.MainMenuRoot);
    }

    internal void QuitGame()
    {
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (!UNITY_EDITOR)
                Application.Quit();
#endif
        Debug.Log("Qutting");
    }

    internal void StartMiniGame()
    {
        SetGameState(GameState.Minigame);
    }

}

public enum GameState
{
    Boot,
    MainUI,
    Dialogue,
    Office,
    Minigame,
    GameOver,
    Quitting
}

public enum MenuScreen
{
    None,
    MainMenuRoot,
    Character,
    Values,
    Pause
}