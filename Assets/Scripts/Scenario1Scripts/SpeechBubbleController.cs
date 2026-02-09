using Unity.Cinemachine;
using UnityEngine;

public class SpeechBubbleController : MonoBehaviour
{

    private Transform _cameraTransform;
    void Awake()
    {
        var brain = FindFirstObjectByType<CinemachineBrain>();
        if (brain != null)
            _cameraTransform = brain.GetComponent<Camera>().transform;

        var materialRenderer = GetComponent<Renderer>();
        int colourToSet= Random.Range(0, 4);
        switch (colourToSet)
        {
            case 0:
            {
                materialRenderer.material.SetColor("_Color", Color.red);
                break;
            }
            case 1:
            {
                materialRenderer.material.SetColor("_Color", Color.blue);
                break;
            }
            case 2:
            {
                materialRenderer.material.SetColor("_Color", Color.yellowNice);
                break;
            }
            case 3:
            {
                materialRenderer.material.SetColor("_Color", Color.black);
                break;
            }
        }
        
        
        
    }

    void LateUpdate()
    {
        if (_cameraTransform == null) return;

        // Face the camera's POSITION (not its rotation)
        transform.LookAt(_cameraTransform.position, Vector3.up);
    }

}
