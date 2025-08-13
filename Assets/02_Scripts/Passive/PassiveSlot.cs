using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PassiveSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private PassiveBase passive;
    public Image image;
    public Sprite nullSprite;
    public TextMeshProUGUI passiveStackText;
    
    // void OnEnable()
    // {
    //     EventBus.Subscribe("StackablePassiveChanged", ChangepassiveStackText);
    // }
    //
    // void OnDisable()
    // {
    //     EventBus.Unsubscribe("StackablePassiveChanged", ChangepassiveStackText);
    // }
    
    public PassiveBase Passive
    {
        get => passive;
        set
        {
            passive = value;
            ChangepassiveStackText();
        }
    }

    public void ChangeImage()
    {
        
        if (passive != null)
        {
            image.sprite = passive.Icon;
            SetAlpha(1f);
        }
        else
        {
            image.sprite =  nullSprite;
            SetAlpha(0f);
        }
    }
    private void SetAlpha(float a)
    {
        var c = image.color;
        c.a = a;
        image.color = c;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (passive == null) return;
    
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position);
    
        // EventBus.Publish(new TooltipShowEvent
        // {
        //     data = passive,
        //     screenPosition = screenPos,
        //     attachedRunes = null // 패시브에는 룬이 없으니 null
        // });
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        EventBus.Publish(new TooltipHideEvent());
    }

    public void ChangepassiveStackText()
    {
        if (passive is IStackablePassive stackable)
        {
            passiveStackText.text = stackable.Stack().ToString();
        }
        else
        {
            passiveStackText.text = string.Empty;
        }
    }
}