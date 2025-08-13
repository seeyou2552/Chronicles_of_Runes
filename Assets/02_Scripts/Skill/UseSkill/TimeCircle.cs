using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;

public class TimeCircle : MonoBehaviour, SkillAction
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

    private float finalDamage;
    private float timer;
    private bool startTimer; // 타이머 시작 불값
    private Image timeComp;
    private GameObject timeObj; // 풀링이 애매하여 캔버스 이미지 캐싱해서 사용
    private HashSet<Collider2D> floorTargets = new HashSet<Collider2D>();
    private float searchRadius = 200f;
    private Transform target;
    float searchCooldown = 1f;
    float searchTimer;


    [Header("Controller")]
    [SerializeField] private SkillSoundController soundController;
    [SerializeField] private SkillVFXController vfxController;
    [SerializeField] private GameObject timeImagePrefab; // 프리팹



    public void Init(Vector3 pos)
    {
        if (timeObj == null)
        {
            timeObj = Instantiate(timeImagePrefab);
            timeObj.transform.SetParent(SkillCanvas.Instance.transform, false);
            timeComp = timeObj.GetComponent<Image>();
        }
        timeObj.SetActive(true);

        onMiddle?.Invoke();
        ChangeColor();

        timeObj.transform.position = pos;
        timeObj.transform.localScale = transform.localScale;
        transform.position = pos;

        timer = 0;
        finalDamage = 0;
        soundController.StartSound(objName);

        StartCoroutine(IncreaseDamage());
    }

    private IEnumerator IncreaseDamage()
    {
        timer = 0;
        startTimer = true;
        while (timer < duration)
        {
            yield return YieldCache.WaitForSeconds(1f);
            finalDamage += damage;
        }
        SoundManager.StopAllSFX();
        startTimer = false;
        TakeDamage();
        timeComp.fillAmount = 1f;
        soundController.HitSound(objName);
        Return();
    }

    void Update()
    {
        if (startTimer)
        {
            timer += Time.deltaTime;
            timeComp.fillAmount = timer / duration;
        }
        if (homing)
        {
            
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                searchTimer = searchCooldown;
                UpdateTarget();
            }

            if (target != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                timeObj.transform.position = Vector3.MoveTowards(timeObj.transform.position, target.position, speed * Time.deltaTime);
            }
        }
    }

    private void UpdateTarget()
    {
        GameObject[] GetEnemiesByPhysics()
            {
                // 현재 오브젝트 위치를 중심으로 반경 searchRadius 내의 Collider2D를 레이어 마스크로 검색
                Collider2D[] hits = Physics2D.OverlapCircleAll(gameObject.transform.position, searchRadius, LayerMask.GetMask("Enemy"));
                return hits
                    .Select(c => c.gameObject)
                    .Distinct()  // 중복 제거
                    .ToArray();
            }

            var enemies = GetEnemiesByPhysics();
            Logger.Log(enemies.Length.ToString());

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
            target = closest;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy)
        {
            floorTargets.Add(collision);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {
            floorTargets.Add(collision);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemy)
        {
            floorTargets.Remove(collision);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && enemy)
        {
            floorTargets.Remove(collision);
        }
    }

    void TakeDamage()
    {
        if (floorTargets == null) return;
        foreach (var target in floorTargets)
        {
            if (target != null)
            {
                if (!target.GetComponent<EnemyController>().isDead)
                {
                    target.GetComponent<IDamageable>().OnDamage(finalDamage);
                    target.GetComponent<DebuffController>().TakeDebuff(target, elemental);
                    onHit?.Invoke();
                }
            }
        }
    }

    public void Return()
    {
        onEnd?.Invoke();
        timeObj.SetActive(false);
        ObjectPoolManager.Instance.Return(this.gameObject, objName);
    }

    private void ChangeColor()
    {
        switch (elemental)
        {
            case ElementalType.Normal:
                timeComp.color = new Color(1, 1, 1);
                break;
            case ElementalType.Fire:
                timeComp.color = new Color(1, 0, 0);
                break;
            case ElementalType.Water:
                timeComp.color = new Color(0, 0.5f, 1);
                break;
            case ElementalType.Ice:
                timeComp.color = new Color(0, 1, 1);
                break;
            case ElementalType.Electric:
                timeComp.color = new Color(1, 1, 0);
                break;
            case ElementalType.Dark:
                timeComp.color = new Color(1, 0, 1);
                break;
            case ElementalType.Light:
                timeComp.color = new Color(1, 0.8f, 0);
                break;
        }
    }


    public void EnemyInit()
    {

    }

}
