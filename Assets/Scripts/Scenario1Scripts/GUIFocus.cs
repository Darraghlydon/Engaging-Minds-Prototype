using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class GUIFocus : MonoBehaviour
{
    [SerializeField] private GameObject defaultButton;

    private void OnEnable()
    {
        StartCoroutine(SetDefaultNextFrame());
    }

    private IEnumerator SetDefaultNextFrame()
    {
        yield return null; // wait one frame
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultButton);
    }
}