using _02_Scripts.Inventory;
using _02_Scripts.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WeightedItem<T>
{
    public T data;               // 실제 아이템 SO
    [Range(0f, 1f)]
    public float weight = 0.1f;  // 이 아이템이 뽑힐 상대적 확률
}
public class PurchaseManager : MonoBehaviour, IUIInterface
{
    public StoreDialogController dialogCtrl;

    [Header("구매 리스트")]
    public Transform purchaseList;     // ScrollView → Content
    public GameObject purchaseSlotPrefab;

    [Header("PurchaseWindow")]
    public GameObject purchaseWindow;
    public Image windowIcon;
    public TextMeshProUGUI windowName;
    public TextMeshProUGUI windowDesc;
    public TextMeshProUGUI windowPrice;
    public Button confirmPurchaseBtn;
    public Button closePurchaseBtn;
    
    public List<StoreItemInfo> StoreItem = new List<StoreItemInfo>();
    

    [Header("아이템 풀")]
    [Range(10, 20)] public int minItems = 10;
    [Range(10, 20)] public int maxItems = 15;
    [Range(0f, 1f)] public float potionProb = 0.2f;
    [Range(0f, 1f)] public float runeProb = 0.4f; // skillProb = 1 - potionProb - runeProb

    [Header("세부 확률 조정")]
    public List<WeightedItem<PotionData>> potionPool;
    public List<WeightedItem<RuneData>> runePool;
    public List<WeightedItem<SkillBookData>> skillPool;

    [Header("인벤토리에 넣을 아이템 프리팹들")]
    [SerializeField] private GameObject potionItemPrefab;
    [SerializeField] private GameObject runeItemPrefab;
    [SerializeField] private GameObject skillItemPrefab;

    // 현재 선택된 슬롯
    private PurchaseSlotController selectedSlot;

    void Awake()
    {
        confirmPurchaseBtn.onClick.AddListener(ConfirmPurchase);
        closePurchaseBtn.onClick.AddListener(CloseUI);
    }

    public void GeneratePurchaseList()
    {
        // 기존 슬롯들 지우기
        foreach (Transform t in purchaseList) Destroy(t.gameObject);

        int count = Random.Range(minItems, maxItems + 1);
        List<int> itemList = PickWeightedItemsWithDuplicate(StoreItem, count);
        for (int i = 0; i < count; i++)
        {
            if (i < itemList.Count)
            {
                var data = ItemManager.Instance.GetItem(itemList[i]);
                var go = Instantiate(purchaseSlotPrefab, purchaseList);
                var slot = go.GetComponent<PurchaseSlotController>();
                slot.Initialize(data, OnSlotClicked);
            }
            
        }
    }

    void OnSlotClicked(PurchaseSlotController slot)
    {
        selectedSlot = slot;
        ShowPurchaseWindow(slot.Data);
        dialogCtrl.Show(DialogType.Purchase);
    }

    void ShowPurchaseWindow(ItemData data)
    {
        UIManager.Instance.RegisterUI(this);

        windowIcon.sprite = data.icon;
        windowName.text = data.displayName;
        windowDesc.text = data.description;
        windowPrice.text = data.price.ToString();
        purchaseWindow.SetActive(true);
    }
    public static List<int> PickWeightedItemsWithDuplicate(List<StoreItemInfo> items, int count)
    {
        List<int> result = new();

        for (int i = 0; i < count; i++)
        {
            float totalWeight = 0f;
            foreach (var item in items)
                totalWeight += item.chance;

            float roll = Random.value * totalWeight;

            foreach (var item in items)
            {
                if (roll <= item.chance)
                {
                    result.Add(item.itemId);
                    break;
                }
                roll -= item.chance;
            }
        }

        return result;
    }


    public void ConfirmPurchase()
    {
        //선택된 슬롯에서 데이터 꺼내오기
        var slot = InventoryManager.Instance.inventoryUIManager.FindFirstEmptySlot();
        if (slot == null)
        {
            StaticPopupManager.Instance.ShowAlert("빈 인벤토리 슬롯이 없습니다.");
            return;
        }
        var data = selectedSlot.Data;
        if (data.price > InventoryManager.Instance.Gold)
        {
            StaticPopupManager.Instance.ShowAlert("금액이 부족합니다.");
            return;
        }

        //포션 구매라면
        if (data.itemType == ItemType.Potion)
        {
            // 포션 슬롯에 이미 있는지
            var diInPotionSlot = InventoryManager.Instance.inventoryUIManager.FindPotionInSlot((PotionData)data);
            if (diInPotionSlot != null)
            {
                //diInPotionSlot.GetComponent<PotionAmount>().curPotionAmount++;
                var parentSlot = diInPotionSlot.transform.parent
                                      .GetComponent<PotionSlotController>();
                if (parentSlot != null)
                {
                    var pa = diInPotionSlot.GetComponent<PotionAmount>();
                    pa.curPotionAmount++;
                    InventoryManager.Instance.consumGold(data.price);
                    InventoryManager.Instance.inventoryUIManager.RenewGold();

                    EventBus.Publish(new PotionChangedEvent
                    {
                        slotIndex = parentSlot.slotIndex,
                        potionData = diInPotionSlot.itemData as PotionData,
                        currentAmount = pa.curPotionAmount
                    });

                    selectedSlot.SoldOut();
                    CloseUI();
                    return;
                }
            }
            dialogCtrl.Show(DialogType.PurchaseConf);
            InventoryManager.Instance.AddItem(data.id);
            InventoryManager.Instance.consumGold(data.price);
            InventoryManager.Instance.inventoryUIManager.RenewGold();

            selectedSlot.SoldOut();
            CloseUI();
            //purchaseWindow.SetActive(false);
            selectedSlot = null;
            return;
        }

        //포션이 아닌 다른 아이템 → 기존 로직
        GameObject prefab = data.itemType switch
        {
            ItemType.Rune => runeItemPrefab,
            ItemType.SkillBook => skillItemPrefab,
            _ => null
        };
        if (prefab == null)
        {
            return;
        }
        dialogCtrl.Show(DialogType.PurchaseConf);
        InventoryManager.Instance.AddItem(data.id);
        InventoryManager.Instance.consumGold(data.price);

        InventoryManager.Instance.inventoryUIManager.RenewGold();
        selectedSlot.SoldOut();
        //purchaseWindow.SetActive(false);
        CloseUI();
        selectedSlot = null;
    }

    public void CloseUI()
    {
        purchaseWindow.SetActive(false);
        UIManager.Instance.UnRegisterUI(this);
    }
}
