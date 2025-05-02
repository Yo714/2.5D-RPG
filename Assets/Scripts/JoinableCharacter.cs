using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JoinableCharacter : MonoBehaviour
{
    public PartyMemberInfo MemberToJoin;
    [SerializeField] private GameObject interactPrompt;

    void Start()
    {
        CheckIfJoined();
    }

    public void ShowInteractPrompt(bool showPrompt)
    {
        interactPrompt.SetActive(showPrompt);
    }

    public void CheckIfJoined()
    {
        List<PartyMember> currParty = GameObject.FindFirstObjectByType<PartyManager>().GetCurrentParty();

        for (int i = 0; i < currParty.Count; i++)
        {
            if(currParty[i].MemberName == MemberToJoin.MemberName)
            {
                gameObject.SetActive(false);
            }            
        }
    }
}
