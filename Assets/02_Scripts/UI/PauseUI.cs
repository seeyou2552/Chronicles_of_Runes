using _02_Scripts.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PauseUI : MonoBehaviour, IUIInterface
{
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject settingUI;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private TextMeshProUGUI choiceText;
    [SerializeField] private GameObject collectUI;
    private Action executeAction;

    public void OnTogglePaused()
    {
        
        if (GameManager.Instance.playing)
            {
                OpenUI();
            }
            else if (!GameManager.Instance.playing)
            {
                CloseUI();
            }
            else
            {
                GameManager.Instance.Pause();
                pauseUI.SetActive(true);
            }
    }

    public void OpenUI()
    {
        if (GameManager.Instance.state == GameState.MainMenu || GameManager.Instance.state == GameState.Ending) return;
        
        if (!UIManager.Instance.RegisterUI(this))
        {
            return;
        }
        GameManager.Instance.Pause();
        pauseUI.SetActive(true);
    }

    public void CloseUI()
    {
        if (!UIManager.Instance.UnRegisterUI(this))
        {
            return;
        }
        pauseUI.SetActive(false);
        choicePanel.SetActive(false);
        GameManager.Instance.Playing();
    }

    public void OnCollect()
    {
        //PauseUI 닫기
        UIManager.Instance.UnRegisterUI(this);
        pauseUI.SetActive(false);

        // CollectorUI 열기
        var collector = collectUI.GetComponent<CollectorUI>();
        collector.OpenUI();
        CloseUI();
    }

    public void OnSetting()
    {
        //PauseUI 닫기
        UIManager.Instance.UnRegisterUI(this);
        pauseUI.SetActive(false);

        //SettingUi 열기
        var setting = settingUI.GetComponent<SettingUI>();
        setting.OpenUI();
        CloseUI();
    }
    public void OnRetry() // 회차 재시작
    {
        if (GameManager.Instance.state == GameState.Village || GameManager.Instance.state == GameState.Tutorial)
        {
            StaticNoticeManager.Instance.ShowSideNotice("이미 마을에 있습니다.", 1f);
            return;
        }
        
        executeAction = () =>
        {
            choicePanel.SetActive(false);
            pauseUI.SetActive(false);
            GameManager.Instance.Playing();
            GameManager.Instance.ChangeState(GameState.GameOver);
        };

        choiceText.text = "Really Retry?";
        choicePanel.SetActive(true);
    }

    public void OnMain() // 메인으로
    {
        executeAction = () =>
        {
            choicePanel.SetActive(false);
            pauseUI.SetActive(false);
            GameManager.Instance.Playing();
            GameManager.Instance.ChangeState(GameState.MainMenu);
        };

        choiceText.text = "Really Exit?";
        choicePanel.SetActive(true);
    }

    public void OnHideChoice() // 취소
    {
        choicePanel.SetActive(false);
    }

    public void OnChoiceExecute() // Yes 실행
    {
        executeAction?.Invoke();
    }



}
