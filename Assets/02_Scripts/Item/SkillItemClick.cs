using UnityEngine;
using UnityEngine.EventSystems;

public class SkillItemClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var selector = GetComponentInParent<SkillSlotSelector>();
        if (selector != null)
            selector.OnPointerClick(eventData);
    }
}
