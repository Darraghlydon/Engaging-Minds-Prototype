using System.Xml.Serialization;
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
    [SerializeField] private Color matchColor;
    [SerializeField] private Color mismatchColor;

    private float calmValue = 0f; // 0 = panic, 1 = calm
    private float matchTimer = 0f;
    private float mismatchTimer = 0f;


    private void OnEnable()
    {
        calmValue = 0;
    }

    void Update()
    {
        float featherPos = feather.GetNormalizedBreath();
        float playerPos = cursor.GetNormalizedHeight();

        float distance = Mathf.Abs(featherPos - playerPos);

        //float match = Mathf.InverseLerp(0.4f, 0f, distance);
        Debug.Log(distance);
        bool isMatching = distance <= successRange;
        float dt = Time.unscaledDeltaTime;

        if (isMatching)
        {
            Debug.Log("Matching");
            cursor.SetColor(matchColor);
            calmValue += calmGainRate * dt;

            // Matching streak builds, mismatch streak resets
            matchTimer += dt;
            mismatchTimer = 0f;

            if (matchTimer >= secondsPerStressDown)
            {
                matchTimer = 0f; // or: matchTimer -= secondsPerStressDown; (allows multiple ticks if dt is big)
                stressLevelController.ReduceStress(1);
            }
        }
        else
        {
            Debug.Log("Not Matching");
            cursor.SetColor(mismatchColor);
            calmValue -= calmLossRate * Time.unscaledDeltaTime;

            mismatchTimer += dt;
            matchTimer = 0f;

            if (mismatchTimer >= secondsPerStressUp)
            {
                mismatchTimer = 0f; // or: mismatchTimer -= secondsPerStressUp;
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
