using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using UnityEditor.Rendering;
using System.Threading.Tasks;


public interface SkillAction
{
    public Action onStart { get; set; } // 스킬 시작 시 Action (상의 후 제거 예정)
    public Action onMiddle { get; set; } // 스킬 사용 중 Action
    public Action onHit { get; set; } // 스킬 적중 시 Action
    public Action onEnd { get; set; } // 스킬 종료 시 Action
    public Action onEnemy { get; set; }

    public bool through { get; set; } // 관통 여부
    public bool homing { get; set; } // 추적 여부
    public float duration { get; set; } // 스킬 지속시간
    public ElementalType elemental { get; set; } // 스킬 속성 값
    public float damage { get; set; }

    public bool enemy { get; set; } // 시전자 체크
    public string objName { get; set; } // 스킬 오브젝트 네임
    public GameObject caster { get; set; }

    public float speed { get; set; }

    void EnemyInit(); // Enemy의 스킬시전
    void Init(Vector3 pos);
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill/Skill Set")]
public class Skill : ScriptableObject
{
    [Header("SkillOption")]
    public string skillName; // 스킬 이름
    public float speed;
    public GameObject obj; // 발사체
    public float coolTime;  // 쿨타임
    public int coast; // 마나
    public float delay; // 스킬 선딜레이 (단위 : 초)
    public float duration; // 스킬 지속시간
    public string SkillAnim;

    [Header("데미지 관련")]
    public float damage; // 스킬의 기본 데미지
    [Range(0f, 150f)]
    public float playerAttackPercentage; // 데미지에 적용할 플레이어의 공격력 비율


    [Header("룬 액션 세팅")]
    public Action setStart;
    public Action setMiddle;
    public Action setHit;
    public Action setEnd;
    public Action onUseSkill; // 스킬 사용했을 때 발동할 액션
    public List<Func<UniTask>> onUseAsyncSkill; // 현재 Retry 용으로 사용 중
    public Action setEnemy; // Enemy 용 액션

    public SkillAction skillAction;
    public SkillAction skillActionDevide;
    public SkillAction enemySkillAction;

    [Header("ETC")]
    public GameObject pool;
    public GameObject devidePool;
    public GameObject enemyPool;
    public int count;

    public void StartSet(Action act)
    {
        setStart += act;
    }

    public void MiddleSet(Action act)
    {
        setMiddle += act;
    }

    public void HitSet(Action act)
    {
        setHit += act;
    }

    public void EndSet(Action act)
    {
        setEnd += act;
    }

    public void UseSkillSet(Action act)
    {
        onUseSkill += act;
    }

    public void UseSkillAsyncSet(Func<UniTask> asyncAct)
    {
        onUseAsyncSkill.Add(asyncAct);
    }

    public void EnemySet(Action act)
    {
        setEnemy += act;
    }

    public void ApplyRunes(List<Rune> runes)
    {
        onUseAsyncSkill = new List<Func<UniTask>>();
        onUseSkill = null;
        setStart = null;
        setMiddle = null;
        setHit = null;
        setEnd = null;
        foreach (var rune in runes)
        {
            rune.Apply(this);
        }
    }

    public void EnemyApplyRunes(List<Rune> runes)
    {
        setEnemy = null;
        foreach (var rune in runes)
        {
            rune.enemy = true;
            rune.Apply(this);
        }
    }

    public async Task Use() // 플레이어 스킬 사용
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(obj.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(obj.name, obj, 1);
        }

        pool = ObjectPoolManager.Instance.Get(obj.name);
        skillAction = pool.GetComponent<SkillAction>();
        pool.SetActive(false);

        // 시작 시 초기화 부분
        devidePool = null;
        skillAction.elemental = ElementalType.Normal;
        pool.transform.localScale = obj.transform.localScale;
        skillAction.enemy = false;
        skillAction.through = false;
        skillAction.duration = duration;
        skillAction.homing = false;

        // Action 주입
        skillAction.speed = speed;
        skillAction.onStart = setStart;
        skillAction.onMiddle = setMiddle;
        skillAction.onHit = setHit;
        skillAction.onEnd = setEnd;
        skillAction.objName = obj.name;
        skillAction.damage = (float)((damage + PlayerController.Instance.AttackPower())*PlayerController.Instance.DamageOutcome()); //* (playerAttackPercentage * 0.01)
        //Debug.Log("( " + damage + "+" + PlayerController.Instance.AttackPower() + " ) * " + PlayerController.Instance.DamageOutcome() + " * " + playerAttackPercentage * 0.01);
        
        // 스킬 위치 조정 및 시작
        pool.transform.position = PlayerController.Instance.transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // 2D 게임이라 Z 0으로 고정
        DelayInit(skillAction, pool, mouseWorldPos).Forget();
        await UniTask.Delay(200);
        foreach (var act in onUseAsyncSkill) // 일단 retry만 사용 중
        {
            await act();
        }

    }

    public void DevideUse() // 플레이어 스킬 사용 (Divide 용)
    {
        devidePool = ObjectPoolManager.Instance.Get(obj.name);
        skillActionDevide = devidePool.GetComponent<SkillAction>();
        devidePool.SetActive(false);

        skillActionDevide.elemental = skillAction.elemental;
        if (devidePool.transform.localScale != obj.transform.localScale) devidePool.transform.localScale = obj.transform.localScale;
        skillActionDevide.enemy = false;
        skillActionDevide.duration = duration;
        skillActionDevide.homing = false;
        skillActionDevide.through = false;

        skillActionDevide.onStart = setStart;
        skillActionDevide.onMiddle = setMiddle;
        skillActionDevide.onHit = setHit;
        skillActionDevide.onEnd = setEnd;
        skillActionDevide.objName = obj.name;
        skillActionDevide.speed = speed;
        skillActionDevide.damage = (float)((damage + PlayerController.Instance.AttackPower()) * PlayerController.Instance.DamageOutcome()) / 2; // * (playerAttackPercentage * 0.01)

        devidePool.transform.position = PlayerController.Instance.transform.position + PlayerController.Instance.transform.right;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // 2D 게임이라 Z 0으로 고정

        devidePool.SetActive(true);
        skillActionDevide.Init(mouseWorldPos);
        // DelayInit(skillActionDevide, devidePool, mouseWorldPos).Forget();
    }

    private async UniTaskVoid DelayInit(SkillAction act, GameObject objPool, Vector3 pos)
    {
        PlayerController.Instance.canControl = false;
        onUseSkill?.Invoke();
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        if (devidePool != null) // devide 있을 때 elemental 주입
        {
            skillActionDevide.elemental = skillAction.elemental;
            devidePool.SetActive(true);
            skillActionDevide.Init(pos);
        }
        PlayerController.Instance.canControl = true;
        objPool.SetActive(true);
        act.Init(pos);
    }

    public GameObject EnemyUse(float enemyDamage, GameObject enemyObj, float shotSpeed = 0f) // 몬스터 스킬 사용
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(obj.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(obj.name, obj, 1);
        }

        enemyPool = ObjectPoolManager.Instance.Get(obj.name);

        enemySkillAction = enemyPool.GetComponent<SkillAction>();

        enemySkillAction.enemy = true;

        enemySkillAction.elemental = ElementalType.Normal;
        if (enemyPool.transform.localScale != obj.transform.localScale) enemyPool.transform.localScale = obj.transform.localScale;

        /*
        enemySkillAction.onMiddle = setMiddle;
        enemySkillAction.onHit = setHit;
        enemySkillAction.onEnd = setEnd;
        */

        enemySkillAction.speed = shotSpeed;
        enemySkillAction.onEnemy = setEnemy;
        enemySkillAction.damage = enemyDamage;
        enemySkillAction.caster = enemyObj;
        enemySkillAction.homing = false;
        enemySkillAction.duration = duration;
        enemySkillAction.objName = obj.name;


        /////////////////
        enemyPool.transform.position = enemyObj.transform.position;
        enemySkillAction.EnemyInit();

        return enemyPool;
    }

    public GameObject EnemyUse(float enemyDamage, GameObject enemyObj, float shotSpeed, Vector3 pos) // 몬스터 스킬 사용
    {
        if (ObjectPoolManager.Instance.pools != null && ObjectPoolManager.Instance.pools.ContainsKey(obj.name))
        {

        }
        else  // 풀 없으면 생성
        {
            ObjectPoolManager.Instance.CreatePool(obj.name, obj, 1);
        }

        enemyPool = ObjectPoolManager.Instance.Get(obj.name);

        enemySkillAction = enemyPool.GetComponent<SkillAction>();

        enemySkillAction.enemy = true;

        enemySkillAction.elemental = ElementalType.Normal;
        if (enemyPool.transform.localScale != obj.transform.localScale) enemyPool.transform.localScale = obj.transform.localScale;

        /*
        enemySkillAction.onMiddle = setMiddle;
        enemySkillAction.onHit = setHit;
        enemySkillAction.onEnd = setEnd;
        */

        enemySkillAction.speed = shotSpeed;
        enemySkillAction.onEnemy = setEnemy;
        enemySkillAction.damage = enemyDamage;
        enemySkillAction.caster = enemyObj;
        enemySkillAction.objName = obj.name;


        /////////////////
        enemyPool.transform.position = pos;
        enemySkillAction.EnemyInit();

        return enemyPool;
    }
}