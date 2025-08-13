using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MagicBeam : MonoBehaviour, SkillAction
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
    public float rotationSpeed = 90f;
    private float currentAngle = 0f;
    public Vector3 tempScale;
    Vector3 startPos; // 시작 지점
    public bool isAttack;
    float timer;
    int layerMask; // 장애물 Layer
    float distance;
    Vector3 target; // 목표 지점
    private SpriteRenderer sr;
    private GameObject[] spheres;

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;
    [SerializeField] private GameObject homingBeamPrefab;


    public void Init(Vector3 pos)
    {
        layerMask = LayerMask.GetMask("Obstacle");
        timer = 0f;

        Vector3 dir = pos - transform.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        currentAngle = targetAngle;

        // float rad = currentAngle * Mathf.Deg2Rad;
        transform.rotation = Quaternion.AngleAxis(targetAngle - 90f, Vector3.forward);


        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        PlayerController.Instance.canControl = false;
        onStart?.Invoke();
        onMiddle?.Invoke();
        GetComponent<Animator>().SetInteger("Elemental", (int)elemental);

        if (tempScale.y <= transform.localScale.y) tempScale = transform.localScale;
        distance = tempScale.y / 5;

        if (!homing)
        {
            FloorEffect floor = GetComponent<FloorEffect>(); // floor 설절
            floor.damage = damage;
            floor.delay = 0.1f;
            floor.elemental = elemental;
            floor.soundController = soundController;
            floor.vfxController = vfxController;
            isAttack = true;
        }
        else StartCoroutine(HomingBeam());

        soundController.StartSound(elemental, objName); // 시작 사운드
    }

    void FixedUpdate()
    {
        if (isAttack) Beam();
    }

    void Beam()
    {
        if (isAttack && !homing) // 기본 호밍 x
        {
            if (transform.position == PlayerController.Instance.transform.position + PlayerController.Instance.transform.right)
            {
                startPos = PlayerController.Instance.transform.position + PlayerController.Instance.transform.right;
            }
            else
            {
                startPos = PlayerController.Instance.transform.position;
            }
            transform.position = startPos;
            if (timer >= duration - 0.5f) GetComponent<Animator>().SetInteger("Elemental", -1);

            if (!enemy)
            {
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target.z = 0f;
            }

            // 각도 계산 (목표 각도)
            Vector3 dir = (target - startPos).normalized;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // currentAngle을 targetAngle로 부드럽게 보간 (회전 속도 조절)
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

            // 막대기 회전
            transform.rotation = Quaternion.AngleAxis(currentAngle - 90f, Vector3.forward);
            Vector3 scale = transform.localScale;

            Debug.DrawRay(transform.position, transform.up * distance, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, distance, layerMask); // 벽에 닿을 경우 길이 조정
            if (hit.collider != null)
            {
                scale.y = hit.distance * 5;
            }
            transform.localScale = scale;

            timer += Time.deltaTime;

            if (timer >= duration) // pool 반환
            {
                SoundManager.StopAllSFX();
                isAttack = false;
                onEnd?.Invoke();
                PlayerController.Instance.canControl = true;
                if (!enemy) ObjectPoolManager.Instance.Return(this.gameObject, objName);
                else if (enemy) ObjectPoolManager.Instance.Return(this.gameObject, "EnemyMagicBeam");
            }
        }
    }

    private IEnumerator HomingBeam()
    {
        if (sr == null) sr = gameObject.GetComponent<SpriteRenderer>();
        var sColor = sr.color;
        sColor.a = 0f;
        sr.color = sColor;

        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(homingBeamPrefab.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(homingBeamPrefab.name, homingBeamPrefab, 10);
        }

        float timer = 0;
        int i = 0;
        spheres = new GameObject[Mathf.CeilToInt(duration / 0.1f * 2)];

        while (timer < duration)
        {
            spheres[i] = ObjectPoolManager.Instance.Get(homingBeamPrefab.name);

            HomingBeam ball = spheres[i].GetComponent<HomingBeam>(); // 설정 주입
            ball.soundController = soundController;
            ball.vfxController = vfxController;
            ball.enemy = false;
            ball.onHit = onHit;
            ball.objName = homingBeamPrefab.name;
            ball.duration = duration;
            ball.through = through;
            ball.elemental = elemental;
            ball.homing = homing;
            ball.speed = speed;
            ball.damage = damage;
            var x = ball.transform.localScale.x;
            x = transform.localScale.y / 36;
            var y = ball.transform.localScale.y;
            y = transform.localScale.x / 6;

            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector2 dir = (mousePos - PlayerController.Instance.transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            ball.transform.rotation = Quaternion.Euler(0, 0, angle);
            ball.transform.position = transform.position;

            ball.Init();

            yield return YieldCache.WaitForSeconds(0.1f);
            timer += 0.1f;
            i++;
        }

        // 리턴
        SoundManager.StopAllSFX();
        isAttack = false;
        onEnd?.Invoke();
        PlayerController.Instance.canControl = true;
        sColor.a = 1f;
        sr.color = sColor;
        if (!enemy) ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    void OnDisable()
    {
        transform.localScale = tempScale;
    }

    public void EnemyInit()
    {
        target = PlayerController.Instance.transform.position;
        startPos = transform.position;
        FloorEffect floor = GetComponent<FloorEffect>();
        floor.damage = damage;
        floor.delay = 0.1f;
        floor.enemy = true;
    }

}
