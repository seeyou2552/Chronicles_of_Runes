using _02_Scripts.Inventory;
using _02_Scripts.NPC;
using _02_Scripts.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject MoveDirectionObject;
    [SerializeField] private GameObject MoveDirectionLastPointObject;
    [SerializeField] private GameObject TutorialUI;
    [SerializeField] private GameObject DefaultSkillPrefab;
    [SerializeField] private ItemData SkillSO;
    [SerializeField] private GameObject DefaultRunePrefab;
    [SerializeField] private ItemData RuneSO;
    private PlayerInputActions inputActions;
    [SerializeField] private GameObject SkillTutorialPanel;
    [SerializeField] private GameObject RuneTutorialPanel;
    [SerializeField] private GameObject ShopNpc;
    
    bool isGetSkill = false;
    bool isGetRune = false;

    public int currentStep;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Enable();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<SkillChangedEvent>(OnSkillChanged);
        inputActions.Player.Inventory.performed += OnInventoryPerformed;
        inputActions.Player.Paused.performed += OnESCPerformed;
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SkillChangedEvent>(OnSkillChanged);
        inputActions.Player.Inventory.performed -= OnInventoryPerformed;
        inputActions.Player.Paused.performed -= OnESCPerformed;
    }

    private void Start()
    {
        if (GameManager.Instance.state == GameState.Tutorial)
        {
            currentStep = -1;
            // TutorialDirected();
            TutorialDirected();  
        }
    }

    private void TutorialDirected()
    {
        UIManager.Instance.baseUI.SetActive(false);
        PlayerController.Instance.canControl = false;
        TutorialUI.SetActive(true);
        TutorialUI.GetComponent<PlayableDirector>().Play();
        ShopNpc.gameObject.SetActive(false);
    }

    public void StartTutorial()
    {
        UIManager.Instance.baseUI.SetActive(true);
        PlayerController.Instance.canControl = true;
        StaticNoticeManager.Instance.ShowMainNotice("WASD로 이동할 수 있습니다. 화살표를 따라 이동해주세요.");
        MoveDirectionObject.SetActive(true);
    }

    public void OnReachMoveDirectionEnd()
    {
        if (currentStep == -1)
        {
            MoveDirectionObject.SetActive(false);
            StaticNoticeManager.Instance.ShowMainNotice("늙은 마법사에게 말을 걸어보자");
            currentStep = 0;    
        }
    }

    public void TutorialStep(int step)
    {
        switch (step)
        {
            case 12 :
                string path = SaveManager.Instance.keyBindings[InputIndex.Inventory];
                string text = InputBindingUtility.GetInitialForPath(path);
                StaticNoticeManager.Instance.ShowMainNotice(text+"로 인벤토리를 열 수 있다. 스킬을 등록한 후 사용해보자");
                currentStep = 1;
                break;
            case 13 :
                StaticNoticeManager.Instance.ShowMainNotice("인벤토리를 열어 스킬 아래 룬슬롯에 룬을 장착 후 사용해보자");
                if (!isGetRune)
                {
                    InventoryManager.Instance.AddItem(4);
                    isGetRune = true;
                }
                break;
            case 14 :
                GameManager.Instance.ChangeState(GameState.Village);
                break;
            
        }
        
    }

    public void SkillTutorial()
    {
        foreach (var skill in PlayerSkillManager.Instance.slotSkills)
        {
            skill.Value.skill.onUseSkill += OnAnyTutorialSkillUsed; 
        }
    }

    private void OnAnyTutorialSkillUsed()
    {
        foreach (var skill in PlayerSkillManager.Instance.skills)
            skill.onUseSkill -= OnAnyTutorialSkillUsed;

        StaticNoticeManager.Instance.ShowMainNotice("세르마에게 다시 말을 걸자");
        currentStep = 3;

        AchievementData firstMagic = Resources.Load<AchievementData>("Achievement/First_Magic"); // 처음 마법 사용 업적
        if (!CollectorManager.Instance.achievDataDic.ContainsKey(firstMagic.displayName))
        {
            CollectorManager.Instance.achievDataDic.Add(firstMagic.displayName, firstMagic);
        }
    }

    private async void OnSkillChanged(SkillChangedEvent e)
    {
        if (currentStep != 1 && currentStep != 3) return;

        if (currentStep == 1)
        {
            await UniTask.Delay(100); // 100ms 정도만 기다리면 대부분 등록됨
            SkillTutorial();
        }

        if (e.skillItem != null && e.skillItem.attachedRunes != null)
        {
            foreach (var rune in e.skillItem.attachedRunes)
            {
                if (rune != null)
                {
                    CompleteRuneTutorial();
                    break;
                }
            }
        }
    }
    
    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        if (currentStep != 1 && currentStep != 3) return;
        if (!isGetSkill)
        {
            InventoryManager.Instance.AddItem(11);
            isGetSkill = true;
        }

        if (InventoryManager.Instance.inventoryUIManager.isOpenInventory)
        {
            if (isGetRune)
            {
                RuneTutorialPanel.SetActive(true);
            }
            else
            {
                SkillTutorialPanel.SetActive(true);
            }
        }
        else
        {
            RuneTutorialPanel.SetActive(false);
            SkillTutorialPanel.SetActive(false);
        }
    }

    private void OnESCPerformed(InputAction.CallbackContext ctx)
    {
        if (currentStep != 1 && currentStep != 3) return;
        RuneTutorialPanel.SetActive(false);
        SkillTutorialPanel.SetActive(false);
    }

    private void CompleteRuneTutorial()
    {
        EventBus.Unsubscribe<SkillChangedEvent>(OnSkillChanged); // 이벤트 해제
        StaticNoticeManager.Instance.ShowMainNotice("세르마에게 다시 말을 걸자");
        RuneTutorialPanel.SetActive(false);
        inputActions.Player.Inventory.performed -= OnInventoryPerformed;
        currentStep = 4;

        AchievementData firstRune = Resources.Load<AchievementData>("Achievement/First_of_Rune"); // 처음 룬 착용 업적
        if (!CollectorManager.Instance.achievDataDic.ContainsKey(firstRune.displayName))
        {
            CollectorManager.Instance.achievDataDic.Add(firstRune.displayName, firstRune);
        }
    }
    
    public int GetCurrentStep()
    {
        return currentStep;
    }
}