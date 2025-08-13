using _02_Scripts.Inventory;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02_Scripts.NPC;

public class TutorialNPCController : NPCController
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private List<string> tutorialDialogueKeys;
    bool IsGetItem;

    void Awake()
    {
        IsGetItem = false;
    }

    public override async void Interact()
    {
        if (GameManager.Instance.state == GameState.Tutorial)
        {
            int step = tutorialManager.GetCurrentStep();
            Logger.Log(step.ToString());
            if (step >= 0 && step < tutorialDialogueKeys.Count)
            {
                StaticNoticeManager.Instance.ShowMainNotice("대화는 Space로 진행 할 수 있습니다. W,S로 대답를 선택할수 있습니다.");
                int result = await DialogueManager.Instance.StartDialogue(tutorialDialogueKeys[step]);
                Logger.Log(result.ToString());
                tutorialManager.TutorialStep(result);
            }
        }
        else
        {
            if (IsGetItem == false)
            {
                IsGetItem = true;
            }
            await DialogueManager.Instance.StartDialogue("TutorialDialogue/TutorialDialogue0");
           
        }
    }
}