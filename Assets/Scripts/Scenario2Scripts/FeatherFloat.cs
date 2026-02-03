using UnityEngine;

public class FeatherFloat : MonoBehaviour
{
    [Header("Motion Range")]
    public float amplitude = 180f;

    [Header("Breathing Timing (seconds)")]
    public float inhaleTime = 2.5f;
    public float topHoldTime = 0.8f;
    public float exhaleTime = 3f;
    public float bottomHoldTime = 1f;

    [Header("Motion Feel")]
    public float smoothTime = 0.6f; // Lower = snappier, Higher = floatier

    [Header("Wobble")]
    public float maxWobbleAmount = 8f;
    public float wobbleSpeed = 2f;

    private Vector3 startPos;
    private float cycleTimer;
    private float velocityY; // required for SmoothDamp

    // Assigned by feedback system
    [HideInInspector] public float calmAmount = 0f; // 0 = stressed, 1 = calm

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float cycleLength = inhaleTime + topHoldTime + exhaleTime + bottomHoldTime;

        cycleTimer += Time.deltaTime;
        if (cycleTimer > cycleLength)
            cycleTimer -= cycleLength;

        float normalizedHeight = GetBreathValue(cycleTimer);
        float baseY = Mathf.Lerp(-amplitude, amplitude, normalizedHeight);

        // WOBBLE (reduced when calm)
        float wobbleStrength = Mathf.Lerp(maxWobbleAmount, 1f, calmAmount);
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleStrength;

        float targetY = startPos.y + baseY + wobble;

        float newY = Mathf.SmoothDamp(
            transform.localPosition.y,
            targetY,
            ref velocityY,
            smoothTime
        );

        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // Slight rotation wobble
        float tilt = Mathf.Sin(Time.time * wobbleSpeed * 1.3f) * wobbleStrength * 0.3f;
        transform.localRotation = Quaternion.Euler(0, 0, tilt);
    }

    float GetBreathValue(float t)
    {
        if (t < inhaleTime)
            return EaseInOut(t / inhaleTime);

        t -= inhaleTime;

        if (t < topHoldTime)
            return 1f;

        t -= topHoldTime;

        if (t < exhaleTime)
            return 1f - EaseInOut(t / exhaleTime);

        return 0f;
    }

    float EaseInOut(float x)
    {
        return x * x * (3f - 2f * x);
    }

    public float GetNormalizedBreath()
    {
        float cycleLength = inhaleTime + topHoldTime + exhaleTime + bottomHoldTime;
        return GetBreathValue(cycleTimer % cycleLength);
    }
}
