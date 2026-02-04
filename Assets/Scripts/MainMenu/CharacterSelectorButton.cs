using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectorButton : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject SelImage;

    private void Awake()
    {
        SetSelected(false);
    }

    internal void SetPortraitImage(Sprite option)
    {
        portraitImage.sprite = option;
    }

    internal void SetSelected(bool toggle)
    {
        SelImage.SetActive(toggle);
    }
}
