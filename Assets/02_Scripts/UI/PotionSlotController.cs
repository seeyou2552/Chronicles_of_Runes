using _02_Scripts.Inventory;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class PotionChangedEvent
{
    public InputIndex slotIndex;// 0부터 시작하는 슬롯 번호
    public PotionData potionData;// 장착된 포션의 데이터
    public int currentAmount;// 현재 보유 수량
}

public class PotionSlotController : MonoBehaviour, IDropHandler
{
    [Header("이 슬롯의 인덱스 (0부터)")] public InputIndex slotIndex;

    [Header("이 슬롯에 표시할 포션 수량 텍스트")] public TextMeshProUGUI slotAmountText;
    
    [SerializeField] private ActivePotionSlotController potionSlotController;

    private DraggableItem currentPotion;
    private PotionAmount potionAmtComp;

    void OnEnable()
    {
        EventBus.Subscribe<PotionChangedEvent>(OnPotionChanged);
        EventBus.Subscribe<LoadEvent>(OnLoadEvent);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<PotionChangedEvent>(OnPotionChanged);
        EventBus.Unsubscribe<LoadEvent>(OnLoadEvent);
    }

    void Awake()
    {
        slotAmountText.gameObject.SetActive(false);
    }

    public void RenewPotionAmount()
    {
        if (potionAmtComp != null)
        {
            if (potionSlotController.potionAmount == 0)
            {
                Destroy(currentPotion.gameObject);
            }
            else
            {
                potionAmtComp.curPotionAmount = potionSlotController.potionAmount;    
            }
                
        }
    }
    
    // 포션 슬롯에 드롭 됐을 때
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null || dragged.itemData.itemType != ItemType.Potion) return;
        if (InventoryFilter.CurrentFilter != ItemType.None &&
            dragged.itemData.itemType != InventoryFilter.CurrentFilter) return;

        var existing = GetComponentInChildren<DraggableItem>();
        if (existing != null && existing != dragged)
        {
            var paDragged = dragged.GetComponent<PotionAmount>();
            var paExisting = existing.GetComponent<PotionAmount>();

            // 같은 포션이면 → 수량 합산
            if (dragged.itemData == existing.itemData)
            {
                paExisting.curPotionAmount += paDragged.curPotionAmount;
                // 드래그해온 아이템은 제거
                Destroy(dragged.gameObject);

                // 텍스트 갱신 이벤트 발행
                slotAmountText.text = paExisting.curPotionAmount.ToString();
                EventBus.Publish(new PotionChangedEvent
                {
                    slotIndex = slotIndex,
                    potionData = existing.itemData as PotionData,
                    currentAmount = paExisting.curPotionAmount
                });
            }
            //다른 포션이면 → 자리 교체
            else
            {
                //기존 슬롯에 들어 있던 existing을 원래 dragged의 슬롯에 넣기
                var originParent = dragged.originalParent;
                existing.transform.SetParent(originParent, false);
                existing.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                //dragged를 이 슬롯에 넣기
                dragged.transform.SetParent(transform, false);
                dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            // 공통으로 툴팁 띄우기
            GetComponent<SlotTooltipEventPublisher>()?.OnPointerEnter(eventData);
            return;
        }

        // 새 포션 배치
        dragged.transform.SetParent(transform, false);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        var pa = dragged.GetComponent<PotionAmount>();
        if (pa != null)
        {
            // **디버그 로그로 실제 값을 확인**

            currentPotion = dragged;
            potionAmtComp = pa;

            // 드롭된 직후 실제 값을 다시 읽어서 표시
            int amount = potionAmtComp.curPotionAmount;


            slotAmountText.text = amount.ToString();
            slotAmountText.gameObject.SetActive(true);

            EventBus.Publish(new PotionChangedEvent
            {
                slotIndex     = slotIndex,
                potionData    = dragged.itemData as PotionData,
                currentAmount = amount
            });
        }

        GetComponent<SlotTooltipEventPublisher>()?.OnPointerEnter(eventData);
    }

    //슬롯 자식(포션) 수가 변할 때 자동으로 호출
    void OnTransformChildrenChanged()
    {
        if (transform.childCount == 1)
        {
            // 슬롯에 포션이 없으면 텍스트 숨김
            slotAmountText.gameObject.SetActive(false);
            currentPotion = null;
            potionAmtComp = null;

            EventBus.Publish(new PotionChangedEvent
            {
                slotIndex = slotIndex,
                potionData = null,
                currentAmount = 0
            });
        }
        else
        {
            // 슬롯에 포션이 다시 돌아왔으면 텍스트 켜고 수량 갱신
            var di = GetComponentInChildren<DraggableItem>();
            if (di != null)
            {
                var pa = di.GetComponent<PotionAmount>();
                if (pa != null)
                {
                    slotAmountText.text = pa.curPotionAmount.ToString();
                    slotAmountText.gameObject.SetActive(true);

                    currentPotion = di;
                    potionAmtComp = pa;

                    EventBus.Publish(new PotionChangedEvent
                    {
                        slotIndex = slotIndex,
                        potionData = di.itemData as PotionData,
                        currentAmount = pa.curPotionAmount
                    });
                }
            }
        }
    }

    void OnPotionChanged(PotionChangedEvent evt)
    {
        if (evt.slotIndex != slotIndex) return;

        if (evt.potionData == null)
        {
            ClearPotionSlot();
            return;
        }
        potionAmtComp.curPotionAmount = evt.currentAmount;
        slotAmountText.text = evt.currentAmount.ToString();
    }

    private void ClearPotionSlot()
    {
         slotAmountText.gameObject.SetActive(false);
            currentPotion = null;
            potionAmtComp = null;
    }
    
    private void OnLoadEvent(LoadEvent loadEvent)
    {
        // 1) 슬롯 초기화
        ClearPotionSlot();
        var existing = GetComponentInChildren<DraggableItem>();
        if (existing != null)
            Destroy(existing.gameObject);

        // 2) 저장된 데이터에서 이 슬롯 정보 찾기
        var save = loadEvent.saveData.potionSlots
            .FirstOrDefault(p => p.slotIndex == (int)slotIndex);
        if (save == null) return;

        // 3) 프리팹 인스턴스화
        var go = Instantiate(InventoryManager.Instance.PotionItemPrefab,
            transform, false);
        var di = go.GetComponent<DraggableItem>();

        // 4) 데이터 복원
        di.itemData = ItemManager.Instance.GetItem(save.dataID);
        var paComp = di.GetComponent<PotionAmount>();
        di.Setup(di.itemData as PotionData);
        if (paComp != null)
        {
            paComp.curPotionAmount = save.potionAmount;
            slotAmountText.text    = save.potionAmount.ToString();
            slotAmountText.gameObject.SetActive(true);

            currentPotion = di;
            potionAmtComp = paComp;

            EventBus.Publish(new PotionChangedEvent
            {
                slotIndex     = slotIndex,
                potionData    = di.itemData as PotionData,
                currentAmount = save.potionAmount
            });
        }
    }
}
