using _02_Scripts.Inventory;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState state { get; private set; }
    public GameState tempState { get; private set; }
    public DungeonLevel dungeonLevel { get; private set; }
    public event Action<GameState> OnStateChanged;

    public SceneFlowManager sceneFlowManager;

    [Header("Score")]
    public int totalDamage = 0; // 가한 누적 데미지
    public int totalDamageTaken = 0; // 받은 누적 데미지
    public int killCount = 0; // 처치 횟수
    public int runeGetCount = 0; // 룬 획득 횟수
    public int magicGetCount = 0; // 마법서 획득 횟수
    public int highScore { get; private set; }

    public bool IsTutorialComplete;
    public bool playing { get; private set; }

    [Header("난이도 Data")]
    [SerializeField] private DungeonLevelDataSo easy;
    [SerializeField] private DungeonLevelDataSo normal;
    [SerializeField] private DungeonLevelDataSo hard;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            state = GameState.MainMenu;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 이미 존재하는 경우 자신을 파괴
        }
        playing = true;
    }

    void Start()
    {
        // dungeonLevel = DungeonLevel.Hard;

        // 테스트용
        // CollectorManager.Instance.achievDataDic.Add(testData.displayName, testData);

        // ChangeState(GameState.Init);
        //SoundManager.PlayBGM("BGM11");
    }

    public async void ChangeState(GameState newState)
    {
        if (state == newState) return;
        tempState = state;
        state = newState;
        OnStateChanged?.Invoke(state);
        switch (state)
        {
            case GameState.Boss:
                sceneFlowManager.LoadBossScene().Forget();
                break;
            case GameState.Init:
                ChangeState(GameState.MainMenu);
                break;
            case GameState.MainMenu:
                PlayerController.Instance.statHandler.Resurrection();
                EventBus.Publish(GameState.MainMenu, null);
                dungeonLevel = DungeonLevel.Easy;
                highScore = 0;
                sceneFlowManager.LoadMainScene().Forget();
                break;
            case GameState.Synopsis:
                sceneFlowManager.LoadSynopsisScene().Forget();
                break;
            case GameState.Tutorial:
                sceneFlowManager.LoadVillageScene().Forget();
                break;
            case GameState.Village:
                sceneFlowManager.LoadVillageScene().Forget();
                break;
            case GameState.Dungeon:
                sceneFlowManager.LoadNextStageScene().Forget();
                break;
            case GameState.GameOver:
                await UniTask.Yield();
                EventBus.Publish(GameState.GameOver, null);
                break;
            case GameState.Ending:
                EventBus.Publish(GameState.Ending, null);
                sceneFlowManager.LoadEndingScene().Forget();
                break;
        }
    }

    public void Playing()
    {
        playing = true;
        Time.timeScale = 1f;

    }

    public void Pause()
    {
        playing = false;
        Time.timeScale = 0f;
    }

    public int ScoreCalculate() // 점수 정산
    {
        int score = (totalDamage)
                    - (totalDamageTaken) // 받은 데미지 만큼 -
                    + (killCount * 100) // killCount의 100배 +
                    + (runeGetCount * 10) // rune 획득 횟수의 10배
                    + (magicGetCount * 10) // 마법서 획득 횟수의 
                    + (InventoryManager.Instance.Gold * 10); // 소지 골드의 

        return score;
    }

    public void NewHighScore(int score) // highScore 갱신
    {
        highScore = score;
    }

    public void ResetScore() // highScore를 제외한 점수 초기화
    {
        totalDamage = 0;
        totalDamageTaken = 0;
        killCount = 0;
        runeGetCount = 0;
        magicGetCount = 0;
    }

    public void ChangeDungeonLevel(DungeonLevel newLevel)
    {
        if (dungeonLevel == newLevel) return;
        dungeonLevel = newLevel;

        switch (dungeonLevel)
        {
            case DungeonLevel.Easy:
                break;
            case DungeonLevel.Normal:
                break;
            case DungeonLevel.Hard:
                break;
        }
    }

    public void ChangeMonsterStat(EnemyStat enemy)
    {
        switch (dungeonLevel)
        {
            case DungeonLevel.Easy:
                ChangeStat(enemy, easy);
                break;
            case DungeonLevel.Normal:
                ChangeStat(enemy, normal);
                break;
            case DungeonLevel.Hard:
                ChangeStat(enemy, hard);
                break;
        }
    }

    private void ChangeStat(EnemyStat enemy, DungeonLevelDataSo changed)
    {
        enemy.maxHp *= changed.hp;
        enemy.curHp = enemy.maxHp;
        enemy.attackPower *= changed.power;
        enemy.speed *= changed.speed;
    }

}

