using UnityEngine;
using System.Collections.Generic;

public class House : InteractItem
{
    private PartyManager partyManager;

    private const string HEAL_MESSAGE = "All characters have rested.";

    protected override void Start()
    {
        base.Start();
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
    }

    public override void Interact()
    {
        List<PartyMember> currentParty = partyManager.GetCurrentParty();
        foreach (PartyMember member in currentParty)
        {
            member.CurrHealth = member.MaxHealth;
        }
        ShowInteractPrompt(true);
        joinPopup.SetActive(true);
        joinPopupText.text = HEAL_MESSAGE;
    }

}
