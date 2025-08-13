using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MagicGatling : MonoBehaviour, SkillAction
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

    [Header("Obj")]
    public GameObject gatlingObj;

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;

    private GameObject[] spheres;
    private Vector3 randomOffset;
    public float range = 2f;




    public void Init(Vector3 pos)
    {
        PlayerController.Instance.canControl = false;
        onStart?.Invoke();
        onMiddle?.Invoke();
        transform.position = PlayerController.Instance.transform.position;

        StartCoroutine(CreateBullet(pos));
    }

    private IEnumerator CreateBullet(Vector3 mousePos)
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(gatlingObj.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(gatlingObj.name, gatlingObj, 10);
        }

        float timer = 0f;
        spheres = new GameObject[Mathf.CeilToInt(duration / 0.1f * 2)];
        int i = 0;
        
        // 마우스 방향 벡터 계산 및 정규화
        Vector3 direction = (mousePos - PlayerController.Instance.transform.position).normalized;

        while (timer < duration)
        {
            spheres[i] = ObjectPoolManager.Instance.Get(gatlingObj.name);

            MagicBall ball = spheres[i].GetComponent<MagicBall>(); // rain에 설정 주입
            // rain.soundController = soundController;
            // rain.vfxController = vfxController;
            
            ball.enemy = false;
            ball.onMiddle = onMiddle;
            ball.onHit = onHit;
            ball.onEnd = onEnd;
            ball.objName = gatlingObj.name;
            ball.duration = duration / 2;
            ball.through = through;
            ball.elemental = elemental;
            ball.homing = homing;
            ball.speed = speed;
            ball.damage = damage;
            ball.transform.localScale = transform.localScale;

            Vector2 dir = (mousePos - PlayerController.Instance.transform.position).normalized;
            // 랜덤 각도
            float randomAngle = UnityEngine.Random.Range(-range, range);

            // 기준 벡터를 중심으로 Z축 회전
            Vector3 randomizedDir = Quaternion.Euler(0, 0, randomAngle) * dir;

            float tempAngle = Mathf.Atan2(randomizedDir.y, randomizedDir.x) * Mathf.Rad2Deg;

            // 방향 벡터로 회전 적용
            ball.transform.rotation = Quaternion.Euler(0, 0, tempAngle);
            ball.transform.position = PlayerController.Instance.transform.position;

            ball.GatlingInit();

            PlayerController.Instance.transform.position -= direction * 0.1f;

            yield return YieldCache.WaitForSeconds(0.1f);
            timer += 0.1f;
            i++;
        }

        var lastObj = ObjectPoolManager.Instance.Get(gatlingObj.name).GetComponent<MagicBall>();

        lastObj.enemy = false;
        lastObj.onMiddle = onMiddle;
        lastObj.onHit = onHit;
        lastObj.onEnd = onEnd;
        lastObj.objName = gatlingObj.name;
        lastObj.duration = duration;
        lastObj.through = through;
        lastObj.elemental = elemental;
        lastObj.homing = homing;
        lastObj.speed = speed * 2;
        lastObj.damage = damage * 1.5f;
        lastObj.transform.localScale = transform.localScale * 2;


        // 방향 벡터로 각도 계산 (라디안 → 도 단위)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 오브젝트 회전 설정 (Z축 회전)
        lastObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        lastObj.transform.position = PlayerController.Instance.transform.position;

        lastObj.GatlingInit();

        PlayerController.Instance.canControl = true;

        if (!enemy)
        {
            ObjectPoolManager.Instance.Return(gameObject, objName);
        }

    }

    public void EnemyInit()
    {

    }
}
