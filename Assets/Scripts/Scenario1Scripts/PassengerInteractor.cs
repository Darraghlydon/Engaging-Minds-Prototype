using UnityEngine;

public class PassengerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask _passengerMask;
    [SerializeField] private ReactionMinigameController _minigame;
    [SerializeField] private ReactionMinigameProfile _defaultProfile;

    public void TryInteract()
    {
        var hits = Physics.OverlapSphere(transform.position, interactRange, _passengerMask);
        foreach (var h in hits)
        {
            var p = h.GetComponentInParent<Passenger>();
            if (p != null && p.IsNoisy)
            {
                p.Silence();
                Debug.Log("Silence");
                _minigame.Show(_defaultProfile, success =>
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
