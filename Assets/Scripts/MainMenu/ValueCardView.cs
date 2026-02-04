using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ValueCardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    [Header("Optional selection visuals")]
    [SerializeField] private GameObject selectedIndicator; // tick, outline, etc.

    public event Action<ValueDefinition> Clicked;

    public string BoundId { get; private set; }
    private ValueDefinition _bound;

    public void Bind(ValueDefinition def, bool selected)
    {
        _bound = def;
        BoundId = def != null ? def.Id : string.Empty;

        if (label) label.text = def != null ? def.DisplayName : "Missing";
        if (icon) icon.sprite = def != null ? def.ValueIcon : null;

        SetSelected(selected);

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Clicked?.Invoke(_bound));
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator) selectedIndicator.SetActive(selected);

        button.targetGraphic.color = selected ? Color.green : Color.white;
    }
}
