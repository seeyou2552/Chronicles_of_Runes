using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlotController : MonoBehaviour, IDropHandler
{
    //인벤토리 내에서 휴지통 기능을 담당하는 스크립트

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        //장착했던 스킬을 버릴 때만 특별 처리
        if (dragged.itemData.itemType == ItemType.SkillBook)
        {
            // 원래 있었던 SkillSlotController 찾아서 ClearSlot 호출
            var originSlot = dragged.originalParent?.GetComponent<SkillSlotController>();
            if (originSlot != null)
                originSlot.ClearSlot(); // 여기서 SkillChangedEvent 이벤트버스도 같이 발행됨
        }
        Destroy(dragged.gameObject);

    }
}
