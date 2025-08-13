using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveSkillTooltipPublisher : MonoBehaviour,　IPointerEnterHandler, IPointerExitHandler
{
    //실제 사용할 액티브 스킬 슬롯의 툴팁

    [Header("툴팁을 Y축으로 얼마나 올릴건지")]
    public float yOffset = 50f;

    private ActiveSkillSlotController activeSlotCtrl;
    private RectTransform rt;

    void Awake()
    {
        activeSlotCtrl = GetComponent<ActiveSkillSlotController>();
        rt = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventBus.Publish(new TooltipHideEvent());
    }
}