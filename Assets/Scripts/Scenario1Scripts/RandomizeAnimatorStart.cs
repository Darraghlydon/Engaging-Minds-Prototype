using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomizeAnimatorStart : MonoBehaviour
{
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();

        // Pick a random normalized time (0 = start, 1 = end)
        float randomTime = Random.value;

        // Play the current state on layer 0 at random time
        _animator.Play(0, 0, randomTime);

        // Force animator to update immediately
        _animator.Update(0f);
    }
}