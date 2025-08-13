using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrateUI : MonoBehaviour
{
    public CrateController  crate;
    public Canvas canvas;
    
    [Header("버튼 그룹")]
    public Button button1;
    public Image itemImage1;
    public TextMeshProUGUI nameText1;
    public TextMeshProUGUI descriptionText1;
    public Button button2;
    public Image itemImage2;
    public TextMeshProUGUI nameText2;
    public TextMeshProUGUI descriptionText2;
    public Button button3;
    public Image itemImage3;
    public TextMeshProUGUI nameText3;
    public TextMeshProUGUI descriptionText3;
    public Button rerollButton;
    public TextMeshProUGUI rerollText;
    public Button exitButton;
    
    private PlayerInputActions inputActions;
    private bool trigger = false;
    
    private void OnEnable()
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();

        inputActions.Player.Enable();
        inputActions.Player.NPCInteration.performed += OnInteract;
        
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.NPCInteration.performed -= OnInteract;
            inputActions.Player.Disable();
        }
        PlayerController.Instance.canControl = true;
    }
    
    void Start()
    {
        canvas.gameObject.SetActive(false);
        button1.onClick.AddListener(() => crate.OnItemSelected(0));
        button2.onClick.AddListener(() => crate.OnItemSelected(1));
        button3.onClick.AddListener(() => crate.OnItemSelected(2));
        rerollButton.onClick.AddListener(() =>
        {
            if (PlayerController.Instance.statHandler.GetRerollCount() > 0)
            {
                PlayerController.Instance.statHandler.RerollCount();
                SetItemData();
            }
        });
        exitButton.onClick.AddListener(() =>
        {
            canvas.gameObject.SetActive(false);
            PlayerController.Instance.canControl = true;
            Time.timeScale = 1f;
        });
        
        SetRerollText();
        SetItemData();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            trigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            trigger = false;
        }
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        if (trigger)
        {
            if (crate.IsOpened) return;
            
            Time.timeScale = 0f;
            PlayerController.Instance.canControl = false;
            canvas.gameObject.SetActive(true);
        }
    }
    
    public void SetRerollText()
    {
        rerollText.text = "Reroll(" + PlayerController.Instance.statHandler.GetRerollCount() + ")";
    }
    
    private void SetItemData()
    {
        ItemData[] items = crate.SetItemData();

        if (items != null)
        {
            itemImage1.sprite = items[0].icon;
            nameText1.text = items[0].displayName;
            descriptionText1.text = items[0].description;

            itemImage2.sprite = items[1].icon;
            nameText2.text = items[1].displayName;
            descriptionText2.text = items[1].description;

            itemImage3.sprite = items[2].icon;
            nameText3.text = items[2].displayName;
            descriptionText3.text = items[2].description;
        }

        SetRerollText();
    }
}
