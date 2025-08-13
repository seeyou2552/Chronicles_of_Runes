using UnityEngine;
using UnityEngine.InputSystem;

public class NPCController : MonoBehaviour
{
    [SerializeField] private TooltipCanvasHandler tooltipHandler;

    public string tooltipText = "";

    private Transform targetNPC;
    private bool activeTooltip = false;
    private PlayerInputActions inputActions => SaveManager.Instance.inputActions;
    private LoadState currentLoadState;
    
    private void OnEnable()
    {
        //if (inputActions == null)
        //    inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.NPCInteration.performed += OnInteract;

        EventBus.Subscribe(LoadState.LoadStart, SetCurrentLoadStateStart);
        EventBus.Subscribe(LoadState.LoadComplete, SetCurrentLoadStateComplete);
    }

    private void OnDisable()
    {
        inputActions.Player.NPCInteration.performed -= OnInteract;
            
        EventBus.Unsubscribe(LoadState.LoadStart, SetCurrentLoadStateStart);
        EventBus.Unsubscribe(LoadState.LoadComplete, SetCurrentLoadStateComplete);
    }

    private void Start()
    {
        currentLoadState = LoadState.LoadComplete;
    }

    public void SetTarget(Transform npc)
    {
        if (!string.IsNullOrEmpty(tooltipText))
        {
            tooltipHandler.SetTooltip(tooltipText, npc);
            activeTooltip = true;
            targetNPC = npc;
        }
    }

    public void Clear()
    {
        tooltipHandler.ClearTooltip();
        activeTooltip = false;
        targetNPC = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetTarget(this.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Clear();
        }
    }

    public virtual void Interact()
    {

    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!PlayerController.Instance.canControl) return;
        if (context.performed && activeTooltip)
        {
            Interact();
        }
    }

    public void SetCurrentLoadStateStart(object value)
    {
        Clear();
        currentLoadState = LoadState.LoadStart;
    }

    public void SetCurrentLoadStateComplete(object value)
    {
        currentLoadState = LoadState.LoadComplete;
    }
}