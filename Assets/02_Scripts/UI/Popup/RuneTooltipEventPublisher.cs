using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RuneTooltipEventPublisher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //스킬 슬롯 안에 있는 룬 슬롯에만 붙은 스크립트. 

    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //  이 슬롯에 실제 룬 아이콘이 있는지 확인
        var di = GetComponentInChildren<DraggableItem>();
        if (di == null || di.itemData == null) return;
        if (di.itemData.itemType != ItemType.Rune) return;

        //툴팁 위치
        Vector2 showPos = new Vector2(rt.position.x, rt.position.y);

        //  룬 정보만 보여 주면 되므로 attachedRunes 는 빈 리스트
        EventBus.Publish(new TooltipShowEvent
        {
            data = di.itemData,
            screenPosition = showPos,
            attachedRunes = new List<RuneData>()
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventBus.Publish(new TooltipHideEvent());
    }
}
