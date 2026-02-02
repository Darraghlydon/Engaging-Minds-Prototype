using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Icon Registry")]
public class IconRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string key;
        public Sprite sprite;
    }

    [SerializeField] private List<Entry> entries = new();

    // Runtime lookup table
    private Dictionary<string, Sprite> _map;

    public void Build()
    {
        _map = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            if (string.IsNullOrWhiteSpace(e.key) || e.sprite == null) continue;
            _map[e.key] = e.sprite;
        }
    }

    public Sprite Get(string key)
    {
        if (_map == null) Build();
        if (string.IsNullOrWhiteSpace(key)) return null;
        return _map.TryGetValue(key, out var s) ? s : null;
    }
}
