using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActivePotionSlotController : MonoBehaviour
{
    [Header("이 액티브 슬롯의 인덱스 (0부터)")]
    public InputIndex slotIndex;

    public Image potionImage;
    public TextMeshProUGUI amountText;
    [CanBeNull] private PotionData potionData;
    public TMP_Text bindingText;
    public int potionAmount;

    private Color color;

    void OnEnable()
    {
        EventBus.Subscribe<KeyBindingChangedEvent>(OnKeyBindingChanged);
        EventBus.Subscribe<PotionChangedEvent>(OnPotionChanged);
        EventBus.Subscribe(GameState.GameOver, ClearSlot);
        EventBus.Subscribe(GameState.Ending, ClearSlot);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<KeyBindingChangedEvent>(OnKeyBindingChanged);
        EventBus.Unsubscribe<PotionChangedEvent>(OnPotionChanged);
        EventBus.Unsubscribe(GameState.GameOver, ClearSlot);
        EventBus.Unsubscribe(GameState.Ending, ClearSlot);
    }

    void Start()
    {
        // 초기엔 투명하게
        color = potionImage.color;
        color.a = 0f;
        potionImage.color = color;
        amountText.gameObject.SetActive(false);
        potionData = null;
        string path = SaveManager.Instance.keyBindings[slotIndex];
        bindingText.text = InputBindingUtility.GetInitialForPath(path);
    }

    private void OnPotionChanged(PotionChangedEvent evt)
    {
        Logger.Log("PotionChangedEvent ActivePotionSlotController");
        // 내 슬롯 인덱스만 처리
        if (evt.slotIndex != slotIndex) return;

        if (evt.potionData == null)
        {
            Logger.Log("A");
            // 빈 슬롯 처리
            potionImage.sprite = null;
            color.a = 0f;
            potionImage.color = color;
            amountText.gameObject.SetActive(false);
            potionAmount = 0;
        }
        else
        {
            Logger.Log("B");
            // 아이콘 갱신
            potionImage.sprite = evt.potionData.icon;
            color.a = 1f;
            potionImage.color = color;
            potionData = evt.potionData;

            // 수량 갱신
            potionAmount = evt.currentAmount;
            amountText.text = potionAmount.ToString();
            amountText.gameObject.SetActive(true);
        }
    }

    public PotionData GetPotion()
    {
        return potionData;
    }

    public void UseAmount(int amount)
    {
        potionAmount -= amount;
        EventBus.Publish(new PotionChangedEvent
        {
            slotIndex = slotIndex,
            potionData = potionData,
            currentAmount = potionAmount
        });
    }

    private void OnKeyBindingChanged(KeyBindingChangedEvent evt)
    {
        if (evt.slotIndex != slotIndex) return;

        string path = SaveManager.Instance.keyBindings[evt.slotIndex];

        bindingText.text = InputBindingUtility.GetInitialForPath(path);
    }

    public void ClearSlot(object param)
    {
        potionData = null;
        potionImage.sprite = null;
        color.a = 0f;
        potionImage.color = color;
        amountText.gameObject.SetActive(false);
        potionAmount = 0;
    }
}
