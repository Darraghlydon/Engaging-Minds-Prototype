using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public LevelState CurrentLevelState { get; private set; } = LevelState.Boot;
    public MenuScreen CurrentMenuState { get; private set; } = MenuScreen.None;

    public bool IsPaused { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsInDialogue { get; private set; }

    public bool PlayerInputBlocked => IsPaused || IsLoading || IsInDialogue || CurrentMenuState != MenuScreen.None;
    private bool IsAnyMenuOpen => CurrentMenuState != MenuScreen.None;
    private bool _persistentUiLoaded;

    private MenuScreen previousMenuState;

    [SerializeField] private SceneRegistrySO sceneRegistry;

    //Input (Pause)
    [SerializeField] private InputActionAsset inputActions;
    private InputAction _pauseAction;

    // Prevent overlapping  transitions
    private bool _isSessionTransitioning;
    private SceneLoadManager sceneLoadManager;

    //Events
    public event Action<LevelState> LevelStateChanged;
    public event Action<LevelState> LevelSceneReady;
    public event Action<MenuScreen> MenuStateChanged;
    public event Action<RunState> RunStateChanged;
    private RunState _lastRunState;

    public static Action ResetGameSession;

    public event Action<bool> PauseChanged;
    public event Action<bool> LoadingChanged;
    public event Action<bool> DialogueChanged;

    public RunState CurrentRunState
    {
        get
        {
            if (IsLoading) return RunState.Loading;
            if (IsPaused) return RunState.Paused;
            if (IsInDialogue) return RunState.Dialogue;
            if (CurrentMenuState != MenuScreen.None) return RunState.Menu;
            return RunState.Playing;
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sceneLoadManager = gameObject.AddComponent<SceneLoadManager>();
        sceneLoadManager.sceneRegistry = sceneRegistry;

        if (inputActions == null)
        {
            Debug.LogError("GameManager: inputActions not assigned. Pause input will be disabled.");
        }
        else
        {
            var map = inputActions.FindActionMap("MainGame", false);
            if (map == null)
            {
                Debug.LogWarning("GameManager: 'MainGame' action map not found on InputActionAsset.");
            }
            else
            {
                _pauseAction = map.FindAction("Menu", false);
                if (_pauseAction == null)
                {
                    Debug.LogWarning("GameManager: 'Menu' action not found on 'MainGame' map.");
                }
            }
        }
    }

    private void OnEnable()
    {
        LevelSceneReady += CheckLevelState;
    }
    private void OnDisable()
    {
        LevelSceneReady -= CheckLevelState;
    }

    private void CheckLevelState(LevelState state)
    {
        if (CurrentLevelState == LevelState.Office)
        {
            DialogManager.Instance.TryTriggerGameOverIfInOffice();
        }
    }

    private void Start()
    {
        _lastRunState = CurrentRunState;

        SetMenuState(MenuScreen.MainMenuRoot);
        SetLevelState(LevelState.Office);
    }

    private void Update()
    {
        if (_pauseAction == null) return;
        if (!_pauseAction.WasPerformedThisFrame()) return;

        if (IsLoading) return;

        if (CurrentLevelState == LevelState.Minigame1) return;

        if (IsPaused) ExitPause();
        else EnterPause();
    }

    #region Scene/Level Setting
    public void SetLevelState(LevelState targetState)
    {
        if (CurrentLevelState == targetState) return;
        if (_isSessionTransitioning) return;

        StartCoroutine(LevelLoadState(targetState));
    }

    private IEnumerator LoadOffice()
    {
        // unload minigames
        yield return StartCoroutine(sceneLoadManager.Unload(SceneId.MiniGame1));

        // load office
        yield return StartCoroutine(sceneLoadManager.Load(SceneId.Main, additive: true, setActive: true));
    }

    private IEnumerator LoadMinigame(SceneId minigameId)
    {
        // unload office
        yield return StartCoroutine(sceneLoadManager.Unload(SceneId.Main));

        // load requested minigame
        yield return StartCoroutine(sceneLoadManager.Load(minigameId, additive: true, setActive: true));
    }

    #endregion

    #region GameMode Setting
    private IEnumerator LevelLoadState(LevelState targetState)
    {
        _isSessionTransitioning = true;
        SetLoading(true);

        yield return EnsurePersistentUI();

        CurrentLevelState = targetState;
        LevelStateChanged?.Invoke(targetState);

        switch (targetState)
        {
            case LevelState.Office:
                yield return LoadOffice();
                break;
            case LevelState.Minigame1:
                yield return LoadMinigame(SceneId.MiniGame1);
                break;
            case LevelState.Quitting:
                QuitGame();
                break;
        }

        yield return null;

        SetLoading(false);
        LevelSceneReady?.Invoke(CurrentLevelState);
        _isSessionTransitioning = false;
    }

    private void SetLoading(bool loading)
    {
        IsLoading = loading;
        LoadingChanged?.Invoke(loading);
    }

    private IEnumerator EnsurePersistentUI()
    {
        if (_persistentUiLoaded) yield break;

        yield return StartCoroutine(sceneLoadManager.Load(SceneId.PersistentUI, true, false));

        _persistentUiLoaded = true;
    }

    private void NotifyRunStateMaybeChanged()
    {
        var next = CurrentRunState;
        if (next == _lastRunState) return;
        _lastRunState = next;
        RunStateChanged?.Invoke(next);
    }

    #endregion

    #region Menu Setting
    public void SetMenuState(MenuScreen menuState)
    {
        if (CurrentMenuState == menuState) return;

        if (IsLoading) return;

        if (IsInDialogue)
            ExitDialogue();

        previousMenuState = CurrentMenuState;
        CurrentMenuState = menuState;
        MenuStateChanged?.Invoke(CurrentMenuState);
        NotifyRunStateMaybeChanged();
    }

    #endregion

    #region Dialogue Setting
    public bool TryEnterDialogue()
    {
        if (IsLoading || IsAnyMenuOpen)
            return false;

        IsInDialogue = true;
        DialogueChanged?.Invoke(true);
        NotifyRunStateMaybeChanged();
        return true;
    }

    public void ExitDialogue()
    {
        if (!IsInDialogue) return;

        IsInDialogue = false;
        DialogueChanged?.Invoke(false);
        NotifyRunStateMaybeChanged();
    }

    #endregion

    #region Menu Options
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
        SetMenuState(MenuScreen.None);
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
        Debug.Log("Quitting");
    }

    public void ResetSession()
    {
        ResetGameSession?.Invoke();
        StartNewGameFlow();
    }

    internal void Resume()
    {
        ExitPause();
    }
    #endregion

    #region Utility Methods
    private void EnterPause()
    {
        if (IsLoading) return;
        if (IsAnyMenuOpen) return;

        IsPaused = true;
        Time.timeScale = 0f;
        PauseChanged?.Invoke(true);

        SetMenuState(MenuScreen.Pause);
    }

    private void ExitPause()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        PauseChanged?.Invoke(false);

        SetMenuState(MenuScreen.None);
    }

    #endregion
}

public enum LevelState
{
    Boot,
    Office,
    Minigame1,
    Quitting
}

public enum RunState { Loading, Paused, Dialogue, Menu, Playing }

public enum PlayState
{
    Playing,
    Paused
}

public enum MenuScreen
{
    None,
    MainMenuRoot,
    Character,
    Values,
    Pause,
    GameOverScreen
}