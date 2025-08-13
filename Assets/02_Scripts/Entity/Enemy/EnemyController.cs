using _02_Scripts.Enetity.Enemy;
using _02_Scripts.UI;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Pathfinding;
using SmallScaleInc.TopDownPixelCharactersPack1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour, IDamageable, IPoolObject
{

    [Header("몬스터가 사용할 상태")]
    public PatternDataSo idelState_So;
    public PatternDataSo attackState_So;
    public PatternDataSo moveState_So;

    [Header("사용할 패턴 리스트")]
    public List<PatternDataSo> attackPatternList;
    public List<PatternDataSo> movePatternList;
    //public List<PatternDataSo> carrierPatternList;

    private StateFactory stateFactory;
    public Dictionary<PatternDataSo, EnemyState> FSM = new();  // 상태 캐싱용 (옵션)

    [HideInInspector] public List<PatternDataSo> attackCachedPatternTypes = new();     // SO 순서대로 타입 캐싱 //가중치에 따른 인덱스로 뽑아내기 위한 캐싱
    [HideInInspector] public List<int> attackPatternWeights = new();          // SO 순서대로 가중치

    [HideInInspector] public List<PatternDataSo> moveCachedPatternTypes = new();
    [HideInInspector] public List<int> movePatternWeights = new();

    [HideInInspector] public List<PatternDataSo> carrierCachedPatternTypes = new();
    [HideInInspector] public List<int> carrierPatternWeights = new();

    public StateMachine stateMachine;

    [Header("디버그 용도")]
    [SerializeField] private string currentStateName;

    [SerializeField]
    public EnemyStat enemyStat;
    public Transform target;

    public float distance;
    private EnemyDropHandler enemyDropHandler;


    public Transform targetPos; //이동 위치 트랜스폼
    [HideInInspector] public AIDestinationSetter setter;
    [HideInInspector] public AIPath aiPath;
    [HideInInspector] public Seeker seeker;

    public LayerMask targetLayer;

    public Transform room;
    public Collider2D roomCollider;

    [HideInInspector] public EnemyState idleState;
    [HideInInspector] public EnemyState moveState;
    [HideInInspector] public EnemyState attackState;

    public EnemyAnimator animationController;

    public bool isChasing = false;
    float invincibilityDelay = 1f;
    float invincibilityTime;
    public bool isDead = false;
    public bool isAniOnUpdate = true;

    public PlayerStatUIHandler statUIHandler;

    public DoubleBoss doubleBoss;

    public CancellationTokenSource cts;

    public bool canControl = true;

    public List<Tween> tweens = new List<Tween>();

    private ItemManager.DropSystem dropSystem;

    SpriteRenderer sr;
    Color c;
    Color hitColor;

    public float spawnTime = 0.5f;

    private void OnEnable()
    {
        cts = new CancellationTokenSource();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        aiPath.canMove = false;
        EventBus.Subscribe(GameState.GameOver, ReturnPool);


        PlayPortalEffect();

        isChasing = true;
        
        
        /*
        // 초기 상태 설정
        transform.localScale = Vector3.zero;
        Color c = Color.white;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        // 1. 스케일 튀어나오기
        transform.DOScale(originalScale, spawnTime)
            .SetEase(Ease.OutBack);

        // 2. 살짝 회전
        transform.DORotate(new Vector3(0, 0, 20f), spawnTime / 2)
            .SetLoops(2, LoopType.Yoyo);

        // 3. 알파 페이드 인
        if (sr != null)
            sr.DOFade(1f, spawnTime * 0.8f);

        // 4. 착지 느낌 주기 (조금 내려가면서 흔들림)
        transform.DOPunchPosition(Vector3.down * 0.2f, 0.3f, 5, 1);
        */

    }


    void OnDisable()
    {
        cts?.Cancel(); // 비동기 작업 취소
        cts?.Dispose();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        EventBus.Unsubscribe(GameState.GameOver, ReturnPool);
    }


    public void OnSceneUnloaded(Scene scene)
    {
        ReturnPool(null);
    }

    //반납 타이밍 호출
    private void StateInit()
    {
        //레이어 초기화
        gameObject.layer = 7;

        isChasing = false;

        c = sr.color;
        c.a = 1f;
        sr.color = c;

        animationController.animator.enabled = true;
        animationController.animator.speed = 4f;
        animationController.isAttack = false;
        animationController.animator.Rebind();
        animationController.animator.Update(0f);
        stateMachine.Initialize(idleState, this);

    }



    //풀에서 네임만 초기화
    public void Init(string name)
    {
        originalName = name;
    }
    //DataSo 참조해서 스탯 가져오는 용도
    public EnemyStat ConvertSoToStat(EnemyDataSo so)
    {

        enemyStat = new EnemyStat();
        enemyStat.enemyName = so.enemyName;
        enemyStat.displayName = so.displayName;
        enemyStat.isBoss = so.isBoss;
        enemyStat.maxHp = so.hp;
        enemyStat.curHp = so.hp;
        enemyStat.speed = so.speed;
        enemyStat.safeDistance = so.safeDistance;
        enemyStat.enemyPrefab = so.enemyPrefab;
        enemyStat.chaseRange = so.chaseRange;
        enemyStat.attackPower = so.attackPower;
        enemyStat.attackRange = so.attackRange;
        enemyStat.dropTable = new List<DropEntry>(so.dropTable);
        enemyStat.NoDropWeight = so.NoDropWeight;

        if (enemyStat.isBoss)
        {
            isChasing = false;
            BossHpbar.Instance.HPbar.SetActive(true);
            statUIHandler.HPSlider = BossHpbar.Instance.BgSlider;
            statUIHandler.BossSlider = BossHpbar.Instance.HpSlider;
            BossHpbar.Instance.nameText.text = enemyStat.displayName;

            if (doubleBoss == null)
            {
                statUIHandler.ChangeBossHP(enemyStat.curHp, enemyStat.maxHp);
                
            }
            else if (doubleBoss != null)
            {
                statUIHandler.ChangeBossHP(enemyStat.maxHp + doubleBoss.otherEnemyController.enemyStat.maxHp, enemyStat.maxHp + doubleBoss.otherEnemyController.enemyStat.maxHp);
                /*
                if (doubleBoss.isSecond == true)
                {
                    Logger.Log((enemyStat.maxHp + doubleBoss.otherEnemyController.enemyStat.maxHp).ToString()+ " + " + (enemyStat.maxHp + doubleBoss.otherEnemyController.enemyStat.maxHp).ToString());
                    statUIHandler.ChangeBossHP(enemyStat.maxHp + doubleBoss.otherEnemyController.enemyStat.maxHp, enemyStat.maxHp + doubleBoss.otherEnemyController.enemyStat.maxHp);
                }
                */
            }

        }
        aiPath.maxSpeed = enemyStat.speed;
        GameObject go = ObjectPoolManager.Instance.Get("TargetPos");
        targetPos = go.transform;
        animationController.isAttack = false;
        isDead = false;
        gameObject.layer = 7;

        return enemyStat;
    }


    Vector3 originalScale;
    private void Awake()
    {
        isDead = true;
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        setter = GetComponent<AIDestinationSetter>();
        animationController = GetComponent<EnemyAnimator>();
        enemyDropHandler = GetComponent<EnemyDropHandler>();

        aiPath = GetComponent<AIPath>();

        hitColor = new Color(1, 0.5f, 0.5f);
        targetLayer = LayerMask.GetMask("Player");

        doubleBoss = GetComponent<DoubleBoss>();
        dropSystem = new ItemManager.DropSystem(ItemManager.Instance.monsterDropTable);
    }

    string originalName;
    void Start()
    {
        target = PlayerController.Instance.transform;

        if (setter != null)
        {
            setter.target = target;
        }

        stateMachine = new StateMachine();
        stateFactory = new StateFactory(this, stateMachine);

        InitFSM();

        stateMachine.Initialize(idleState, this);
    }

    void Update()
    {
        if (isDead) return;

        //즉시반응용도
        if (canControl == false)
        {//한번만
            if (stateMachine.currentState != idleState)
            {
                stateMachine.ChangeState(idleState);
            }
        }

        //그냥쓸거면 제곱근 해야함
        if (target != null)
        {
            distance = (target.position - transform.position).sqrMagnitude;
            if (isChasing == false)
            {
                //정확함보다는 빠름을 위해 사용
                if ((target.position - transform.position).sqrMagnitude < enemyStat.chaseRange * enemyStat.chaseRange)
                {
                    isChasing = true;
                    setter.target = target;

                }
            }
        }

        if (isChasing)
        {
            stateMachine.Update();
            currentStateName = stateMachine.CurrentStateName;
        }
        if (isAniOnUpdate)
        {
            animationController.AniUpdate();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyStat.chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyStat.attackRange);
    }
#endif


    //리스트에 들어있는 패턴 초기화
    public void InitFSM()
    {
        idleState = stateFactory.Create(idelState_So);
        FSM[idelState_So] = idleState;
        carrierCachedPatternTypes.Add(idelState_So);
        carrierPatternWeights.Add(idelState_So.weight);

        attackState = stateFactory.Create(attackState_So);
        FSM[attackState_So] = attackState;
        carrierCachedPatternTypes.Add(attackState_So);
        carrierPatternWeights.Add(attackState_So.weight);

        moveState = stateFactory.Create(moveState_So);
        FSM[moveState_So] = moveState;
        carrierCachedPatternTypes.Add(moveState_So);
        carrierPatternWeights.Add(moveState_So.weight);

        foreach (var pattern in attackPatternList)
        {
            var state = stateFactory.Create(pattern);
            FSM[pattern] = state;
            attackCachedPatternTypes.Add(pattern);
            attackPatternWeights.Add(pattern.weight); // 가중치는 SO에서 가져옴
        }

        foreach (var pattern in movePatternList)
        {
            var state = stateFactory.Create(pattern);
            FSM[pattern] = state;
            moveCachedPatternTypes.Add(pattern);
            movePatternWeights.Add(pattern.weight); // 가중치는 SO에서 가져옴
        }
    }

    private System.Random rng = new System.Random();

    //상태전환시 가중치 참고용
    public int WeightedRandomIndex(List<int> weights)
    {
        if (weights == null || weights.Count == 0)
            return -1;

        int total = 0;
        foreach (int w in weights) total += w;

        if (total <= 0)
            return -1;

        int rand = Random.Range(0, total);

        int sum = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            sum += weights[i];
            if (rand < sum)
                return i;
        }

        return -1;
    }

    public void OnDamage(float value)
    {
        if (isDead) return;

        if (invincibilityTime > Time.time)
        {
            return;
        }
        else
        {
            if (isChasing == false)
            {
                isChasing = true;
            }
            GameManager.Instance.totalDamage += (int)value; // 가한 누적 데미지 저장
            enemyStat.curHp -= value;
            //일단 무적은 스킬쪽에서 판단
            //invincibilityTime = Time.time + invincibilityDelay;

            DamageFontManager.Instance.ShowDamage(transform.position, value);
            if (enemyStat.isBoss)
            {
                if (doubleBoss == null)
                {
                    statUIHandler.ChangeBossHP(enemyStat.curHp, enemyStat.maxHp);
                }
                else
                {
                    statUIHandler.ChangeBossHP(enemyStat.curHp + doubleBoss.otherEnemyController.enemyStat.curHp, enemyStat.maxHp + doubleBoss.otherEnemyController.enemyStat.maxHp);
                }
            }
        }

        if (enemyStat.curHp <= 0)
        {
            isDead = true;
            GameManager.Instance.killCount++; // 몬스터 처치 횟수 저장

            if (tweens.Count > 0)
            {
                foreach (var t in tweens)
                {
                    t.Kill();
                }
                tweens.Clear();

            }

            if (enemyStat.isBoss)
            {
                if (doubleBoss == null)
                {
                    EventBus.Publish(EventType.BossDead, null);
                    EventBus.Publish("MonsterDead", this.gameObject);
                }
                else
                {
                    //다른 보스도 사망시
                    if (doubleBoss.otherEnemyController.isDead == true)
                    {
                        EventBus.Publish(EventType.BossDead, null);
                        EventBus.Publish("MonsterDead", this.gameObject);
                    }
                }
            }
            else
            {
                EventBus.Publish("MonsterDead", this.gameObject);
            }


            //드랍하는 부분
            List<ItemData> dropItems = GetDropsFromTable();
            if (dropItems != null && dropItems.Count > 0)
            {
                foreach (var item in dropItems)
                {
                    if (item == null) continue;
                    enemyDropHandler.SpawnDropItems(item);
                }
            }

            gameObject.layer = 0;

            aiPath.canMove = false;
            animationController.animator.SetTrigger("Die");
            StartCoroutine(PauseAnimatorAtEnd());
            return;
        }
        StartCoroutine(HitEffect());
    }

    public ItemData GetRandomDrop()
    {
        //위에서 캐싱하는편이 좋을거같습니다
        float rand;
        List<ItemData> itemInfo = new List<ItemData>();

        //드랍테이블이 존재할때만 동작
        if (enemyStat.dropTable.Count != 0)
        {
            //드랍테이블의 모든아이템 순회
            foreach (var entry in enemyStat.dropTable)
            {
                //각 아이템마다 독립적으로 확률 검사
                rand = Random.Range(0f, 100f);
                if (rand <= entry.dropChance)
                {
                    //드롭확정 리스트에 추가
                    itemInfo.Add(entry.itemData);
                }
            }

            for (int i = 0; i < enemyStat.NoDropWeight; i++)
            {
                //드랍 확정된 리스트에 드랍이 안되는 가중치 추가
                itemInfo.Add(null);
            }

            int DropItemIndex = Random.Range(0, itemInfo.Count);
            if (itemInfo.Count > DropItemIndex)
            {
                return itemInfo[DropItemIndex];
            }
        }
        else
        {
            return null;
        }

        return null;
    }


    //사망 애니메이션용도
    private IEnumerator PauseAnimatorAtEnd()
    {
        Animator animator = animationController.animator;
        animator.speed = 1.5f;
        // 일단 1프레임 대기: GetCurrentAnimatorStateInfo는 재생 요청 직후엔 반영 안 됨
        yield return null;

        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 대충 마지막 프레임정도
            if (stateInfo.normalizedTime >= 0.85f)
            {

                // 애니메이션 정지
                animator.speed = 0f;
                animationController.animator.enabled = false;
                break;


            }

            yield return null;
        }

        float alpha = sr.color.a;
        DoTweenExtensions.TweenFloat(alpha, 0f, 2f, (alpha) =>
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        },
        async () =>
        {
            await UniTask.Delay(2000);
            StateInit();
            //ObjectPoolManager.Instance.Return(targetPos.gameObject, "TargetPos");
            ReturnPool(null);
            if (enemyStat.isBoss)
            {
                if (doubleBoss == null)
                {
                    BossHpbar.Instance.HPbar.SetActive(false);
                }

                else
                {
                    if (doubleBoss.otherEnemyController.isDead == true)
                    {
                        BossHpbar.Instance.HPbar.SetActive(false);
                    }
                }
            }
        }

        );
    }


    public Action<Collider2D> ontriggerEntered;
    private void OnTriggerEnter2D(Collider2D collision)
    {

        ontriggerEntered?.Invoke(collision);

    }


    //애니메이션을 공격 딜레이에 맞게 길이 조절하는곳
    public async UniTask AnimationPlay(string aniName, float duration, Action onComplete = null)
    {
        Animator animator = animationController.animator;

        // 트리거 설정
        animator.SetTrigger(aniName);

        // 상태 전환 대기 (최대 0.5초까지 기다림)
        AnimatorClipInfo[] clipInfos = null;
        int retry = 0;
        while (retry < 30)
        {
            await UniTask.Yield(); // 한 프레임 대기

            clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfos.Length > 0 && clipInfos[0].clip != null)
                break;

            retry++;
        }

        if (clipInfos == null || clipInfos.Length == 0 || clipInfos[0].clip == null)
        {
            onComplete?.Invoke();
            return;
        }

        AnimationClip clip = clipInfos[0].clip;
        float clipLength = clip.length;

        float originalSpeed = animator.speed;

        // 애니메이션을 원하는 duration 시간 안에 끝내도록 속도 조정
        animator.speed = clipLength / duration;

        await UniTask.Delay((int)(duration * 1000));

        // 원래 속도로 복원
        animator.speed = originalSpeed;


        onComplete?.Invoke();
    }

    public void ReturnPool(object obj)
    {
        ObjectPoolManager.Instance.Return(gameObject, originalName);

        if (targetPos != null)
            ObjectPoolManager.Instance.Return(targetPos.gameObject, "TargetPos");
    }

    public List<ItemData> GetDropsFromTable()
    {
        if (dropSystem == null)
        {
            return null;
        }

        int dropGold;
        List<int> dropItemIds = dropSystem.GetDropItems(enemyStat.enemyName, out dropGold);

        // 골드 추가
        if (dropGold > 0)
        {
            enemyDropHandler.SpawnDropGold(dropGold);
        }

        List<ItemData> dropItems = new();
        foreach (var itemId in dropItemIds)
        {
            var item = ItemManager.Instance.GetItem(itemId);
            if (item != null)
            {
                dropItems.Add(item);
            }
        }

        return dropItems;
    }

    public void KillTweens()
    {
        if (tweens.Count > 0)
        {
            foreach (var t in tweens)
            {
                t.Kill();
            }
            //tweens.Clear();
            tweens = new List<Tween>();

        }
    }


    public IEnumerator DelayOneFrame(Action callback)
    {
        yield return null;
        callback?.Invoke();
    }




    ///////////스폰 연출/////////////////////

    private SpriteRenderer[] renderers;

    [Header("Drop Settings")]
    public float dropHeight = 3f;
    public float dropDuration = 0.3f;

    [Header("Portal Settings")]
    public float portalScaleTime = 0.3f;
    public float portalPunchScale = 0.2f;


    public void PlayPortalEffect()
    {
        transform.localScale = Vector3.zero;
        SetAlpha(0f);

        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale, portalScaleTime).SetEase(Ease.OutBack))
           .Join(DOTween.To(() => 0f, x => SetAlpha(x), 1f, portalScaleTime))
           .Append(transform.DOPunchScale(Vector3.one * portalPunchScale, 0.15f, 5, 1f));
    }

    //흠... 이게 왜 여기에...
    void SetAlpha(float alpha)
    {
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    private IEnumerator HitEffect()
    {
        sr.color = hitColor;
        yield return YieldCache.WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

}

