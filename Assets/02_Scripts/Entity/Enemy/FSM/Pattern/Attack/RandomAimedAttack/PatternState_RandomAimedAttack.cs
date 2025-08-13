using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Transform = UnityEngine.Transform;

public class PatternState_RandomAimedAttack : EnemyState
{
    private Boss01PatternState_AimedAttack_So data;

    private float firstDelay;
    private float lastDelay;
    private float shotSpeed;
    private float warningLength;
    private float warningWidth;
    private float Ratio;
    private float coolDown;
    private AnimationType endAnimation;
    private List<string> SFXKeyList;
    private List<Rune> runeList = new();
    private float radius;
    private Skill projectileSo;

    PatternDataSo callBackPatternSo;

    private UnityEvent OnStarted;
    private UnityEvent OnUpdate;
    private UnityEvent OnFinished;

    public PatternState_RandomAimedAttack(EnemyController enemyController, StateMachine stateMachine, PatternState_RandomAimedAttack_So data) : base(enemyController, stateMachine)
    {
        firstDelay = data.firstDelay;
        lastDelay = data.lastDelay;
        shotSpeed = data.shotSpeed;
        warningLength = data.warningLength;
        warningWidth = data.warningWidth;
        Ratio = data.Ratio;
        coolDown = data.coolDown;
        endAnimation = data.endAnimation;
        SFXKeyList = data.SFX;
        radius = data.radius;
        projectileSo = data.projectileSo;

        callBackPatternSo = data.callBackPattern;
        OnStarted = data.OnStarted;
        OnFinished = data.OnFinished;

        foreach (var rune in data.runeList)
        {
            Rune temp = Object.Instantiate(rune);
            runeList.Add(temp);
        }
    }


    private enum Phase
    {
        Warning,
        Fire,
        End
    }

    private Phase currentPhase;
    private float timer;
    private Quaternion rot;
    private Vector3 dir;
    private Vector3 spawnPos;


    public override void Enter()
    {
        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        timer = 0f;
        currentPhase = Phase.Warning;
    }

    public override void Update()
    {

        if (enemyController.isDead)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        switch (currentPhase)
        {
            case Phase.Warning:
                WarningPhase();
                break;
            case Phase.Fire:
                FirePhase();
                break;
            case Phase.End:
                EndPhase();
                break;
        }

    }

    public override void Exit()
    {
    }


    private void WarningPhase()
    {
        spawnPos = GetRandomPos(PlayerController.Instance.transform, radius);
        dir = (enemyController.target.position - spawnPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rot = Quaternion.Euler(0, 0, angle);

        enemyController.animationController.LookDir(dir);

        WarningEffect.Instance.CreateLines(
            new Vector3[] { spawnPos },
            dir, 1, warningLength, warningWidth,
            firstDelay * Ratio, enemyController.targetLayer
            //() => { SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]); }
        );

        timer = firstDelay;
        currentPhase = Phase.Fire;

    }

    private void FirePhase()
    {
        foreach (var rune in runeList)
        {
            rune.enemy = true;
            if (projectileSo == null)
            {
                Logger.Log("없어이거");
            }
            rune.Apply(projectileSo);
        }

        GameObject projectile = projectileSo.EnemyUse(enemyController.enemyStat.attackPower, enemyController.gameObject, shotSpeed, spawnPos);
        projectile.transform.rotation = rot;

        timer = lastDelay;
        currentPhase = Phase.End;
    }


    public Vector3 GetRandomPos(Transform target, float radius)
    {
        float angle = Random.Range(0f, 360f); // 0~360도 중 랜덤
        float rad = angle * Mathf.Deg2Rad;    // 라디안 변환

        float x = Mathf.Cos(rad) * radius;
        float y = Mathf.Sin(rad) * radius;

        return new Vector3(
            target.position.x + x,
            target.position.y + y,
            0f
        );
    }

    private void EndPhase()
    {
        afterCoolTime = Time.time + coolDown;

        if (callBackPatternSo != null)
        {
            stateMachine.ChangeState(enemyController.FSM[callBackPatternSo]);
        }
        else
        {
            stateMachine.ChangeState(enemyController.idleState);
        }

    }
}
