using System;
using UnityEngine;

public class OfficeNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcId = "npc_1";
    public string NpcId => npcId;

    public string DialogueMessage { get; set; }

    public TextAsset TextAsset { get; set; }

    public Sprite npcImage;

    void Start()
    {
        if (DialogManager.Instance) DialogManager.Instance.RegisterNPC(this);
    }

    void OnDisable()
    {
        if (DialogManager.Instance) DialogManager.Instance.UnregisterNPC(this);
    }

    public void Interact(LookInteractor interactor)
    {
        DialogManager.Instance.StartDialogWithNPC(this);
    }

    internal void SetAssignedValueMessage(string value)
    {
        DialogueMessage = value;
    }
}

public interface IInteractable
{
    void Interact(LookInteractor interactor);
}