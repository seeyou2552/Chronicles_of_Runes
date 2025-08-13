using _02_Scripts.Inventory;
using _02_Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StoreManager : Singleton<StoreManager>, IUIInterface
{
    [Header("전체 상점 UI")]
    public GameObject storePanel;      // 토글할 상점 패널

    [Header("구매/판매 탭 버튼")]
    public Button purchaseTabBtn;      // 구매 탭
    public Button saleTabBtn;          // 판매 탭 

    [Header("탭 패널")]
    public GameObject purchasePanel;
    public ScrollRect purchaseScrollRect;
    public GameObject salePanel;

    [Header("Sub-Managers")]
    public PurchaseManager purchaseManager;
    public SaleManager saleManager;

    [Header("구매창")]
    public GameObject purchaseWindow;

    public TextMeshProUGUI dialogText;
    public bool IsSaleTabActive => salePanel.activeSelf;

    protected override void Awake()
    {
        base.Awake();
        purchaseTabBtn.onClick.AddListener(ShowPurchaseTab);
        saleTabBtn.onClick.AddListener(ShowSaleTab);
    }

    void Start()
    {
        purchaseManager.StoreItem = StoreItemLoader.LoadStoreItems();
        // 씬 시작 시 한 번만 구매 목록 생성
        purchaseManager.GeneratePurchaseList();
        ShowPurchaseTab();
        storePanel.SetActive(false);
    }

    //NPC 역할 임시 버튼
    public void OnOffStore()
    {
        if (storePanel.activeSelf)
        {
            StaticNoticeManager.Instance.ShowMainNotice();
            CloseUI();
        }
        else
        {
            StaticNoticeManager.Instance.HideMainNotice();
            OpenUI();
        }
    }

    public void CloseUI()
    {
        if (!UIManager.Instance.UnRegisterUI(this))
        {
            return;
        }
        saleManager.CancelAllSales();
        storePanel.SetActive(false);
        InventoryManager.Instance.inventoryUIManager.HideOnlyInventoryPanel();
    }

    public void OpenUI()
    {
        if (!UIManager.Instance.RegisterUI(this))
        {
            return;
        }
        storePanel.SetActive(true);
        purchaseScrollRect.verticalNormalizedPosition = 1f;
        ShowPurchaseTab();
        purchaseWindow.SetActive(false);
        InventoryManager.Instance.inventoryUIManager.ShowOnlyInventoryPanel();
    }

    void ShowPurchaseTab()
    {
        dialogText.text = "뭘 구매하고 싶어?";
        saleManager.CancelAllSales();//구매탭 클릭 시 판매대에 있는것 전부 취소
        purchasePanel.SetActive(true);
        salePanel.SetActive(false);
    }

    void ShowSaleTab()
    {
        dialogText.text = "물건을 판매하려고? 마우스 오른쪽 클릭이나 드래그로 물건을 팔 수 있어";
        purchasePanel.SetActive(false);
        salePanel.SetActive(true);
        if (UIManager.Instance.IsUIOpen<PurchaseManager>())
        {
            UIManager.Instance.UnRegisterUI(purchaseManager);
        }
        purchaseWindow.SetActive(false);
    }
}