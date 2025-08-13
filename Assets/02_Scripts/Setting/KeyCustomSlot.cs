using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyCustomSlot : MonoBehaviour
{
    //키 커스텀 슬롯 프리팹에 붙는 스크립트

    public InputIndex slotIndex;
    public TextMeshProUGUI bindingText;
    public Button rebindButton, confirmButton;

    public TextMeshProUGUI warningText;

    private InputAction action;
    private string pendingPath;
    private static readonly List<KeyCustomSlot> allSlots = new();
    private bool isRebinding = false;
    private InputActionRebindingExtensions.RebindingOperation currentOp;// 현재 이 슬롯이 갖고 있는 리바인딩 연산

    private Dictionary<InputIndex, string> Bindings => SaveManager.Instance.keyBindings;

    private void Start()
    {
        action = SaveManager.Instance.GetActionFor(slotIndex);
        if (action == null)
        {
            enabled = false;
        }

        RefreshDisplay();

        allSlots.Add(this);

        rebindButton.onClick.AddListener(StartRebind);
        confirmButton.onClick.AddListener(ConfirmRebind);
        confirmButton.interactable = false;
    }
    private void StartRebind()
    {
        // 리바인드 시작 전 disable
        action.Disable();

        if (isRebinding || allSlots.Any(s => s.isRebinding))
            return;

        if (pendingPath != null)
        {
            action.RemoveBindingOverride(0);
            pendingPath = null;
            RefreshDisplay();
        }

        bindingText.text = "입력 대기";

        //재입력 허용
        confirmButton.interactable = false;//입력 대기 상태에서는 확인버튼 누를 수 없게

        isRebinding = true;
        UpdateAllButtons();

        currentOp = action.PerformInteractiveRebinding(0)
        // 마우스 포인터 입력 제외
        .WithControlsExcluding("<Mouse>/position")
        .WithControlsExcluding("<Mouse>/delta")
        // WASD 제외
        .WithControlsExcluding("<Keyboard>/w")
        .WithControlsExcluding("<Keyboard>/a")
        .WithControlsExcluding("<Keyboard>/s")
        .WithControlsExcluding("<Keyboard>/d")
        // anyKey 제외
        .WithControlsExcluding("<Keyboard>/anyKey")
        .WithControlsExcluding("<Keyboard>/escape")
        .OnComplete(op => HandleRebindComplete(op));
        currentOp.Start();//실제 입력을 감지하기 시작
    }

    private void RefreshDisplay()
    {
        var current = Bindings[slotIndex];
        var toShow = pendingPath ?? current;
        var utilName = InputControlPath.ToHumanReadableString(
            toShow,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );//inputsystem에서 내부적으로 쓰이는 문자열을 화면에 깔끔하게 보여주기 위해 제공되는 유틸API
        bindingText.text = pendingPath != null
            ? $"{utilName}\n확인 대기"
            : utilName;
    }



    private void HandleRebindComplete(InputActionRebindingExtensions.RebindingOperation op)
    {
        var newPath = action.bindings[0].effectivePath;

        // 중복 검사
        bool isDuplicate = Bindings.Any(kv => kv.Key != slotIndex && kv.Value == newPath);

        if (isDuplicate)
        {
            ShowWarning("이미 사용중인 키입니다");
            op.Dispose();
            action.RemoveBindingOverride(0);
            action.Enable();

            currentOp = null;
            // 다시 리바인드 대기 상태로 진입
            StartRebind();
            return;
        }

        // 정상 후보
        pendingPath = newPath;

        op.Dispose();
        action.Enable();

        currentOp = null;

        confirmButton.interactable = true;
        RefreshDisplay();
    }

    private void ConfirmRebind()
    {
        if (pendingPath == null)
        {
            CancelRebind();
            return;
        }

        // Dictionary 업데이트
        Bindings[slotIndex] = pendingPath;
        //런타임에 바로 적용
        SaveManager.Instance.ApplyKeyBindings();
        SaveManager.Instance.SaveKeyBindingsToPrefs();

        ///// 로그 확인용
        var appliedPath = action.bindings[0].effectivePath;
        var human = InputControlPath.ToHumanReadableString(
            appliedPath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
        ///// 
        EventBus.Publish(new KeyBindingChangedEvent(slotIndex));

        isRebinding = false;
        UpdateAllButtons();

        // 상태 초기화
        currentOp = null;
        pendingPath = null;
        rebindButton.interactable = true;
        confirmButton.interactable = false;
        RefreshDisplay();
    }

    private void CancelRebind()
    {
        // 진행 중인 연산 취소
        if (currentOp != null && !currentOp.completed)
        {
            currentOp.Cancel();
            currentOp.Dispose();
            action.RemoveBindingOverride(0);
            action.Enable();
        }

        // 상태 초기화

        isRebinding = false;
        UpdateAllButtons();

        currentOp = null;
        pendingPath = null;
        rebindButton.interactable = true;
        confirmButton.interactable = false;
        warningText.text = "";
        RefreshDisplay();

    }
    private void OnDisable()//창 비활성화 되면 입력 대기 상태 취소
    {
        CancelRebind();
    }
    void OnDestroy()
    {
        allSlots.Remove(this);
    }
    private static void UpdateAllButtons()
    {
        bool someoneRebinding = allSlots.Any(s => s.isRebinding);
        foreach (var slot in allSlots)
        {
            slot.rebindButton.interactable = !someoneRebinding;
        }
    }

    #region 텍스트 코루틴
    private void ShowWarning(string m)
    {
        warningText.text = m;
        StartCoroutine(ClearText());
    }
    private IEnumerator ClearText()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        warningText.text = "";
    }
    #endregion
}
