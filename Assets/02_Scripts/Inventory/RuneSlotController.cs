using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RuneSlotController : MonoBehaviour,IDropHandler
{
    //룬 슬롯에 붙어있는 스크립트

    [HideInInspector] public SkillSlotController parentSkillSlot;

    private bool isOpen = false;
    [SerializeField] private Image slotImage;
    public Image lockImage;
    private Color color;

    void Awake()
    {
        color = slotImage.color;
    }

    public void SetInteractable(bool open)
    {
        color = slotImage.color;
        isOpen = open;
        // 슬롯 투명도 설정(잠금 상태를 눈으로 확인)
        //추후 수정 가능성 큼
        //color.a = open ? 1f : 0.3f;
        //slotImage.color = color;
        lockImage.enabled = !open;
        // 열려 있을 때만 드롭 허용하게
        slotImage.raycastTarget = open;
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (!isOpen) return;
        var dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null || !dragged.dragEnabled) return;
        //인벤토리 필터가 걸린 상태면 룬 슬롯으로 이동 불가
        if (InventoryFilter.CurrentFilter != ItemType.None
            && dragged.itemData.itemType != InventoryFilter.CurrentFilter)
            return;
        if (dragged.itemData.itemType != ItemType.Rune) return;

        // 이미 룬이 장착되어 있다면 → 덮어씌우기 확인
        if (transform.childCount > 0)
        {
            var oldRune = transform.GetChild(0).gameObject;
            var oldData = oldRune.GetComponent<DraggableItem>().itemData as RuneData;
            var newData = dragged.itemData as RuneData;

            OverwriteAlert.Show(oldData, newData,
                onYes: () => {
                    PerformDrop(dragged);
                    Destroy(oldRune);
                    },
                onNo: () =>
                {
                    // 원위치 복귀
                    dragged.transform.SetParent(dragged.originalParent, false);
                    dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            );
            return;
        }

        // 빈 슬롯이면 바로 배치
        PerformDrop(dragged);
    }

    //룬을 장착하고 SkillChangedEvent/TooltipShowEvent 발행
    private void PerformDrop(DraggableItem dragged)
    {
        //데이터 갱신
        var skillSlot = GetComponentInParent<SkillSlotController>();
        var skillCtrl = skillSlot.GetComponentInChildren<SkillItemController>();
        // 이 슬롯의 인덱스 (0 ~ 4)
        //현재 룬 슬롯의 부모 아래에서 몇 번째 자식인지 0부터 시작하는 정수로 반환
        int slotIndex = transform.GetSiblingIndex();
        var runeData = dragged.itemData as RuneData;

        if (slotIndex < skillCtrl.attachedRunes.Count)
            skillCtrl.attachedRunes[slotIndex] = runeData;
        else
            skillCtrl.attachedRunes.Add(runeData);

        //UI 상 배치
        dragged.transform.SetParent(transform, false);
        var rt = dragged.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        //더 이상 드래그 금지
        dragged.SetDragEnabled(false);

        //ActiveSkillSlot 동기화
        EventBus.Publish(new SkillChangedEvent
        {
            slotIndex = skillSlot.slotIndex,
            skillData = skillSlot.CurrentSkill,
            skillItem = skillSlot.skillItemCtrl,
            currentCoolTime = PlayerSkillManager.Instance.slotSkills[skillSlot.slotIndex].currentCoolTime
        });

        //툴팁 즉시 재출력 할 때 툴팁 생성위치의 기준은 룬 아이템이 아니라 룬 슬롯
        var slotRt = GetComponent<RectTransform>();
        //툴팁 즉시 재출력
        Vector2 screenPt = new Vector2(slotRt.position.x, slotRt.position.y);
        EventBus.Publish(new TooltipShowEvent
        {
            data = dragged.itemData,
            screenPosition = screenPt + Vector2.up,
            attachedRunes = new List<RuneData>()
        });
        
        PlayerController.Instance.GetStatModifier().SkillSlotChanged();
    }
}
