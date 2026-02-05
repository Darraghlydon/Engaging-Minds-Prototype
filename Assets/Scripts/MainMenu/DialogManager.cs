using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [SerializeField] private TextAsset dialogAsset = null;
    [SerializeField] private TextAsset completedInkDialogAsset = null;

    public event Action<OfficeNPC> OnDialogStart;

    [Serializable]
    private sealed class NpcState
    {
        public bool completed;
        public string assignedMessage;
        [NonSerialized] public OfficeNPC live;
    }

    private readonly Dictionary<string, NpcState> _statesById =
        new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
        ResetRun();
    }

    public void ResetRun()
    {
        foreach (var kvp in _statesById)
        {
            var state = kvp.Value;
            state.completed = false;

            if (state.live != null)
            {
                state.live.TextAsset = dialogAsset;
                state.live.SetAssignedValueMessage(state.assignedMessage ?? string.Empty);
            }
        }

        Debug.Log("Resetting NPCs dialogues");
    }

    public void RegisterNPC(OfficeNPC npc)
    {
        if (npc == null) return;
        if (string.IsNullOrWhiteSpace(npc.NpcId)) return;

        var id = npc.NpcId.Trim();
        var state = GetOrCreate(id);

        state.live = npc;

        npc.TextAsset = state.completed ? completedInkDialogAsset : dialogAsset;
        npc.SetAssignedValueMessage(state.assignedMessage ?? string.Empty);
    }

    public void UnregisterNPC(OfficeNPC npc)
    {
        if (npc == null) return;
        if (string.IsNullOrWhiteSpace(npc.NpcId)) return;

        var id = npc.NpcId.Trim();
        if (_statesById.TryGetValue(id, out var state) && state.live == npc)
            state.live = null;
    }

    private NpcState GetOrCreate(string npcId)
    {
        if (!_statesById.TryGetValue(npcId, out var state))
        {
            state = new NpcState();
            _statesById[npcId] = state;
        }
        return state;
    }

    public void StartDialogWithNPC(OfficeNPC npc)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (npc == null || string.IsNullOrWhiteSpace(npc.NpcId)) return;

        if (!gm.TryEnterDialogue())
            return;

        var id = npc.NpcId.Trim();
        var state = GetOrCreate(id);

        npc.SetAssignedValueMessage(state.assignedMessage ?? string.Empty);
        npc.TextAsset = state.completed ? completedInkDialogAsset : dialogAsset;

        OnDialogStart?.Invoke(npc);
    }

    public void RequestMinigame1(OfficeNPC npc)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (npc == null || string.IsNullOrWhiteSpace(npc.NpcId)) return;

        var id = npc.NpcId.Trim();
        MarkCompleted(id);

        gm.ExitDialogue();

        gm.SetLevelState(LevelState.Minigame1);
    }

    public void MarkCompleted(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId)) return;

        var id = npcId.Trim();
        var state = GetOrCreate(id);
        if (state.completed) return;

        state.completed = true;

        if (state.live != null)
            state.live.TextAsset = completedInkDialogAsset;
    }

    public void TryTriggerGameOverIfInOffice()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.CurrentLevelState != LevelState.Office)
            return;

        if (_statesById.Count == 0) return;

        foreach (var kvp in _statesById)
        {
            if (!kvp.Value.completed)
                return;
        }

        gm.SetMenuState(MenuScreen.GameOverScreen);
    }

    public void SetNPCDialogues()
    {
        var confirmed = CharacterManager.Instance?.characterData?.SelectedValues;
        if (confirmed == null || confirmed.Count == 0)
        {
            Debug.LogWarning("DialogManager: No selected values to assign.");
            return;
        }

        var messages = confirmed
            .Where(v => v != null && !string.IsNullOrWhiteSpace(v.DialogMessage))
            .Select(v => v.DialogMessage)
            .ToList();

        if (messages.Count == 0)
        {
            Debug.LogWarning("DialogManager: Selected values had no dialog messages.");
            return;
        }

        if (_statesById.Count == 0)
        {
            Debug.LogWarning("DialogManager: No NPCs registered yet.");
            return;
        }

        var npcIds = _statesById.Keys
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (int i = 0; i < npcIds.Count; i++)
        {
            var npcId = npcIds[i];
            var msg = messages[i % messages.Count];

            var state = GetOrCreate(npcId);
            state.assignedMessage = msg;

            if (state.live != null)
                state.live.SetAssignedValueMessage(msg);
        }
    }
}
