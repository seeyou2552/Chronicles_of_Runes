using UnityEngine;
using UnityEngine.EventSystems;

public class SaleEventPublisher : MonoBehaviour, IPointerClickHandler
{
    //아이템 프리팹(룬, 포션, 스킬북)에 붙이는 스크립트
    //판매할 때 마우스 오른쪽 클릭 담당

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (!StoreManager.Instance.IsSaleTabActive)
            return;

        var item = GetComponent<DraggableItem>();
        var origin = GetComponentInParent<InventorySlotController>();
        if (item == null || origin == null) return;

        //이벤트 발행
        EventBus.Publish(new SaleRequestedEvent
        {
            item = item,
            originSlot = origin
        });
    }
}
