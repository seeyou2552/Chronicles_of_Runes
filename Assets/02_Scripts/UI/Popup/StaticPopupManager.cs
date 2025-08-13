using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaticPopupManager : Singleton<StaticPopupManager>
{
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    public bool IsShowing => confirmPopup.activeSelf;

    void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject); 
    }

    public void ShowConfirm(string message, Action onYes, Action onNo = null)
    {
        messageText.text = message;
        noButton.gameObject.SetActive(true);
        confirmPopup.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            onYes?.Invoke();
            confirmPopup.SetActive(false);
        });

        noButton.onClick.AddListener(() =>
        {
            onNo?.Invoke();
            confirmPopup.SetActive(false);
        });
    }

    public void ShowAlert(string message)
    {
        messageText.text = message;
        noButton.gameObject.SetActive(false);
        confirmPopup.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            confirmPopup.SetActive(false);
        });
    }

    // No 버튼 눌렀을 때와 동일한 동작
    public void SelectNo()
    {
        if (!IsShowing) return;
        noButton.onClick.Invoke();//콜백 실행
        confirmPopup.SetActive(false);//팝업 닫기
    }
}