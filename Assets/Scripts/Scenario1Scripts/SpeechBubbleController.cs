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
    }

    void LateUpdate()
    {
        if (_cameraTransform == null) return;

        // Face the camera's POSITION (not its rotation)
        transform.LookAt(_cameraTransform.position, Vector3.up);
    }

}
