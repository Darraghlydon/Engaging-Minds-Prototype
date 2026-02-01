using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }

    public event Action<string> LoadStarted;

    public event Action<string> LoadCompleted;

    public bool IsLoading { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (IsLoading)
        {
            Debug.LogWarning($"SceneLoader: Already loading. Ignoring request for '{sceneName}'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneLoader: sceneName is null/empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"SceneLoader: Scene '{sceneName}' cannot be loaded. Is it in Build Settings?");
            return;
        }

        StartCoroutine(LoadRoutine(sceneName, mode));
    }

    private IEnumerator LoadRoutine(string sceneName, LoadSceneMode mode)
    {
        IsLoading = true;
        LoadStarted?.Invoke(sceneName);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
        if (op == null)
        {
            Debug.LogError($"SceneLoader: LoadSceneAsync returned null for '{sceneName}'.");
            IsLoading = false;
            yield break;
        }

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        IsLoading = false;
        LoadCompleted?.Invoke(sceneName);
    }
}
