using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValuesController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private ValueCardView cardPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private int requiredSelectionCount = 3;
    [SerializeField] private Image portraitImage;

    private readonly List<ValueCardView> _cards = new();
    private readonly HashSet<string> _selectedIds = new(StringComparer.OrdinalIgnoreCase);

    private void OnEnable()
    {
        if (!CharacterManager.Instance)
        {
            return;
        }

        SetupUI();
    }

    private void OnDisable()
    {
        ClearUI();
    }

    private void SetupUI()
    {
        BuildUI();
        UpdateConfirmButton();
    }

    public void BuildUI()
    {
        ClearUI();

        if (!gridParent || !cardPrefab)
        {
            Debug.LogError("ValuesPanelController: gridParent/cardPrefab not assigned.");
            return;
        }

        portraitImage.sprite = CharacterManager.Instance.characterData.SelectedPortrait;

        gridParent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cardPrefab.GetComponent<RectTransform>().sizeDelta.x, cardPrefab.GetComponent<RectTransform>().sizeDelta.y);
        var defs = CharacterManager.Instance.ValueDefinitions;
        for (int i = 0; i < defs.Count; i++)
        {
            var def = defs[i];

            var card = Instantiate(cardPrefab, gridParent);
            card.Bind(def, IsSelected(def.Id));
            card.Clicked += OnCardClicked;

            _cards.Add(card);
        }

        if (confirmButton)
            confirmButton.onClick.AddListener(SubmitValueSelection);
    }

    private void ClearUI()
    {
        if (confirmButton)
            confirmButton.onClick.RemoveListener(SubmitValueSelection);

        for (int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i] != null)
                _cards[i].Clicked -= OnCardClicked;
        }
        _cards.Clear();

        if (gridParent)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
                Destroy(gridParent.GetChild(i).gameObject);
        }

        _selectedIds.Clear();
    }

    private void OnCardClicked(ValueDefinition def)
    {
        if (def == null) return;

        var id = def.Id;

        if (_selectedIds.Contains(id))
        {
            _selectedIds.Remove(id);
        }
        else
        {
            if (_selectedIds.Count >= requiredSelectionCount)
            {
                return;
            }
            _selectedIds.Add(id);
        }

        RefreshCardStates();
        UpdateConfirmButton();
    }

    private void RefreshCardStates()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];
            if (!card) continue;
            card.SetSelected(IsSelected(card.BoundId));
        }
    }

    private void UpdateConfirmButton()
    {
        if (!confirmButton) return;
        confirmButton.interactable = (_selectedIds.Count == requiredSelectionCount);
    }

    public void SubmitValueSelection()
    {
        if (_selectedIds.Count != requiredSelectionCount) return;

        var confirmed = new List<ValueDefinition>(requiredSelectionCount);
        var defs = CharacterManager.Instance.ValueDefinitions;

        for (int i = 0; i < defs.Count; i++)
        {
            var def = defs[i];
            if (_selectedIds.Contains(def.Id))
                confirmed.Add(def);
        }

        CharacterManager.Instance.SetSelectedValues(confirmed);
        DialogManager.Instance.SetNPCDialogues();
    }

    private bool IsSelected(string valueId) => _selectedIds.Contains(valueId);

}
