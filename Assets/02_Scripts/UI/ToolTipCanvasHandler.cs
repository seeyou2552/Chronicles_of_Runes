using TMPro;
using UnityEngine;

public class TooltipCanvasHandler : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0.5f, 0.5f, 0);
    private InputIndex slotIndex = InputIndex.Interact;

    private Transform targetWorldObject;
    private bool isActive = false;
    
    void OnEnable()
    {
        EventBus.Subscribe<KeyBindingChangedEvent>(OnKeyBindingChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<KeyBindingChangedEvent>(OnKeyBindingChanged);
    }

    void Start()
    {
        string path = SaveManager.Instance.keyBindings[slotIndex];
        bindingText.text = InputBindingUtility.GetInitialForPath(path);
    }

    public void SetTooltip(string text, Transform target)
    {
        if (string.IsNullOrEmpty(text)) return;

        tooltipPanel.SetActive(true);
        tooltipText.text = text;
        targetWorldObject = target;
        isActive = true;
    }

    public void ClearTooltip()
    {
        tooltipPanel.SetActive(false);
        tooltipText.text = "";
        targetWorldObject = null;
        isActive = false;
    }

    private void Update()
    {
        if (!isActive || targetWorldObject == null) return;

        Vector3 worldPos = targetWorldObject.position + worldOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        tooltipPanel.transform.position = screenPos;
    }
    
        
    private void OnKeyBindingChanged(KeyBindingChangedEvent evt)
    {
        if (evt.slotIndex != slotIndex) return;

        string path = SaveManager.Instance.keyBindings[evt.slotIndex];

        bindingText.text = InputBindingUtility.GetInitialForPath(path);
    }
}