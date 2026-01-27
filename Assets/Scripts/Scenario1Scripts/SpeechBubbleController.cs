using Unity.Cinemachine;
using UnityEngine;

public class SpeechBubbleController : MonoBehaviour
{

    private Transform cameraTransform;
    void Awake()
    {
        var brain = FindFirstObjectByType<CinemachineBrain>();
        if (brain != null)
            cameraTransform = brain.GetComponent<Camera>().transform;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Face the camera's POSITION (not its rotation)
        transform.LookAt(cameraTransform.position, Vector3.up);
    }

}
