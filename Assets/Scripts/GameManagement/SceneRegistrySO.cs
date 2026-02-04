using System;
using System.Collections.Generic;
using UnityEngine;

public enum SceneId
{
    PersistentUI,
    Main,
    MiniGame1
}

[CreateAssetMenu(menuName = "Game/Scene Registry", fileName = "SceneRegistry")]
public sealed class SceneRegistrySO : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public SceneId id;

        public string sceneName;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<SceneId, string> _idToName;
    private Dictionary<string, SceneId> _nameToId;

    public string GetSceneName(SceneId id)
    {
        EnsureMaps();

        if (_idToName.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name))
            return name;

        throw new KeyNotFoundException($"SceneId '{id}' is missing from SceneRegistry '{this.name}'.");
    }

    public bool TryGetSceneId(string sceneName, out SceneId id)
    {
        EnsureMaps();

        id = default;

        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        return _nameToId.TryGetValue(sceneName.Trim(), out id);
    }

    private void EnsureMaps()
    {
        if (_idToName != null && _nameToId != null)
            return;

        _idToName = new Dictionary<SceneId, string>();
        _nameToId = new Dictionary<string, SceneId>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in entries)
        {
            if (string.IsNullOrWhiteSpace(e.sceneName))
                continue;

            var trimmed = e.sceneName.Trim();

            _idToName[e.id] = trimmed;

            if (!_nameToId.ContainsKey(trimmed))
            {
                _nameToId[trimmed] = e.id;
            }
            else
            {
                Debug.LogWarning(
                    $"SceneRegistry '{name}': Duplicate sceneName '{trimmed}' detected. " +
                    $"Reverse lookup will keep first mapping to '{_nameToId[trimmed]}'.");
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _idToName = null;
        _nameToId = null;
    }
#endif
}
