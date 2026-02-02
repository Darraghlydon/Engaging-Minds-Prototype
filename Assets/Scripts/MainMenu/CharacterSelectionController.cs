using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{
    [Header("Target UI Image")]
    [SerializeField] private Image targetImage;

    [Header("Start")]
    [SerializeField] private int startIndex = 0;
    [SerializeField] private bool setNativeSize = false;

    [SerializeField] private Transform characterGrid;
    [SerializeField] private GameObject characterButtonPrefab;

    public int CurrentIndex { get; private set; }

    private readonly List<CharacterSelectorButton> _buttons = new();
    private int _selectedButtonIndex = -1;

    private IReadOnlyList<Sprite> PortraitOptions
    {
        get
        {
            if (CharacterManager.Instance == null) return null;
            return CharacterManager.Instance.PortraitOptions;
        }
    }

    private void Start()
    {
        if (!targetImage) targetImage = GetComponent<Image>();

        var options = PortraitOptions;
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("CharacterSelectionController: No portrait options available from CharacterManager.");
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            var buttonObj = Instantiate(characterButtonPrefab, characterGrid);
            var characterSelButton = buttonObj.GetComponent<CharacterSelectorButton>();
            if (characterSelButton)
            {
                characterSelButton.SetPortraitImage(option);
                var button = buttonObj.GetComponent<Button>();
                if (button)
                {
                    int index = i; 
                    button.onClick.AddListener(() => OnButtonClicked(index));
                }

                _buttons.Add(characterSelButton);
            }
            else
            {
                var button = buttonObj.GetComponent<Button>();
                if (button)
                {
                    int index = i;
                    button.onClick.AddListener(() => OnButtonClicked(index));
                }
                _buttons.Add(null);
            }
        }

        var initialIndex = startIndex;
        var currentPortrait = CharacterManager.Instance?.characterData?.SelectedPortrait;
        if (currentPortrait != null)
        {
            var idx = options.ToList().IndexOf(currentPortrait);
            if (idx >= 0) initialIndex = idx;
        }

        SetIndex(initialIndex);
        UpdateSelectionVisuals(_selectedButtonIndex);
    }

    private void OnButtonClicked(int index)
    {
        SetIndex(index);
        UpdateSelectionVisuals(index);
    }

    public void Next()
    {
        var options = PortraitOptions;
        if (options == null || options.Count == 0) return;
        int next = (CurrentIndex + 1) % options.Count;
        SetIndex(next);
        UpdateSelectionVisuals(_selectedButtonIndex);
    }

    public void Prev()
    {
        var options = PortraitOptions;
        if (options == null || options.Count == 0) return;
        int prev = (CurrentIndex - 1 + options.Count) % options.Count;
        SetIndex(prev);
        UpdateSelectionVisuals(_selectedButtonIndex);
    }

    public void SubmitCharacterSelection()
    {
        var portrait = GetSelectedPortrait();
        if (portrait == null) return;

        if (CharacterManager.Instance != null)
            CharacterManager.Instance.SetSelectedPortrait(portrait);
    }

    public Sprite GetSelectedPortrait()
    {
        var options = PortraitOptions;
        if (options == null || options.Count == 0) return null;
        if (CurrentIndex < 0 || CurrentIndex >= options.Count) return null;
        return options[CurrentIndex];
    }

    public void SetIndex(int index)
    {
        var options = PortraitOptions;
        if (options == null || options.Count == 0) return;

        index = Mathf.Clamp(index, 0, options.Count - 1);
        CurrentIndex = index;

        var sprite = options[index];
        if (!sprite)
        {
            Debug.LogWarning($"CharacterSelectionController: Sprite at index {index} is null.");
            return;
        }

        targetImage.sprite = sprite;
        targetImage.enabled = true;

        if (setNativeSize)
            targetImage.SetNativeSize();

        // update internal selected button index
        _selectedButtonIndex = index;
    }

    private void UpdateSelectionVisuals(int newlySelectedIndex)
    {
        // turn off previously selected visuals except the newly selected
        for (int i = 0; i < _buttons.Count; i++)
        {
            var btn = _buttons[i];
            if (btn == null) continue;
            btn.SetSelected(i == newlySelectedIndex);
        }
    }
}
