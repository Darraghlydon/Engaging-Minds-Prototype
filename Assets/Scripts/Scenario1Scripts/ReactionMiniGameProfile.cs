using UnityEngine;

[CreateAssetMenu(menuName = "Minigames/Reaction Minigame Profile")]
public class ReactionMinigameProfile : ScriptableObject
{
    [Range(0f, 1f)] public float zoneCenter = 0.5f;
    [Range(0.01f, 1f)] public float zoneSize = 0.2f;

    [Tooltip("How fast the arrow moves (normalized units per second).")]
    public float speed = 1.2f;

    [Tooltip("Optional time limit; <= 0 means no limit.")]
    public float timeLimit = 2.5f;
}
