using UnityEngine;
using UnityEngine.EventSystems;

public class EnsureSingleEventSystem : MonoBehaviour
{
    void Awake()
    {
        var systems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (systems.Length > 1)
        {
            // Disable or destroy THIS EventSystem
            Debug.Log("Duplicate EventSystem found. Disabling this one.");
            gameObject.SetActive(false);
        }
    }
}