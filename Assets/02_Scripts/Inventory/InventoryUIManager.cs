using _02_Scripts.Inventory;
using _02_Scripts.UI;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIManager : Singleton<InventoryUIManager>, IUIInterface
{
    //인벤토리 매니저
    //I키로 인벤토리 열고 닫기
    //인벤토리에 아이템 넣기 담당

    [Header("인벤토리 슬롯 할당")]
    public InventorySlotController[] inventorySlots;
    [SerializeField] public GameObject[] passiveSlots;
    [SerializeField] public PotionSlotController[] potionSlots;
    public SkillSlotController[] skillSlots;
    
    [SerializeField] private GameObject ItemSlotPrefab;
    [SerializeField] private int itemSlotCount;
    [SerializeField] private GameObject itemSlotParent;

    [SerializeField]
    private GameObject inventoryUIPanel;
    [SerializeField]
    private GameObject SkillUIPanel;
    [SerializeField]
    private GameObject PassiveUIPanel;
    public bool isOpenInventory = false;
    [SerializeField] private TMP_Text GoldText;
    [SerializeField] private GameObject basicSkillBook;
    private PlayerInputActions inputActions;
    
    void OnEnable()
    {
        EventBus.Subscribe(GameState.GameOver, ResetInventory);
        EventBus.Subscribe(GameState.Ending, ResetInventory);
        EventBus.Subscribe(GameState.MainMenu, ResetInventory);
        EventBus.Subscribe<LoadEvent>(OnLoadEvent);
        
        foreach (var skillSlot in skillSlots)
        {
            skillSlot.Init();
        }

        SaveManager.Instance.isLoadingSlotData = false;
    }

    void OnDisable()
    {
        EventBus.Unsubscribe(GameState.GameOver, ResetInventory);
        EventBus.Unsubscribe(GameState.Ending, ResetInventory);
        EventBus.Unsubscribe(GameState.MainMenu, ResetInventory);
        EventBus.Unsubscribe<LoadEvent>(OnLoadEvent);

        // if (inputActions == null) return;
        // inputActions.Player.Inventory.performed -= OnInventoryPerformed;
    }

    private void Awake()
    {
        base.Awake();

        inventorySlots = new InventorySlotController[itemSlotCount];
        for (int i = 0; i < itemSlotCount; i++)
        {
            GameObject go = Instantiate(ItemSlotPrefab, itemSlotParent.transform, false);
            inventorySlots[i] = go.GetComponent<InventorySlotController>();
        }
    }

    public void SkillSlotSet()
    {
        foreach (var skillSlot in skillSlots)
        {
            skillSlot.Init();
        }
    }

    public void OnOffInventory()
    {
        if (isOpenInventory)
        {
            CloseUI();
        }
        else
        {
            OpenUI();
        }
    }

    public void OpenUI()
    {
        if (StoreManager.Instance.storePanel.activeSelf) return; //상점 열려있으면 호출 반환
        // 레벨 선택 창이 떠 있을 땐
        if (UIManager.Instance.IsUIOpen<LevelSelectUI>())
            return;

        // 도감 창이 떠 있을 땐
        if (UIManager.Instance.IsUIOpen<CollectorUI>())
            return;

        // 설정창이 떠 있을 땐
        if (UIManager.Instance.IsUIOpen<SettingUI>())
            return;
        if (!UIManager.Instance.RegisterUI(this))
        {
            return;
        }
        RenewGold();
        foreach (var potionSlot in potionSlots)
        {
            potionSlot.RenewPotionAmount();
        }
        
        inventoryUIPanel.SetActive(true);
        SkillUIPanel.SetActive(true);
        PassiveUIPanel.SetActive(true);
        isOpenInventory = true;

        foreach (var passiveSlot in passiveSlots)
        {
            passiveSlot.GetComponent<PassiveSlot>().ChangepassiveStackText();
        }
        
    }

    public void CloseUI()
    {
        if (!UIManager.Instance.UnRegisterUI(this))
        {
            return;
        }
        inventoryUIPanel.SetActive(false);
        SkillUIPanel.SetActive(false);
        PassiveUIPanel.SetActive(false);
        isOpenInventory = false;

        EventBus.Publish(new TooltipHideEvent());
    }

    public void ShowOnlyInventoryPanel()
    {
        RenewGold();
        inventoryUIPanel.SetActive(true);
        SkillUIPanel.SetActive(false);
        PassiveUIPanel.SetActive(false);
    }
    
    public void HideOnlyInventoryPanel()
    {
        EventBus.Publish(new TooltipHideEvent());
        inventoryUIPanel.SetActive(false);
        SkillUIPanel.SetActive(true);
        PassiveUIPanel.SetActive(true);
    }

    //인벤토리에서 빈 곳에 아이템 넣기
    public InventorySlotController FindFirstEmptySlot()
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.GetComponentInChildren<DraggableItem>() == null)
                return slot;
        }
        return null;
    }
    
    //패시브 칸에서 빈 곳에 아이템 넣기
    public GameObject FindFirstEmptySlotPassive()
    {
        foreach (var slot in passiveSlots) 
        {
            if (slot.GetComponent<PassiveSlot>().Passive == null)
                return slot;
        }
        return null;
    }

    //포션 슬롯 검색하기
    public DraggableItem FindPotionInSlot(PotionData data)
    {
        //포션 슬롯에 구매하려는 아이템이 있다면 수량 증가
        //없고 인벤토리 슬롯에 있다면 수량 증가 (즉 포션은 같은 아이템이면 새 슬롯을 할당받는 것을 방지)

        //먼저 포션 슬롯에서 포션 검색
        foreach (var slot in potionSlots)
        {
            var di = slot.GetComponentInChildren<DraggableItem>();
            if (di != null && di.itemData.id == data.id)
                return di;
        }

        //못 찾았으면 인벤토리 슬롯에서 검색
        foreach (var slot in inventorySlots)
        {
            var di = slot.GetComponentInChildren<DraggableItem>();
            if (di != null && di.itemData.id == data.id)
                return di;
        }

        return null;
    }

    public void RenewGold()
    {
        GoldText.text = InventoryManager.Instance.Gold+" G";
    }

    public void ResetInventory(object param)  //죽었을 떄 인벤토리 초기화
    {
        foreach (var slot in inventorySlots)  //아이템 초기화
        {
            var draggableItem = slot.GetComponentInChildren<DraggableItem>();
            if (draggableItem != null)
            {
                Destroy(draggableItem.gameObject); // 아이템 오브젝트 제거
            }
        }
        
        foreach (var slot in passiveSlots) //패시브 초기화
        {
            if (slot.TryGetComponent(out PassiveSlot tempSlot))
            {
                tempSlot.Passive = null;
                tempSlot.ChangeImage();
            }
            else
                break;
        }
        
        foreach (var slot in potionSlots)  //포션 초기화
        {
            var draggableItem = slot.GetComponentInChildren<DraggableItem>();
            if (draggableItem != null)
            {
                Destroy(draggableItem.gameObject);
            }
        }
        Debug.Log("초기화 실행");
        foreach (var slot in skillSlots)  //스킬 슬롯 초기화
        {
            var draggableItem = slot.GetComponentInChildren<DraggableItem>();
            if (draggableItem != null)
            {
                Destroy(draggableItem.gameObject);
                SkillSlotController tempController = slot.GetComponent<SkillSlotController>();
                EventBus.Publish(new SkillChangedEvent
                {
                    slotIndex = tempController.slotIndex,
                    skillData = null,
                    skillItem = tempController.skillItemCtrl
                });
            }
        }   
    }

    public void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance.IsUIOpen<PauseUI>())
            return;

        if (StaticPopupManager.Instance.IsShowing)
            return;
        if (UIManager.Instance.IsUIOpen<LevelSelectUI>())
            return;
        OnOffInventory();
    }

   private void OnLoadEvent(LoadEvent loadEvent)
    {
        // 1) 모든 슬롯 클리어
        ClearAllSlots();
        InventoryManager.Instance.Gold = loadEvent.saveData.Gold;

        // 2) 슬롯별 Add 호출
        foreach (var inv in loadEvent.saveData.inventoryItems)
        {
            Debug.Log(inv);
            if (inv.itemType == ItemType.Potion)
            {
                AddPotionItem(inv);
            }
            else if (inv.itemType == ItemType.SkillBook)
            {
                AddSkillItem(inv);
            }
            else
            {
                AddRuneItem(inv);
            }
        }
    }

    private void ClearAllSlots()
    {
        // 인벤토리
        foreach (var slot in inventorySlots)
            DestroyExisting(slot.transform);
        // 포션
        foreach (var slot in potionSlots)
            DestroyExisting(slot.transform);
        // 스킬
        foreach (var slot in skillSlots)
            DestroyExisting(slot.transform);
    }

    private void DestroyExisting(Transform parent)
    {
        var existing = parent.GetComponentInChildren<DraggableItem>();
        if (existing != null) Destroy(existing.gameObject);
    }

    private void AddRuneItem(InventoryItemSave data)
    {
        var slot = inventorySlots[data.slotIndex];
        var go   = Instantiate(InventoryManager.Instance.RuneItemPrefab, slot.transform, false);
        var dr   = go.GetComponent<DraggableItem>();
        dr.itemData = ItemManager.Instance.GetItem(data.dataID);
        dr.Setup(dr.itemData);
    }

    private void AddPotionItem(InventoryItemSave data)
    {
        var slot = inventorySlots[data.slotIndex];
        var go   = Instantiate(InventoryManager.Instance.PotionItemPrefab, slot.transform, false);
        var dr   = go.GetComponent<DraggableItem>();
        dr.itemData = ItemManager.Instance.GetItem(data.dataID);
        if (dr.TryGetComponent<PotionAmount>(out var amt))
            amt.curPotionAmount = data.potionAmount;
        dr.Setup(dr.itemData);
    }

    private void AddSkillItem(InventoryItemSave data)
    {
        var slot = inventorySlots[data.slotIndex];
        var go   = Instantiate(InventoryManager.Instance.SkillItemPrefab, slot.transform, false);
        var dr   = go.GetComponent<DraggableItem>();
        dr.itemData = ItemManager.Instance.GetItem(data.dataID);
        if (dr.TryGetComponent<SkillItemController>(out var ctrl))
        {
            ctrl.level = data.skillLevel;
            foreach (var runeId in data.attachedRuneIDs)
            {
                var rune = ItemManager.Instance.GetItem(runeId);
                ctrl.attachedRunes.Add(rune as RuneData);
            }
        }
        dr.Setup(dr.itemData);
    }
    
    public int CountEquippedPassives() //패시브 갯수 검사
    {
        int count = 0;
        foreach (var slot in passiveSlots)
        {
            if (slot.TryGetComponent<PassiveSlot>(out var passiveSlot))
            {
                if (passiveSlot.Passive != null)
                    count++;
            }
        }
        return count;
    }


}
