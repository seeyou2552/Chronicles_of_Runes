using _02_Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    private PlayerInputActions inputActions;

    void OnEnable()
    {
        inputActions = SaveManager.Instance.inputActions;
        if (inputActions == null) return;
        inputActions.Player.Inventory.performed += InventoryUIManager.Instance.OnInventoryPerformed;
        inputActions.Player.Paused.performed += UIManager.Instance.OnPerformedEsc;
        SaveManager.Instance.isLoadingSlotData = false;
        UIManager.Instance.inputAction.Player.Enable();

        if(GameManager.Instance.tempState == GameState.MainMenu) InventoryUIManager.Instance.SkillSlotSet();
    }

    void OnDisable()
    {
        if (inputActions == null) return;
        inputActions.Player.Inventory.performed -= InventoryUIManager.Instance.OnInventoryPerformed;
        inputActions.Player.Paused.performed -= UIManager.Instance.OnPerformedEsc;
    }
}
