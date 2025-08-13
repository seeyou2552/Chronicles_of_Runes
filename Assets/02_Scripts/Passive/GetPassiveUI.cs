using _02_Scripts.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetPassiveUI : MonoBehaviour
{
    public GetPassiveController getPassiveController;
    public GameObject getPassiveUI;
    
    [Header("버튼 그룹")]
    public Button button1;
    public Image passiveImage1;
    public TextMeshProUGUI nameText1;
    public TextMeshProUGUI descriptionText1;
    public Button button2;
    public Image passiveImage2;
    public TextMeshProUGUI nameText2;
    public TextMeshProUGUI descriptionText2;
    public Button button3;
    public Image passiveImage3;
    public TextMeshProUGUI nameText3;
    public TextMeshProUGUI descriptionText3;
    public Button rerollButton;
    public TextMeshProUGUI rerollText;
    public Button exitButton;
    
    private PassiveBase[] items;

    
    private void OnEnable()
    {
        EventBus.Subscribe("DungeonStart", PassiveUIOn);
        // EventBus.Subscribe(GameState.GameOver, ExitUI);
    }
    private void OnDisable()
    {
        
    }
    private void Start()
    {
        SetItemData();
        
        button1.onClick.AddListener(() => { getPassiveController.OnItemSelected(items[0]); ExitUI(null); });
        button2.onClick.AddListener(() => { getPassiveController.OnItemSelected(items[1]); ExitUI(null); });
        button3.onClick.AddListener(() => { getPassiveController.OnItemSelected(items[2]); ExitUI(null); });
        exitButton.onClick.AddListener(() => ExitUI(null));
        rerollButton.onClick.AddListener(() =>
        {
            if (PlayerController.Instance.statHandler.GetRerollCount() > 0)
            {
                PlayerController.Instance.statHandler.RerollCount();
                SetItemData();
            }
        });
        
        SetRerollText();
    }

    private void ExitUI(object value)
    {
        Time.timeScale = 1f;

        if (PlayerController.Instance.isDead == false)
        {
            PlayerController.Instance.canControl = true;
        }
        
        if (getPassiveUI != null)
        {
            getPassiveUI.SetActive(false);
        }
    }
    
    private void SetItemData()  //랜덤하게 3개의 패시브를 UI에 표시
    {
        items = getPassiveController.SetItemData();

        passiveImage1.sprite = items[0].Icon;
        nameText1.text = items[0].Name;
        descriptionText1.text = items[0].Description;
        
        passiveImage2.sprite = items[1].Icon;
        nameText2.text = items[1].Name;
        descriptionText2.text = items[1].Description;
        
        passiveImage3.sprite = items[2].Icon;
        nameText3.text = items[2].Name;
        descriptionText3.text = items[2].Description;
        
        SetRerollText();
    }

    public void SetRerollText()
    {
        rerollText.text = "Reroll(" + PlayerController.Instance.statHandler.GetRerollCount() + ")";
    }
    
    public void PassiveUIOn(object value)
    {
        if (InventoryManager.Instance.inventoryUIManager.CountEquippedPassives() >= GameManager.Instance.sceneFlowManager.currentStageIndex)
        {
            return;
        }
        
        if(GameManager.Instance.state != GameState.Dungeon)
            return;
        PlayerController.Instance.canControl = false;
        if (getPassiveUI != null)
        {
            getPassiveUI.SetActive(true);
            SetItemData();
            PlayerController.Instance.OffCanControl(null);
        }
    }
    
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.H))
    //     {
    //         getPassiveUI.SetActive(true);
    //         SetItemData();
    //         Time.timeScale = 0f;
    //     }
    // }
}
