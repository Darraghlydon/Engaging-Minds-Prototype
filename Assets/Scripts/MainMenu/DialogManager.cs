using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [SerializeField] private TextAsset dialogAsset = null;
    [SerializeField] private TextAsset completedInkDialogAsset = null;

    private bool _transitioningToMinigame = false;

    public event Action<OfficeNPC> OnDialogStart;

    [Serializable]
    private class NpcState
    {
        public bool completed;
        public string assignedMessage;

        [NonSerialized] public OfficeNPC live;
    }

    private readonly Dictionary<string, NpcState> _npcs =
        new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameManager.ResetGameSession += ResetSession;
    }

    private void OnDisable()
    {
        GameManager.ResetGameSession -= ResetSession;
    }

    private void ResetSession()
    {
        _npcs.Clear();
        _transitioningToMinigame = false;
    }

    internal void BindController(DialogController dialogController)
    {
        dialogController.OnMinigameRequested += HandleMiniGame1Request;
        dialogController.OnEndStory += EndDialogWithNPC;
    }

    internal void UnbindController(DialogController dialogController)
    {
        dialogController.OnMinigameRequested -= HandleMiniGame1Request;
        dialogController.OnEndStory -= EndDialogWithNPC;
    }

    public void RegisterNPC(OfficeNPC npc)
    {
        if (npc == null || string.IsNullOrWhiteSpace(npc.NpcId)) return;

        var state = GetOrCreateNpcState(npc.NpcId);
        state.live = npc;

        npc.TextAsset = state.completed ? completedInkDialogAsset : dialogAsset;

        npc.SetAssignedValueMessage(state.assignedMessage ?? string.Empty);
    }

    public void UnregisterNPC(OfficeNPC npc)
    {
        if (npc == null || string.IsNullOrWhiteSpace(npc.NpcId)) return;

        if (_npcs.TryGetValue(npc.NpcId, out var state) && state.live == npc)
            state.live = null;
    }

    private NpcState GetOrCreateNpcState(string npcId)
    {
        if (!_npcs.TryGetValue(npcId, out var state))
        {
            state = new NpcState();
            _npcs[npcId] = state;
        }
        return state;
    }

    public void StartDialogWithNPC(OfficeNPC npc)
    {
        if (GameManager.Instance.CurrentGameState != GameState.Office) return;
        if (npc == null || string.IsNullOrWhiteSpace(npc.NpcId)) return;

        if (_npcs.TryGetValue(npc.NpcId, out var state))
            npc.SetAssignedValueMessage(state.assignedMessage ?? string.Empty);

        _transitioningToMinigame = false;

        GameManager.Instance.SetGameState(GameState.Dialogue);
        OnDialogStart?.Invoke(npc);
    }

    public void EndDialogWithNPC()
    {
        if (_transitioningToMinigame) return;
        GameManager.Instance.SetGameState(GameState.Office);
    }

    private void HandleMiniGame1Request(OfficeNPC npc)
    {
        if (npc == null || string.IsNullOrWhiteSpace(npc.NpcId)) return;

        _transitioningToMinigame = true;

        var state = GetOrCreateNpcState(npc.NpcId);
        state.completed = true;

        GameManager.Instance.SetGameState(GameState.Minigame);
    }

    public void OnMinigameEnded()
    {
        PollCompletion();
        if (GameManager.Instance.CurrentGameState != GameState.GameOver)
            GameManager.Instance.SetGameState(GameState.Office);
    }

    private void PollCompletion()
    {
        if (_npcs.Count == 0) return;

        int completedCount = 0;
        foreach (var kvp in _npcs)
            if (kvp.Value.completed) completedCount++;

        if (completedCount >= _npcs.Count)
            GameManager.Instance.SetGameState(GameState.GameOver);
    }

    public void SetNPCDialogues()
    {
        var confirmed = CharacterManager.Instance.selectedValues;
        if (confirmed == null || confirmed.Count == 0)
        {
            Debug.LogWarning("DialogManager: No selected values to assign.");
            return;
        }

        var messages = new List<string>(confirmed.Count);
        for (int i = 0; i < confirmed.Count; i++)
        {
            var def = confirmed[i];
            if (def == null) continue;

            if (!string.IsNullOrWhiteSpace(def.DialogMessage))
                messages.Add(def.DialogMessage);
        }

        if (messages.Count == 0)
        {
            Debug.LogWarning("DialogManager: Selected values had no dialog messages.");
            return;
        }

        var npcIds = _npcs.Keys.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList();

        for (int i = 0; i < npcIds.Count; i++)
        {
            string npcId = npcIds[i];
            string msg = messages[i % messages.Count];

            var state = GetOrCreateNpcState(npcId);
            state.assignedMessage = msg;

            if (state.live != null)
                state.live.SetAssignedValueMessage(msg);
        }
    }
}
