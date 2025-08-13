using _02_Scripts.UI;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [Header("세팅부분")]
    public SettingUI settingUI;
    public Button settingBtn;
    
    
    [Header("세이브 리스트")] public GameObject saveList;
    public Button openBtn;
    public Button closeBtn;
    
    
    [Header("저장 슬롯")]
    public SaveSlotController[] slotCtrls;

    [Header("닉네임 입력 팝업")]
    public GameObject namePopup;
    public TMP_InputField nameInputField;
    public Button confirmNameBtn, cancelNameBtn;

    private int pendingSlot = -1;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        // “UI/Cancel”(혹은 자신이 만든 CloseUI 액션) 액션 구독
        inputActions.Player.Paused.performed += ctx =>
        {
                CloseUI();
        };
        
        inputActions.Enable();
        
        openBtn.onClick.AddListener(OpenUI);
        closeBtn.onClick.AddListener(CloseUI);
        settingBtn.onClick.AddListener(settingUI.OnSettings);
    }
    
    private void OnDestroy()
    {
        // 메모리 누수 방지
        inputActions.Player.Paused.performed -= ctx => { if (saveList.activeSelf) CloseUI(); };
        inputActions.Disable();
    }

    void Start()
    {
        // 초기 UI 리프레시
        foreach (var sc in slotCtrls)
            sc.Refresh();

        namePopup.SetActive(false);
        confirmNameBtn.onClick.AddListener(OnConfirmName);
        cancelNameBtn.onClick.AddListener(() => namePopup.SetActive(false));
    }

    // 빈 슬롯 클릭 시 호출
    public void OpenNameInput(int slotIndex)
    {
        pendingSlot = slotIndex;
        nameInputField.text = "";
        namePopup.SetActive(true);
    }

    // 매개변수 없는 콜백으로 변경
    private void OnConfirmName()
    {
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName)) return;

        SaveManager.Instance.CreateNewSlot(pendingSlot, playerName);

        // UI를 갱신
        slotCtrls[pendingSlot].Refresh();

        namePopup.SetActive(false);
        pendingSlot = -1;
    }

    public void OpenUI()
    {
        saveList.SetActive(true);
    }

    public void CloseUI()
    {
        
        if (saveList.activeSelf)
        {
            saveList.SetActive(false);    
        }
        if (settingUI.setting.activeSelf)
        {
            settingUI.ClosePanel();
        }
    }
}
