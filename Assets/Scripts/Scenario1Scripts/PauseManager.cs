using UnityEngine;

public enum PauseMode
{
    None,
    WorldOnly,   // minigame overlay active
    Hard         // pause key pressed
}

public static class GamePause
{
    public static PauseMode Mode { get; private set; } = PauseMode.None;

    public static bool WorldPaused => Mode == PauseMode.WorldOnly || Mode == PauseMode.Hard;
    public static bool HardPaused => Mode == PauseMode.Hard;

    public static void SetMode(PauseMode mode)
    {
        Mode = mode;

        // World should freeze in both pause types
        Time.timeScale = (mode == PauseMode.None) ? 1f : 0f;

        // Optional: pause audio only on hard pause
        // AudioListener.pause = (mode == PauseMode.Hard);
    }
}