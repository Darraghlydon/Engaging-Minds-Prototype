using System;
using UnityEngine;

public class GameEvent
{
    private Action GameAction = delegate { };

    public void Publish()
    {
        foreach (Action a in GameAction.GetInvocationList())
        {
            try { a(); }
            catch (Exception ex)
            {
                Debug.LogError($"Error in subscriber {a.Method.Name}: {ex}");
            }
        }
    }

    public void Subscribe(Action subscriber) => GameAction += subscriber;
    public void Unsubscribe(Action subscriber) => GameAction -= subscriber;

    public void DebugSubscribers()
    {
        foreach (var subscriber in GameAction.GetInvocationList())
        {
            Debug.Log($"Subscriber: {subscriber.Method.Name} Target: {subscriber.Target}");
        }
    }
}

public class GameEvent<T>
{
    private Action<T> GameAction = delegate { };

    public void Publish(T param)
    {
        foreach (Action<T> a in GameAction.GetInvocationList())
        {
            try { a(param); }
            catch (Exception ex)
            {
                Debug.LogError($"Error in subscriber {a.Method.Name}: {ex}");
            }
        }
    }

    public void Subscribe(Action<T> subscriber) => GameAction += subscriber;
    public void Unsubscribe(Action<T> subscriber) => GameAction -= subscriber;

    public void DebugSubscribers()
    {
        foreach (var subscriber in GameAction.GetInvocationList())
        {
            Debug.Log($"Subscriber: {subscriber.Method.Name} Target: {subscriber.Target}");
        }
    }
}

public class GameEvent<S, T>
{
    private Action<S, T> GameAction = delegate { };

    public void Publish(S param1, T param2)
    {
        foreach (Action<S, T> a in GameAction.GetInvocationList())
        {
            try { a(param1, param2); }
            catch (Exception ex)
            {
                Debug.LogError($"Error in subscriber {a.Method.Name}: {ex}");
            }
        }
    }

    public void Subscribe(Action<S, T> subscriber) => GameAction += subscriber;
    public void Unsubscribe(Action<S, T> subscriber) => GameAction -= subscriber;

    public void DebugSubscribers()
    {
        foreach (var subscriber in GameAction.GetInvocationList())
        {
            Debug.Log($"Subscriber: {subscriber.Method.Name} Target: {subscriber.Target}");
        }
    }
}

public static class Events
{
    // Audio
    public static readonly GameEvent AudioSkip = new(); // global command

    // Subtitles
    public static readonly GameEvent Pause = new();
    public static readonly GameEvent Unpause = new();
    public static readonly GameEvent<bool> MiniGameSuccess = new();
    public static readonly GameEvent MaxStressReached = new();
}
