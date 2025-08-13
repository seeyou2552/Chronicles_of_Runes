using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PurchaseSlotController : MonoBehaviour, IPointerClickHandler
{
    //구매 슬롯 프리팹에 붙는 스크립트

    public Image iconImg;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public GameObject soldOutWindow;

    private ItemData data;
    private Action<PurchaseSlotController> clickCallback;
    public ItemData Data => data;

    //초기설정
    public void Initialize(ItemData itemData, Action<PurchaseSlotController> onClick)
    {
        data = itemData;
        iconImg.sprite = data.icon;
        nameText.text = data.displayName;
        priceText.text = data.price.ToString();
        soldOutWindow.SetActive(false);
        clickCallback = onClick;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (soldOutWindow.activeSelf) return;
        clickCallback?.Invoke(this);
    }

    public void SoldOut()
    {
        soldOutWindow.SetActive(true);
    }
}
