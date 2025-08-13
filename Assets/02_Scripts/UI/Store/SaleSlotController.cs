using _02_Scripts.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaleSlotController : MonoBehaviour
{
    //판매 슬롯 프리팹에 붙는 스크립트

    public Image iconImg;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button cancelBtn;

    private ItemData data;
    private DraggableItem item;
    private InventorySlotController originSlot;

    private int quantity;
    public int Quantity => quantity;
    public ItemData Data => data;

    public void Initialize(ItemData data,
                           DraggableItem item,
                           InventorySlotController originSlot,
                           int quantity)
    {
        this.data = data;
        this.item = item;
        this.originSlot = originSlot;
        this.quantity = quantity;

        iconImg.sprite = data.icon;
        nameText.text = data.displayName;
        //priceText.text = Mathf.Ceil(data.price*0.7f).ToString();
        var unitPrice = Mathf.Ceil(data.price * 0.7f);

        //포션처럼 수량이 2 이상일 때만 "가격 G x 수량" 이런식으로 표기
        if (quantity > 1)
            priceText.text = $"{unitPrice} G × {quantity}";
        else
            priceText.text = $"{unitPrice} G";

        cancelBtn.onClick.AddListener(() =>
            EventBus.Publish(new SaleCanceledEvent { saleSlot = this })
        );
    }

    //판매 취소: 빈 인벤토리 슬롯 찾아서 활성화된 아이템 복귀
    public void ReturnToOrigin()
    {
        var dest = InventoryManager.Instance.inventoryUIManager.FindFirstEmptySlot();
        if (dest == null) return;

        item.transform.SetParent(dest.transform, false);
        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        item.gameObject.SetActive(true);
        item.SetDragEnabled(true);
        //이미지 다시 보이게
        var img = item.GetComponent<Image>();
        if (img != null) img.enabled = true;
    }

    //판매 확정: 아이템 파괴 + 추후 골드 추가도 구현해야함
    public void DestroyItem()
    {
        Destroy(item.gameObject);
    }
}
