using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Portrait Data")]
    [SerializeField] private List<Sprite> options = new();

    [Header("Values Data")]
    [SerializeField] private TextAsset valuesJson;
    [SerializeField] private IconRegistry iconRegistry;

    private List<ValueDefinition> valueDefinitions = new();
    public List<ValueDefinition> selectedValues;

    public IReadOnlyList<ValueDefinition> ValueDefinitions => valueDefinitions;

    public IReadOnlyList<Sprite> PortraitOptions => options;

    public static CharacterManager Instance;

    public CharacterData characterData { get; set; }

    public static event Action<CharacterData> CharacterDataChanged;
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
            return;
        }

        if (iconRegistry) iconRegistry.Build();

        LoadValuesFromJson(valuesJson);

        characterData = new CharacterData
        {
            SelectedValues = selectedValues,
            SelectedPortrait = null
        };
    }

    private void OnEnable()
    {
        GameManager.ResetGameSession += ResetSession;

    }

    private void OnDisable()
    {
        GameManager.ResetGameSession -= ResetSession;
    }

    private void ResetSession()
    {
        selectedValues = new List<ValueDefinition>();
        characterData = new CharacterData
        {
            SelectedValues = selectedValues,
            SelectedPortrait = null
        };
    }
    
    public void SetSelectedValues(List<ValueDefinition> obj)
    {
        selectedValues = obj;
        CharacterDataChanged?.Invoke(characterData);
    }

    public void SetSelectedPortrait(Sprite portrait)
    {
        if (characterData == null) characterData = new CharacterData();
        characterData.SelectedPortrait = portrait;
        CharacterDataChanged?.Invoke(characterData);
    }

    public void LoadValuesFromJson(TextAsset jsonAsset)
    {
        valueDefinitions.Clear();

        if (jsonAsset == null || string.IsNullOrWhiteSpace(jsonAsset.text))
        {
            Debug.LogError("ValuesManager: Missing or empty valuesJson TextAsset.");
            return;
        }

        ValuesJsonRoot root;
        try
        {
            root = JsonUtility.FromJson<ValuesJsonRoot>(jsonAsset.text);
        }
        catch (Exception e)
        {
            Debug.LogError($"ValuesManager: Failed to parse values JSON. Error: {e.Message}");
            return;
        }

        if (root?.values == null || root.values.Count == 0)
        {
            Debug.LogError("ValuesManager: JSON parsed but contained no values.");
            return;
        }

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var v in root.values)
        {
            if (v == null) continue;

            if (string.IsNullOrWhiteSpace(v.id))
            {
                Debug.LogWarning("ValuesManager: Skipping value with missing id.");
                continue;
            }

            if (!seenIds.Add(v.id))
            {
                Debug.LogWarning($"ValuesManager: Duplicate value id '{v.id}' skipped.");
                continue;
            }

            if (v.anti == null)
            {
                Debug.LogWarning($"ValuesManager: Value '{v.id}' has no anti definition; skipped.");
                continue;
            }

            Sprite valueSprite = ResolveSpriteSafe(v.valueIconKey, $"value '{v.id}' valueIconKey");
            Sprite antiSprite = ResolveSpriteSafe(v.anti.antiIconKey, $"value '{v.id}' antiIconKey");

            valueDefinitions.Add(new ValueDefinition
            {
                Id = v.id,
                DisplayName = string.IsNullOrWhiteSpace(v.displayName) ? v.id : v.displayName,
                DialogMessage = string.IsNullOrWhiteSpace(v.dialogMessageKey) ? string.Empty : v.dialogMessageKey,
                ValueIconKey = v.valueIconKey,
                ValueIcon = valueSprite,

                Anti = new AntiValueDefinition
                {
                    Id = v.anti.id,
                    DisplayName = string.IsNullOrWhiteSpace(v.anti.displayName) ? v.anti.id : v.anti.displayName,
                    AntiIconKey = v.anti.antiIconKey,
                    AntiIcon = antiSprite
                }
            });
        }
    }

    private Sprite ResolveSpriteSafe(string iconKey, string context)
    {
        if (string.IsNullOrWhiteSpace(iconKey))
        {
            Debug.LogWarning($"ValuesManager: Missing iconKey for {context}.");
            return null;
        }

        if (!iconRegistry)
        {
            Debug.LogWarning($"ValuesManager: IconRegistry not assigned; cannot resolve '{iconKey}' for {context}.");
            return null;
        }

        var sprite = iconRegistry.Get(iconKey);
        if (!sprite)
            Debug.LogWarning($"ValuesManager: Icon key '{iconKey}' not found in IconRegistry for {context}.");

        return sprite;
    }
}

public class CharacterData
{
    public List<ValueDefinition> SelectedValues { get; set; }
    public Sprite SelectedPortrait { get; set; }
}

#region JSON DTOs

[Serializable]
public class ValuesJsonRoot
{
    public List<ValueJson> values = new();
}

[Serializable]
public class ValueJson
{
    public string id;
    public string displayName;
    public string valueIconKey;
    public string dialogMessageKey;

    public AntiJson anti;
}

[Serializable]
public class AntiJson
{
    public string id;
    public string displayName;
    public string antiIconKey;
}

#endregion

#region Runtime definitions

[Serializable]
public class ValueDefinition
{
    public string Id;
    public string DisplayName;

    public string ValueIconKey;
    public string DialogMessage;

    public Sprite ValueIcon;

    public AntiValueDefinition Anti;
}

[Serializable]
public class AntiValueDefinition
{
    public string Id;
    public string DisplayName;

    public string AntiIconKey;
    public Sprite AntiIcon;
}

#endregion