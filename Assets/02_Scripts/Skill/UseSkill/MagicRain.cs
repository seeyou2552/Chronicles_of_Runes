using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MagicRain : MonoBehaviour, SkillAction
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
    public GameObject rainObj;

    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;

    private GameObject[] spheres;
    private Vector3 randomOffset;
    public float xRange = 5f;
    public float yRange = 2f;




    public void Init(Vector3 pos)
    {
        onMiddle?.Invoke();
        transform.position = PlayerController.Instance.transform.position;

        StartCoroutine(CreateRain(pos));
    }

    private IEnumerator CreateRain(Vector3 createPos)
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(rainObj.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(rainObj.name, rainObj, 10);
        }

        float timer = 0f;
        spheres = new GameObject[Mathf.CeilToInt(duration / 0.1f)];
        int i = 0;
        while (timer < duration)
        {
            spheres[i] = ObjectPoolManager.Instance.Get(rainObj.name);

            Rain rain = spheres[i].GetComponent<Rain>(); // rain에 설정 주입
            if(elemental == ElementalType.Electric) rain.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            else rain.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
            rain.soundController = soundController;
            rain.vfxController = vfxController;
            rain.onHit = onHit;
            rain.onEnd = onEnd;
            rain.objName = rainObj.name;
            rain.duration = duration;
            rain.through = through;
            rain.elemental = elemental;
            rain.homing = homing;
            rain.speed = speed;
            if (elemental == ElementalType.Electric) rain.damage = damage / 2; // 일렉트릭 + homing 효과가 너무 좋아서 반토막 내야할듯
            else rain.damage = damage;
            rain.transform.localScale = transform.localScale;



            if (elemental != ElementalType.Electric)
            {
                randomOffset = new Vector3(
                    UnityEngine.Random.Range(-xRange, xRange),  // X축 랜덤
                    UnityEngine.Random.Range(10 - yRange, 10 + yRange),  // Y축 랜덤
                    0f
                );
            }
            else // Electric은 마우스 주위에 생성
            {
                randomOffset = new Vector3(
                    UnityEngine.Random.Range(-xRange, xRange),  // X축 랜덤
                    UnityEngine.Random.Range(-yRange, yRange),  // Y축 랜덤
                    0f
                );
            }


            rain.transform.position = createPos + randomOffset;

            rain.Init();

            yield return YieldCache.WaitForSeconds(0.1f);
            timer += 0.1f;
            i++;
        }

        if (!enemy)
        {
            ObjectPoolManager.Instance.Return(gameObject, objName);
        }

    }

    public void EnemyInit()
    {

    }
}
