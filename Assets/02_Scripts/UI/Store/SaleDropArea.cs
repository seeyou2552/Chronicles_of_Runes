using UnityEngine;
using UnityEngine.EventSystems;

public class SaleDropArea : MonoBehaviour, IDropHandler
{
    //Sale의 List에 붙여주는 스크립트
    // 판매할 때 드래그된 아이템 드롭받는 곳
    public void OnDrop(PointerEventData eventData)
    {
        // 판매 탭이 활성화된 상태가 아니면 무시
        if (!StoreManager.Instance.IsSaleTabActive)
            return;

        // 드래그 중이던 아이템
        var dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        // 원래 슬롯
        var origin = dragged.originalParent?.GetComponent<InventorySlotController>();
        if (origin == null) return;

        // 판매 요청 이벤트 발행
        EventBus.Publish(new SaleRequestedEvent
        {
            item = dragged,
            originSlot = origin
        });
    }
}
