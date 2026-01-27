using UnityEngine;

public class PassengerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask passengerMask;
    [SerializeField] private ReactionMinigameController minigame;
    [SerializeField] private ReactionMinigameProfile defaultProfile;

    public void TryInteract()
    {
        var hits = Physics.OverlapSphere(transform.position, interactRange, passengerMask);
        foreach (var h in hits)
        {
            var p = h.GetComponentInParent<Passenger>();
            if (p != null && p.IsNoisy)
            {
                p.Silence();
                Debug.Log("Silence");
                minigame.Show(defaultProfile, success =>
                {
                    if (success)
                    {
                    }
                    else
                    {
                    }
                });

                break;
            }
        }
    }
}
