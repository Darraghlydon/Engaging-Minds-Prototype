using System;
using System.Collections.Generic;
using UnityEngine;


public class MainMenuManager : MonoBehaviour
{
    public List<UIMenuPanel> panels = new List<UIMenuPanel>();

    public MenuScreen CurrentState { get; private set; } = MenuScreen.MainMenuRoot;

    private Dictionary<MenuScreen, GameObject> _panelMap;

    private void OnEnable()
    {
        _panelMap = new Dictionary<MenuScreen, GameObject>();
        foreach (var p in panels)
        {
            if (p.panel) _panelMap[p.panelState] = p.panel;
            SetupPanelButtons(p);
        }
    }

    private void OnDisable()
    {
        foreach (var panel in panels)
            UnregisterButtons(panel);
    }

    private void SetupPanelButtons(UIMenuPanel panel)
    {
        foreach (var buttonInfo in panel.buttons)
        {
            if (!buttonInfo.Button)
            {
                Debug.LogWarning($"{panel.panel.name} has a missing Button reference for {buttonInfo.ButtonType}", panel.panel);
                continue;
            }

            switch (buttonInfo.ButtonType)
            {
                // Start/Main Menu Buttons
                case MainMenuButton.StartNewGame: buttonInfo.Button.onClick.AddListener(OnClickStart); break;
                case MainMenuButton.QuitGame: buttonInfo.Button.onClick.AddListener(OnClickQuit); break;

                // Pause Menu Buttons
                case MainMenuButton.ResumeGame: buttonInfo.Button.onClick.AddListener(OnClickResume); break;
                case MainMenuButton.QuitSession: buttonInfo.Button.onClick.AddListener(OnClickQuitSession); break;

                // Character Menu Buttons
                case MainMenuButton.SubmitCharacter: buttonInfo.Button.onClick.AddListener(OnSubmitCharacterClicked); break;

                // Values Menu Buttons
                case MainMenuButton.SubmitValues: buttonInfo.Button.onClick.AddListener(OnSubmitValuesClicked); break;

                // back button
                case MainMenuButton.Back: buttonInfo.Button.onClick.AddListener(OnClickBack); break;

                default:
                    Debug.LogWarning($"Unhandled button type: {buttonInfo.ButtonType}");
                    break;
            }
        }
    }

    private void UnregisterButtons(UIMenuPanel panel)
    {
        foreach (var buttonInfo in panel.buttons)
        {
            if (!buttonInfo.Button) continue;

            switch (buttonInfo.ButtonType)
            {
                case MainMenuButton.StartNewGame: buttonInfo.Button.onClick.RemoveListener(OnClickStart); break;
                case MainMenuButton.QuitGame: buttonInfo.Button.onClick.RemoveListener(OnClickQuit); break;
                case MainMenuButton.ResumeGame: buttonInfo.Button.onClick.RemoveListener(OnClickResume); break;
                case MainMenuButton.QuitSession: buttonInfo.Button.onClick.RemoveListener(OnClickQuitSession); break;
                case MainMenuButton.SubmitCharacter: buttonInfo.Button.onClick.RemoveListener(OnSubmitCharacterClicked); break;
                case MainMenuButton.SubmitValues: buttonInfo.Button.onClick.RemoveListener(OnSubmitValuesClicked); break;
                case MainMenuButton.Back: buttonInfo.Button.onClick.RemoveListener(OnClickBack); break;
            }
        }
    }

    public void ApplyState(MenuScreen state)
    {
        CurrentState = state;

        foreach (var p in panels)
            if (p.panel) p.panel.SetActive(false);

        if (state != MenuScreen.None &&
            TryGetPanel(state, out var panel) &&
            panel)
        {
            panel.SetActive(true);
        }
    }

    private bool TryGetPanel(MenuScreen state, out GameObject panel)
    => _panelMap.TryGetValue(state, out panel) && panel;

    private void OnClickStart()
    {
        if (!GameManager.Instance) { Debug.LogError("No GameManager in scene."); return; }

        GameManager.Instance.StartNewGameFlow();
    }
    private void OnSubmitCharacterClicked()
    {
        if (!TryGetPanel(MenuScreen.Character, out var panel)) return;

        var characterPanel = panel.GetComponentInChildren<CharacterSelectionController>(true);
        if (!characterPanel)
        {
            Debug.LogWarning("MainMenuManager: CharacterSelectionController not found on Character panel.");
            return;
        }

        characterPanel.SubmitCharacterSelection();
        GameManager.Instance.SubmitCharacter();
    }
    private void OnSubmitValuesClicked()
    {
        if (!TryGetPanel(MenuScreen.Values, out var panel)) return;

        var valuesPanel = panel.GetComponentInChildren<ValuesController>(true);
        if (!valuesPanel)
        {
            Debug.LogWarning("MainMenuManager: ValuesController not found on Values panel.");
            return;
        }
        valuesPanel.SubmitValueSelection();
        GameManager.Instance.SubmitValuesAndPlay();
    }
    private void OnClickResume() => GameManager.Instance.Resume();
    private void OnClickBack() => GameManager.Instance.ReturnToPreviousMenu();
    private void OnClickQuitSession() => GameManager.Instance.QuitToMainMenu();
    private void OnClickQuit() => GameManager.Instance.QuitGame();
}

[Serializable]
public class UIMenuPanel
{
    public MenuScreen panelState;
    public GameObject panel;
    public List<ButtonInfo> buttons;

    [Serializable]
    public class ButtonInfo
    {
        public MainMenuButton ButtonType;
        public UnityEngine.UI.Button Button;
    }

}

public enum MainMenuButton
{
    StartNewGame,
    ResumeGame,
    QuitGame,
    QuitSession,
    SubmitCharacter,
    SubmitValues,
    Back
}

