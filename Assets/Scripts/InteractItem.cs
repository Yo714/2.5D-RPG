using UnityEngine;
using TMPro;
using UnityEngine.TextCore.Text;

public class InteractItem : MonoBehaviour
{
    [SerializeField] private GameObject interactPrompt;
    protected GameObject joinPopup;
    protected TextMeshProUGUI joinPopupText;
    protected CharacterManager characterManager;

    protected virtual void Start() 
    {
        characterManager = GameObject.FindFirstObjectByType<CharacterManager>();
        if (characterManager != null)
        {
            joinPopup = characterManager.GetJoinPopup();
            joinPopupText = characterManager.GetJoinPopupText();
        }
    }

    public virtual void Interact()
    {
        //Function
    }

    public void ShowInteractPrompt(bool showPrompt)
    {
        interactPrompt.SetActive(showPrompt);
    }
}
