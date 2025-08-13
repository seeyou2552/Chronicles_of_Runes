using _02_Scripts.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 판매 요청 이벤트
public class SaleRequestedEvent
{
    public DraggableItem item;// 판매할 아이템
    public InventorySlotController originSlot;// 원래 자리
}

// 판매 취소 이벤트
public class SaleCanceledEvent
{
    public SaleSlotController saleSlot;// 취소된 판매 슬롯
}

public class SaleManager : MonoBehaviour
{
    [SerializeField] private float difference = 0.7f;
    public StoreDialogController dialogCtrl;
    [Header("판매 리스트")]
    public Transform saleList; // ScrollView → Content
    public GameObject saleSlotPrefab;

    [Header("판매 확정 창")]
    public Button confirmSaleBtn; //판매 확인 버튼
    public TextMeshProUGUI sellingPriceText;//판매 골드

    private readonly List<SaleSlotController> activeSales = new();

    void Start()
    {
        confirmSaleBtn.onClick.AddListener(ConfirmSale);//판매 확인 버튼 연결
    }

    void OnEnable()
    {
        EventBus.Subscribe<SaleRequestedEvent>(OnSaleRequested);
        EventBus.Subscribe<SaleCanceledEvent>(OnSaleCanceled);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<SaleRequestedEvent>(OnSaleRequested);
        EventBus.Unsubscribe<SaleCanceledEvent>(OnSaleCanceled);
    }

    private void OnSaleRequested(SaleRequestedEvent e)
    {
        //판매 슬롯 생성
        var go = Instantiate(saleSlotPrefab, saleList);
        var slot = go.GetComponent<SaleSlotController>();

        //포션은 개수까지 고려해서 판매
        int potionAmount = 1;
        var pa = e.item.GetComponent<PotionAmount>();
        if (pa != null)
            potionAmount = pa.curPotionAmount;

        slot.Initialize(e.item.itemData, e.item, e.originSlot, potionAmount);

        // 실제 아이템을 OriginSlot에서 뽑아서 이 슬롯으로 옮기기
        e.item.transform.SetParent(go.transform, false);
        e.item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        e.item.SetDragEnabled(false);
        //이미지 안보이게
        var img = e.item.GetComponent<Image>();
        if (img != null) img.enabled = false;

        activeSales.Add(slot);
        UpdateTotalPrice();
    }

    private void OnSaleCanceled(SaleCanceledEvent e)
    {
        //빈 인벤토리 슬롯 찾아서 돌려놓기
        e.saleSlot.ReturnToOrigin();

        //판매 UI에서 제거
        activeSales.Remove(e.saleSlot);
        Destroy(e.saleSlot.gameObject);

        UpdateTotalPrice();
    }

    //현재 판매 리스트에 올라가 있는 모든 슬롯을 취소(인벤토리 복귀) 처리
    public void CancelAllSales()
    {
        // 복제 리스트로 순회해야 루프 도중 변경돼도 안전
        foreach (var slot in new List<SaleSlotController>(activeSales))
        {
            EventBus.Publish(new SaleCanceledEvent { saleSlot = slot });
        }
    }

    public void ConfirmSale()
    {
        int salePrice = 0;// 골드 지급 로직 추가 예정

        // 모든 판매 슬롯의 아이템 파괴
        foreach (var slot in new List<SaleSlotController>(activeSales))
        {
            //salePrice += (int)Mathf.Ceil(slot.Data.price * difference);
            salePrice += Mathf.CeilToInt(slot.Data.price * difference) * slot.Quantity;
            slot.DestroyItem();
            Destroy(slot.gameObject);
        }
        activeSales.Clear();
        InventoryManager.Instance.addGold(salePrice);
        InventoryManager.Instance.inventoryUIManager.RenewGold();
        UpdateTotalPrice();
        dialogCtrl.Show(DialogType.SellConf);
    }

    private void UpdateTotalPrice()
    {
        int total = 0;
        foreach (var slot in activeSales)
        {
            total += Mathf.CeilToInt(slot.Data.price * difference) * slot.Quantity;
        }
        sellingPriceText.text = $"{total:N0} G";
    }

}
