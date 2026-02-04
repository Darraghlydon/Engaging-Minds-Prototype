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

    [Header("Stress Interaction")]
    [SerializeField] private StressLevelController stressLevelController;
    [SerializeField] private float secondsPerStressDown = 1.5f;
    [SerializeField] private float secondsPerStressUp = 1.5f;

    private float calmValue = 0f; // 0 = panic, 1 = calm
    private float stressReduceTimer = 0f;
    private float stressTimer = 0f;

    void Update()
    {
        float featherPos = feather.GetNormalizedBreath();
        float playerPos = cursor.GetNormalizedHeight();

        float distance = Mathf.Abs(featherPos - playerPos);

        //float match = Mathf.InverseLerp(0.4f, 0f, distance);
        Debug.Log(distance);
        bool isMatching = distance <= successRange;

        if (isMatching)
        {
            Debug.Log("Matching");
            calmValue += calmGainRate * Time.unscaledDeltaTime;

            stressTimer += Time.unscaledDeltaTime;
            if (stressTimer >= secondsPerStressDown)
            {
                //stressTimer = 0;
                stressTimer -= secondsPerStressDown;
                stressLevelController.ReduceStress(1);
            }
        }
        else
        {
            Debug.Log("Not Matching");
            calmValue -= calmLossRate * Time.unscaledDeltaTime;

            stressTimer += Time.unscaledDeltaTime;
            if (stressTimer >= secondsPerStressUp)
            {
                stressTimer -= secondsPerStressUp;
                //stressTimer = 0;
                stressLevelController.AddStress(1);
            }
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
