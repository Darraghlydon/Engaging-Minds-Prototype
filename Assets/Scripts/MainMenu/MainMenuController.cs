using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private MainMenuManager _menuManager;

    private void Start()
    {
        GameManager.Instance.MenuStateChanged += OnMenuStateChanged;
        OnMenuStateChanged(GameManager.Instance.CurrentMenuState);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.MenuStateChanged -= OnMenuStateChanged;
    }

    private void OnMenuStateChanged(MenuScreen state)
    {
        _menuManager.ApplyState(state);
    }
}

