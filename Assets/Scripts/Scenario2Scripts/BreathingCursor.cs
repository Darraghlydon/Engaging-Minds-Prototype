using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BreathingCursor : MonoBehaviour
{
    public float moveSpeed = 140f;
    public float minY = -200f;
    public float maxY = 200f;

    [SerializeField] private Image cursorImage;

    private float inputValue;
    private Vector3 pos;

    void Start()
    {
        //cursorImage = GetComponent<Image>();
        pos = transform.localPosition;
    }

    private void OnEnable()
    {
        //var actions = InputManager.Instance.Actions;

        InputManager.Instance.Actions.BreathingGame.Breathe.performed += OnBreathePerformed;
        InputManager.Instance.Actions.BreathingGame.Breathe.canceled += OnBreatheCanceled;
        
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;

        //var actions = InputManager.Instance.Actions;
        InputManager.Instance.Actions.BreathingGame.Breathe.performed -= OnBreathePerformed;
        InputManager.Instance.Actions.BreathingGame.Breathe.canceled -= OnBreatheCanceled;

        inputValue = 0f;
    }

    private void OnBreathePerformed(InputAction.CallbackContext ctx)
    { 
        inputValue = ctx.ReadValue<float>();
        Debug.Log("Breathing");
    }

    private void OnBreatheCanceled(InputAction.CallbackContext ctx)
    {
        inputValue = 0f;
    }

    void Update()
    {
        pos.y += inputValue * moveSpeed * Time.unscaledDeltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.localPosition = pos;
    }

    public void SetColor(Color cursorColour)
    {
        cursorImage.color = cursorColour;
    }

    public float GetNormalizedHeight()
    {
        return Mathf.InverseLerp(minY, maxY, transform.localPosition.y);
    }
}
