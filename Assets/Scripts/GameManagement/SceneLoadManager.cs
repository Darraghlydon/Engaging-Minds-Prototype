using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoadManager : MonoBehaviour
{
    public SceneRegistrySO sceneRegistry { get; set; }

    public bool IsLoading { get; private set; }

    public IEnumerator Load(SceneId id, bool additive = true, bool setActive = true)
    {
        if (IsLoading) yield break;
        if (sceneRegistry == null)
        {
            Debug.LogError("SceneLoadManager: sceneRegistry is null.");
            yield break;
        }

        var sceneName = sceneRegistry.GetSceneName(id);

        if (additive)
        {
            var existing = SceneManager.GetSceneByName(sceneName);
            if (existing.IsValid() && existing.isLoaded)
            {
                if (setActive) SceneManager.SetActiveScene(existing);
                yield break;
            }
        }

        IsLoading = true;

        var mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        var op = SceneManager.LoadSceneAsync(sceneName, mode);
        if (op == null)
        {
            Debug.LogError($"SceneLoadManager: LoadSceneAsync returned null for '{sceneName}'.");
            IsLoading = false;
            yield break;
        }

        while (!op.isDone)
            yield return null;

        if (setActive)
        {
            var loaded = SceneManager.GetSceneByName(sceneName);
            if (loaded.IsValid() && loaded.isLoaded)
                SceneManager.SetActiveScene(loaded);
        }

        IsLoading = false;
    }

    public IEnumerator Unload(SceneId id)
    {
        if (IsLoading) yield break;
        if (sceneRegistry == null)
        {
            Debug.LogError("SceneLoadManager: sceneRegistry is null.");
            yield break;
        }

        var sceneName = sceneRegistry.GetSceneName(id);
        var scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.IsValid() || !scene.isLoaded)
            yield break;

        IsLoading = true;

        var op = SceneManager.UnloadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"SceneLoadManager: UnloadSceneAsync returned null for '{sceneName}'.");
            IsLoading = false;
            yield break;
        }

        while (!op.isDone)
            yield return null;

        IsLoading = false;
    }

    public void SetActive(SceneId id)
    {
        if (sceneRegistry == null)
        {
            Debug.LogError("SceneLoadManager: sceneRegistry is null.");
            return;
        }

        var sceneName = sceneRegistry.GetSceneName(id);
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
            SceneManager.SetActiveScene(scene);
    }
}
