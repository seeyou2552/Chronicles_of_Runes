using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotTooltipEventPublisher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    //아이템 슬롯, 스킬 슬롯 프리팹에 붙은 스크립트

    // 스킬 슬롯이면 참조할 컨트롤러
    private SkillSlotController skillSlot;
    // 인벤토리 슬롯이면
    private InventorySlotController invSlot;
    //포션 슬롯이면 
    private PotionSlotController potionSlot;
    //도감 슬롯이면
    [SerializeField]
    private CollectorSlot collectorSlot;
    private RectTransform rt;


    void Awake()
    {
        skillSlot = GetComponent<SkillSlotController>();
        invSlot = GetComponent<InventorySlotController>();
        potionSlot = GetComponent<PotionSlotController>();
        collectorSlot = GetComponent<CollectorSlot>();
        rt = GetComponent<RectTransform>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 드래그 중일 땐 무시
        if (DraggableItem.IsDragging) return;

        PublishShowIfValid(eventData.pointerEnter);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventBus.Publish(new TooltipHideEvent());
    }

    // 추가된 부분
    // 아이템을 슬롯에 드롭하는 순간 호출
    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 중인 아이템인지 확인
        var di = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (di == null || !di.dragEnabled) return;

        // 기존 툴팁 숨기고
        EventBus.Publish(new TooltipHideEvent());

        // 드롭 직후 이 슬롯에 툴팁 띄우기
        PublishShowIfValid(gameObject);
    }

    // OnPointerEnter와 OnDrop에서 공통으로 쓰는 로직
    private void PublishShowIfValid(GameObject go)
    {
        if (go == null) return;

        // 룬 슬롯 위라면 무시
        if (go.GetComponentInParent<RuneSlotController>() != null)
            return;

        //자신인지 확인
        if (go.GetComponentInParent<SlotTooltipEventPublisher>() != this)
            return;

        // 실제 아이템 데이터 추출
        ItemData data = null;
        AchievementData achievData = null;
        List<RuneData> runes = new List<RuneData>();

        if (skillSlot != null && skillSlot.skillDragItem != null)
        {
            data = skillSlot.CurrentSkill;
            runes = new List<RuneData>(
                skillSlot.skillDragItem
                         .GetComponent<SkillItemController>()
                         .attachedRunes
            );
        }
        else if (invSlot != null)
        {
            var childDi = GetComponentInChildren<DraggableItem>();
            if (childDi != null)
            {
                data = childDi.itemData;
                var sic = childDi.GetComponent<SkillItemController>();
                if (sic != null)
                    runes = new List<RuneData>(sic.attachedRunes);
            }
        }
        else if (potionSlot != null)
        {
            var childDi = GetComponentInChildren<DraggableItem>();
            if (childDi != null && childDi.itemData is PotionData pData)
            {
                data = pData;
            }
        }
        else if (collectorSlot != null)
        {
            achievData = collectorSlot.achievementData;
            data = collectorSlot.itemData;
        }

        Vector2 slotPos = rt.position;

        // 데이터 없으면 스킵
        if (data == null && achievData == null) return;

        EventBus.Publish(new TooltipShowEvent
        {
            data = data,
            achievData = achievData,
            screenPosition = slotPos,
            attachedRunes = runes,
            isCollectorSlot = (collectorSlot != null)
        });
    }
}