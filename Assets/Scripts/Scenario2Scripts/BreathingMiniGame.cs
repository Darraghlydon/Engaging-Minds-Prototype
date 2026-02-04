using UnityEngine;
using UnityEngine.UI;

public class BreathingMiniGame : MonoBehaviour
{
    public FeatherFloat feather;
    public BreathingCursor cursor;
    public Slider calmSlider;

    [Header("Gameplay")]
    public float successRange = 40f;     // How close counts as "matching"
    public float calmGainRate = 0.4f;    // How fast calm increases
    public float calmLossRate = 0.25f;   // How fast calm decreases

    private float calmValue = 0f; // 0 = panic, 1 = calm

    void Update()
    {
        float featherPos = feather.GetNormalizedBreath();
        float playerPos = cursor.GetNormalizedHeight();

        float distance = Mathf.Abs(featherPos - playerPos);

        float match = Mathf.InverseLerp(0.4f, 0f, distance);

        bool isMatching = distance <= successRange;

        if (isMatching)
        {
            Debug.Log("Matching");
            calmValue += calmGainRate * Time.unscaledDeltaTime;
        }
        else
        {
            Debug.Log("Not Matching");
            calmValue -= calmLossRate * Time.unscaledDeltaTime;
        }

        calmValue = Mathf.Clamp01(calmValue);
        calmSlider.value = calmValue;



        if (calmValue >= 1f)
        {
            OnCalmedDown();
        }
    }

    void OnCalmedDown()
    {
        Debug.Log("Player calmed down :)");
        // Disable minigame, resume normal gameplay, etc.
        enabled = false;
    }
}
