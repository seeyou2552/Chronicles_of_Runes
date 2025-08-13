#if UNITY_EDITOR
using UnityEditor;
#endif
using _02_Scripts.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using _02_Scripts;

[System.Serializable]
public class SceneReference : ISerializationCallbackReceiver // 씬 string 변환
{
#if UNITY_EDITOR
    public SceneAsset sceneAsset;
#endif

    [SerializeField] private string sceneName;
    public string SceneName => sceneName;

    public string sceneBGM;
    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
            sceneName = sceneAsset.name;
#endif
    }

    public void OnAfterDeserialize() { }
}

public class SceneFlowManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private List<SceneReference> stageScenes; // 스테이지

    [SerializeField] private SceneReference mainScene; // 메인
    [SerializeField] private SceneReference synopsisScene; // 시놉시스
    [SerializeField] private SceneReference bossScene; // 보스
    [SerializeField] private SceneReference villageScene; // 마을
    [SerializeField] private SceneReference endingScene; // 엔딩
    [SerializeField] private Image loadingBar; // 로딩바

    [Header("Player")]

    public int currentStageIndex = 0;
    public EnemySpawnManager enemySpawnManager;


    private void OnEnable()
    {
        EventBus.Subscribe(EventType.BossSpawnCinematic, PlayBossBGM);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.BossSpawnCinematic, PlayBossBGM);
    }

    private void Awake()
    {
        //타이틀기준
        if (SceneManager.GetActiveScene().name == mainScene.SceneName)
        {
            SoundManager.PlayBGM(mainScene.sceneBGM);
        }
    }
    public async UniTask LoadNextStageScene()
    {
        if (currentStageIndex == stageScenes.Count) // 더 이상 스테이지 없으면 ending으로
        {
            GameManager.Instance.ChangeState(GameState.Ending);
            return;
        }

        CameraManager.Instance.mainCamera.gameObject.SetActive(true);
        if (loadingBar == null) loadingBar = StaticPopupManager.Instance.GetComponent<LoadingUI>().loadingBar;
        
        // 진행사항 저장

        EventBus.Publish(LoadState.LoadStart, null); // 로딩 화면 시작

        PlayerController.Instance.GetComponent<Collider2D>().enabled = false;
        await UniTask.Delay(700);
        
        UIManager.Instance.ClearAllUI();

        currentStageIndex += 1;
        
        var op = SceneManager.LoadSceneAsync(stageScenes[currentStageIndex - 1].SceneName);
        // op.allowSceneActivation = false;

        while (!op.isDone)
        {
            // progress: 0~0.9 사이, allowSceneActivation=false 시 0.9에 머뭄
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingBar != null) loadingBar.fillAmount = progress;

            await UniTask.Yield();
        }

        await UniTask.Delay(500);
        if (loadingBar != null) loadingBar.fillAmount = 1f;
        EventBus.Publish(LoadState.LoadComplete, null); // 로딩 화면 끝
        PlayerController.Instance.moveInput = Vector2.zero;
        PlayerController.Instance.GetComponent<Collider2D>().enabled = true;
        SoundManager.PlayBGM(stageScenes[currentStageIndex -1].sceneBGM);
    }

    public async UniTask LoadMainScene()
    {
        // await UniTask.Delay(1000);
        // 진행사항 저장
        EventBus.Publish(LoadState.LoadStart, null); // 로딩 화면 시작

        await UniTask.Delay(1000);

        UIManager.Instance.ClearAllUI();
        var op = SceneManager.LoadSceneAsync(mainScene.SceneName);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            await UniTask.Yield();
        }

        EventBus.Publish(LoadState.LoadComplete, null); // 로딩 화면 끝
        CameraManager.Instance.mainCamera.gameObject.SetActive(false);
        SoundManager.PlayBGM(mainScene.sceneBGM);
    }

    public async UniTask LoadBossScene()
    {
        // 진행사항 저장
        CameraManager.Instance.mainCamera.gameObject.SetActive(true);
        EventBus.Publish(LoadState.LoadStart, null); // 로딩 화면 시작

        await UniTask.Delay(1000);

        UIManager.Instance.ClearAllUI();
        var op = SceneManager.LoadSceneAsync(bossScene.SceneName);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingBar != null) loadingBar.fillAmount = progress;

            await UniTask.Yield();
        }

        EventBus.Publish(LoadState.LoadComplete, null); // 로딩 화면 끝
        PlayerController.Instance.moveInput = Vector2.zero;
        if (loadingBar != null) loadingBar.fillAmount = 1f;
        SoundManager.Instance.Preload("BossBGM");
        SoundManager.PlayBGM(bossScene.sceneBGM);
    }


    public async UniTask LoadVillageScene()
    {
        if (loadingBar == null) loadingBar = StaticPopupManager.Instance.GetComponent<LoadingUI>().loadingBar;
        CameraManager.Instance.mainCamera.gameObject.SetActive(true);
        currentStageIndex = 0;
        GameManager.Instance.ResetScore(); // 스코어 초기화

        EventBus.Publish(LoadState.LoadStart, null); // 로딩 화면 시작

        await UniTask.Delay(1000);
        UIManager.Instance.ClearAllUI();
        var op = SceneManager.LoadSceneAsync(villageScene.SceneName);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingBar != null) loadingBar.fillAmount = progress;

            await UniTask.Yield();
        }

        PlayerController.Instance.gameObject.transform.position = new Vector2(-10, 12);
        await UniTask.Delay(2000);
        if (loadingBar != null) loadingBar.fillAmount = 1f;
        
        EventBus.Publish(LoadState.LoadComplete, null); // 로딩 화면 끝
        PlayerController.Instance.moveInput = Vector2.zero;
        SoundManager.PlayBGM(villageScene.sceneBGM);
    }

    public async UniTask LoadEndingScene()
    {
        currentStageIndex = 0;
        // 여기도 저장 필요
        // 인벤토리 데이터 및 플레이어 스탯 초기화

        EventBus.Publish(LoadState.LoadStart, null); // 로딩 화면 시작

        await UniTask.Delay(1000);
        UIManager.Instance.ClearAllUI();
        var op = SceneManager.LoadSceneAsync(endingScene.SceneName);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            await UniTask.Yield();
        }

        EventBus.Publish(LoadState.LoadComplete, null); // 로딩 화면 끝
        SoundManager.PlayBGM(endingScene.sceneBGM);
    }
    
    public async UniTask LoadSynopsisScene()
    {
        CameraManager.Instance.mainCamera.gameObject.SetActive(true);
        await UniTask.Delay(1000);
        var op = SceneManager.LoadSceneAsync(synopsisScene.SceneName);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            await UniTask.Yield();
        }
        SoundManager.PlayBGM(synopsisScene.sceneBGM);
    }

    public int GetCurrentStage()
    {
        return currentStageIndex;
    }

    public void PlayBossBGM(object obj)
    {
        SoundManager.PlayBGM("BossBGM");
    }
}
