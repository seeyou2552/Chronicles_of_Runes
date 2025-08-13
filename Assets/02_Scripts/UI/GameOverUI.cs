using _02_Scripts.Inventory;
using _02_Scripts.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : Singleton<GameOverUI>
{
    [SerializeField]
    private Image gameOverPanel;
    [SerializeField] private Image clock;
    [SerializeField] private GameObject clockHand;
    [SerializeField] private GameObject retryBtn;
    public ScoreBoardUI scoreBoardUI;
    private PlayerStatHandler playerStat;
    private Image scoreBoardPanel; // 점수 보드 이미지
    private float fadeDuration = 2f;

    void OnEnable()
    {
        EventBus.Subscribe(GameState.GameOver, GameOver);
    }

    void GameOver(object param)
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut() // 게임 오버 화면
    {
        if (BossHpbar.Instance.HPbar.activeSelf) BossHpbar.Instance.HPbar.SetActive(false); // 보스 HPBar가 켜져있다면 비활성화
        PlayerController.Instance.canControl = false; // 캐릭터 정지
        yield return YieldCache.WaitForSeconds(0.2f);
        gameOverPanel.gameObject.SetActive(true);
        UIManager.Instance.baseUI.SetActive(false);

        yield return YieldCache.WaitForSeconds(2f);
        gameOverPanel.DOFade(1f, fadeDuration);

        scoreBoardUI.gameObject.SetActive(true); // 점수보드 시작
        if (scoreBoardPanel == null) scoreBoardPanel = scoreBoardUI.GetComponent<Image>();
        scoreBoardPanel.DOFade(1f, 1f);
        yield return StartCoroutine(scoreBoardUI.OutputScore());

        retryBtn.SetActive(true);
    }

    async UniTaskVoid OnRetryButtonAsync() // 재시작 비동기 실행
    {
        retryBtn.SetActive(false);
        SoundManager.PlaySFX("Clocking");
        scoreBoardPanel.DOFade(0f, 0.1f);
        scoreBoardUI.gameObject.SetActive(false);
        clock.gameObject.SetActive(true);
        clock.DOFade(0.5f, 2f);
        clockHand.SetActive(true);

        await UniTask.Delay(5000);
        SoundManager.StopAllSFX(); // 사운드 종료
        GameManager.Instance.ChangeState(GameState.Village); // 씬 변경

        if (playerStat == null) playerStat = PlayerController.Instance.GetComponent<PlayerStatHandler>();
        playerStat.Resurrection();
        
        clock.DOFade(0, 0.5f);
        clock.gameObject.SetActive(false);
        clockHand.SetActive(false);
        // gameOverPanel.DOFade(0f, 1f);
        // gameOverPanel.gameObject.SetActive(false);
        UIManager.Instance.baseUI.SetActive(true);
        PlayerController.Instance.canControl = true;


        // stat.SetActive(true);
        // skillSlot.SetActive(true);
        // miniMap.SetActive(true);
    }

    public void OnRetryButton() // 재시작 버튼
    {
        OnRetryButtonAsync().Forget();
        InventoryManager.Instance.AddItem(11);
        InventoryManager.Instance.Gold = 50;
    }

}
