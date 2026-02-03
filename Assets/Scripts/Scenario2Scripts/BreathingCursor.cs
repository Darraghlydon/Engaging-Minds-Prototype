using UnityEngine;
using UnityEngine.InputSystem;

public class BreathingCursor : MonoBehaviour
{
    public float moveSpeed = 140f;
    public float minY = -200f;
    public float maxY = 200f;

    private BreathingControls controls;
    private float inputValue;
    private Vector3 pos;

    void Awake()
    {
        controls = new BreathingControls();

        controls.BreathingGame.Breathe.performed += ctx => inputValue = ctx.ReadValue<float>();
        controls.BreathingGame.Breathe.canceled += ctx => inputValue = 0f;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        pos = transform.localPosition;
    }

    void Update()
    {
        pos.y += inputValue * moveSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.localPosition = pos;
    }

    public float GetNormalizedHeight()
    {
        return Mathf.InverseLerp(minY, maxY, transform.localPosition.y);
    }
}
