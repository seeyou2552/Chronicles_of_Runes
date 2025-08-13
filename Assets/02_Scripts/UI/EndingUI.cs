using _02_Scripts.Inventory;
using _02_Scripts.NPC;
using _02_Scripts.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private PlayableDirector endingTimeline;
    [SerializeField] private AchievementData easyData;
    [SerializeField] private AchievementData normalData;
    [SerializeField] private AchievementData hardData;
    [SerializeField] private GameObject nextBtn; // 점수보드 이후 넘어가는 용 버튼
    private ScoreBoardUI scoreBoardUI;
    private Image scoreBoardPanel; // 점수 보드 이미지

    private void Start()
    {
        scoreBoardUI = GameOverUI.Instance.scoreBoardUI;
        UIManager.Instance.baseUI.SetActive(false);
        PlayerController.Instance.canControl = false;
        StartEnding().Forget();
    }

    private async UniTask StartEnding()
    {
        await DialogueManager.Instance.StartDialogue("TutorialDialogue/EndingDialogue"); // Dialogue 끝날떄까지 대기
        StartCoroutine(ScoreBoard());
    }

    private IEnumerator ScoreBoard()
    {
        scoreBoardUI.gameObject.SetActive(true); // 점수보드 시작
        if (scoreBoardPanel == null) scoreBoardPanel = scoreBoardUI.GetComponent<Image>();
        scoreBoardPanel.DOFade(1f, 1f);

        yield return StartCoroutine(scoreBoardUI.OutputScore());
        nextBtn.SetActive(true);
    }

    public void TimelinePlay()
    {
        scoreBoardPanel.DOFade(0, 0.1f);
        scoreBoardUI.gameObject.SetActive(false);
        nextBtn.SetActive(false);
        endingTimeline.Play();
    }

    public void EndTimeline()
    {
        // 대충 점수 보드 띄우고 할듯

        // 난이도 컬랙션
        switch (GameManager.Instance.dungeonLevel)
        {
            case DungeonLevel.Easy:
                if (CollectorManager.Instance.achievDataDic.ContainsKey(easyData.displayName)) break; // 이미 있다면 break;
                CollectorManager.Instance.achievDataDic.Add(easyData.displayName, easyData);
                break;
            case DungeonLevel.Normal:
                if (CollectorManager.Instance.achievDataDic.ContainsKey(normalData.displayName)) break; // 이미 있다면 break;
                CollectorManager.Instance.achievDataDic.Add(normalData.displayName, normalData);
                break;
            case DungeonLevel.Hard:
                if (CollectorManager.Instance.achievDataDic.ContainsKey(hardData.displayName)) break; // 이미 있다면 break;
                CollectorManager.Instance.achievDataDic.Add(hardData.displayName, hardData);
                break;
        }

        // 빌리지 씬 전환 및 캐릭터 초기화
        PlayerController.Instance.statHandler.Resurrection();
        InventoryManager.Instance.AddItem(11);
        InventoryManager.Instance.Gold = 50;
        GameManager.Instance.ChangeState(GameState.Village);
    }
}
