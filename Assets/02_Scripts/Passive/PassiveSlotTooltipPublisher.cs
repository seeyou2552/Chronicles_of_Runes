using UnityEngine;
using UnityEngine.EventSystems;

public class PassiveSlotTooltipPublisher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private PassiveSlot passiveSlot;
    private RectTransform rt;

    void Awake()
    {
        passiveSlot = GetComponent<PassiveSlot>();
        rt = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (passiveSlot == null || passiveSlot.Passive == null)
            return;

        Vector2 showPos = new Vector2(rt.position.x, rt.position.y);

        EventBus.Publish(new TooltipShowEvent
        {
            passive = passiveSlot.Passive,
            screenPosition = showPos
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventBus.Publish(new TooltipHideEvent());
    }
}