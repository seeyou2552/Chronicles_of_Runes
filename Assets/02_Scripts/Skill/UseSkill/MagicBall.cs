using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using DG.Tweening;



public class MagicBall : MonoBehaviour, SkillAction
{
    public Action onStart { get; set; }
    public Action onMiddle { get; set; }
    public Action onHit { get; set; }
    public Action onEnd { get; set; }
    public Action onEnemy { get; set; }
    public bool through { get; set; }
    public bool homing { get; set; }
    public float duration { get; set; }
    public ElementalType elemental { get; set; }
    public float damage { get; set; }
    public bool enemy { get; set; }
    public GameObject caster { get; set; }
    public string objName { get; set; }

    public float speed { get; set; }

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;



    [Header("테스트용")]
    public bool isEnemy;
    public float isdamage;
    public Collider2D testCol;


    private CancellationTokenSource cts;

    public string originalName;
    private float tempSpeed;
    private void Awake()
    {
        tempSpeed = speed;

        testCol = GetComponent<Collider2D>();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        EventBus.Subscribe(EventType.BossSpawnCinematic, ReturnToPool);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(EventType.BossSpawnCinematic, ReturnToPool);
    }
    private void OnEnable()
    {
        //elemental = ElementalType.Normal;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        StartScaleLoop();


    }

    void OnDisable()
    {
        testCol.enabled = false;
        StopScaleLoop();


    }

    public void OnSceneUnloaded(Scene scene)
    {
        ObjectPoolManager.Instance.Return(gameObject, originalName);
        
    }

    public void Init(Vector3 pos)
    {
        isEnemy = enemy;
        isdamage = damage;
        onStart?.Invoke();

        testCol.enabled = true; // 이게 맞나


        // 마우스 방향 벡터 계산 및 정규화
        Vector2 direction = (pos - PlayerController.Instance.transform.position).normalized;

        // 방향 벡터로 각도 계산 (라디안 → 도 단위)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 오브젝트 회전 설정 (Z축 회전)
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // // Rigidbody2D에 방향 * 속도로 속도 설정 → 발사
        // rb.velocity = direction * speed;

        onMiddle?.Invoke();
        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);

        soundController.StartSound(elemental, objName); // 시작 사운드

        StartCoroutine(Return(objName));
        testCol.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy) // Enemy 적중
        {
            onHit?.Invoke();
            soundController.HitSound(elemental, objName); // Hit Sound
            onEnd?.Invoke();
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            collision.GetComponent<IDamageable>().OnDamage(damage);
            if (collision.TryGetComponent<DebuffController>(out DebuffController debuffController))
            {
                debuffController.TakeDebuff(collision, elemental);
            }
            if (!through)
            {
                cts?.Cancel();
                cts?.Dispose();
                ObjectPoolManager.Instance.Return(this.gameObject, originalName);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy) // Player 적중
        {
            collision.GetComponent<IDamageable>().OnDamage(damage);
            ObjectPoolManager.Instance.Return(this.gameObject, originalName);
            vfxController.GetVFX(elemental, transform.rotation, gameObject.transform.position);
            // soundController.HitElementalSound(elemental, objName); // Hit Sound
        }
    }

    public IEnumerator Return(string name)
    {
        yield return YieldCache.WaitForSeconds(duration);
        GetComponent<SpriteRenderer>().sprite = null;

        elemental = ElementalType.Normal;

        speed = tempSpeed;

        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    void ReturnToPool()
    {
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
        GetComponent<SpriteRenderer>().sprite = null;
    }

    void ReturnToPool(object obj)
    {
        if (!gameObject.activeInHierarchy) return;
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
        GetComponent<SpriteRenderer>().sprite = null;
    }



    public void EnemyInit()
    {
        isEnemy = enemy;
        Logger.Log(damage.ToString() + "데미지");
        isdamage = damage;
        duration = 5f;

        /*
        Vector2 direction = (PlayerSkillManager.Instance.transform.position - transform.position).normalized;

        // 방향 벡터로 각도 계산 (라디안 → 도 단위)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 오브젝트 회전 설정 (Z축 회전)
        transform.rotation = Quaternion.Euler(0, 0, angle);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D에 방향 * 속도로 속도 설정 → 발사
        rb.velocity = direction * speed;
        */
        onEnemy?.Invoke();
        // GetComponent<Animator>().SetInteger("Elemental", (int)elemental);
        GetComponent<Animator>().Play(elemental.ToString() + "Ball");

        StartCoroutine(Return(objName));
        testCol.enabled = true;
    }

    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    public void GatlingInit()
    {
        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);
        testCol.enabled = true; // 이게 맞나

        soundController.StartSound(elemental, objName); // 시작 사운드

        if (homing)
        {
            GameObject[] GetEnemiesByPhysics()
            {
                // 현재 오브젝트 위치를 중심으로 반경 searchRadius 내의 Collider2D를 레이어 마스크로 검색
                Collider2D[] hits = Physics2D.OverlapCircleAll(gameObject.transform.position, 200f, LayerMask.GetMask("Enemy"));
                return hits
                    .Select(c => c.gameObject)
                    .Distinct()  // 중복 제거
                    .ToArray();
            }
            var enemies = GetEnemiesByPhysics();
            Transform closest = null;
            float minDist = Mathf.Infinity;
            Vector3 origin = PlayerController.Instance.transform.position;

            foreach (var enemy in enemies)
            {
                float dist = Vector3.Distance(origin, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy.transform;
                }
            }
            var h = GetComponent<HomingProjectile>();
            h.SetTarget(closest);
        }

        StartCoroutine(Return(objName));
    }


    private Tween scaleTween;
    public void StartScaleLoop()
    {
        // 시작 스케일 설정 (0.9로 시작)
        transform.localScale = Vector3.one * 0.9f;

        // 트윈 시작
        scaleTween = transform.DOScale(1.1f, 1.5f)
                              .SetLoops(-1, LoopType.Yoyo)
                              .SetEase(Ease.InOutSine);
    }

    public void StopScaleLoop()
    {
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
            scaleTween = null;
        }
    }
}