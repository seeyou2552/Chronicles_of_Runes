using UnityEngine;
using UnityEngine.EventSystems;

public class SkillSlotSelector : MonoBehaviour, IPointerClickHandler
{
    //스킬 스왑을 위한 스킬슬롯 선택 스크립트. 스킬 슬롯에 붙임

    SkillSlotController slotController;

    void Awake()
    {
        slotController = GetComponent<SkillSlotController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SwapSelectionManager.Instance.OnSlotClicked(slotController);
    }
}