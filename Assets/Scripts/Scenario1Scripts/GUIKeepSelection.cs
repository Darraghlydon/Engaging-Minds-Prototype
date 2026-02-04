using UnityEngine;
using UnityEngine.EventSystems;

public class GUIKeepSelection : MonoBehaviour
{
    private GameObject lastSelected;

    void Update()
    {
        // Save whatever is currently selected
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
        else if (lastSelected != null)
        {
            // Restore selection if Unity cleared it
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }
}

