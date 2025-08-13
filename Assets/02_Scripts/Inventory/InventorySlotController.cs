using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotController : MonoBehaviour, IDropHandler
{
    //인벤토리의 ItemSlot프리팹에 붙어있는 스크립트 / 드롭 기능 

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        //포션 슬롯에서 드래그 해왔는지 확인
        var fromPotionSlot = dragged.originalParent.GetComponent<PotionSlotController>();
        if (fromPotionSlot != null)
        {
            // 스왑 대상(existing)이 있으면
            var existing = GetComponentInChildren<DraggableItem>();
            if (existing != null)
            {
                // 포션이 아닌 아이템이면 스왑 취소
                if (existing.itemData.itemType != ItemType.Potion)
                {
                    // 드래그된 포션을 원래 자리로 되돌리고 끝
                    dragged.transform.SetParent(dragged.originalParent, false);
                    dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    return;
                }
            }
        }

        //스킬 슬롯에서 드래그 해왔는지 확임
        var fromSkillSlot = dragged.originalParent.GetComponent<SkillSlotController>();
        if (fromSkillSlot != null)
        {
            var existing = GetComponentInChildren<DraggableItem>();
            if (existing != null)
            {
                if (existing.itemData.itemType == ItemType.SkillBook)
                {
                    dragged.isSwapping = true;

                    SkillItemController skillItem = existing.GetComponent<SkillItemController>();
                    fromSkillSlot.ReceiveSkill(existing, existing.itemData as SkillBookData, skillItem.attachedRunes, skillItem.level ,existing.currentCoolTime);
                    //Logger.Log(fromSkillSlot.skillDragItem.ToString());
                    dragged.transform.SetParent(this.transform, false);
                    dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
               
                return;
            }
        }


        //변수에 아이템 저장, 스왑
        var existingItem = GetComponentInChildren<DraggableItem>();
        if (existingItem != null)
        {
            existingItem.transform.SetParent(dragged.originalParent, false);
            existingItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        }

        // 모든 아이템 허용
        dragged.transform.SetParent(transform, false);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        //툴팁 띄우기
        if (RectTransformUtility.RectangleContainsScreenPoint(
        GetComponent<RectTransform>(),
        eventData.position,
        eventData.enterEventCamera))
        {
            // 슬롯 퍼블리셔를 직접 찾아서 OnPointerEnter 흉내
            var publisher = GetComponent<SlotTooltipEventPublisher>();
            publisher.OnPointerEnter(eventData);
        }
    }
}
