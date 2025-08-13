using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Random = UnityEngine.Random;

public class PatternState_Meteor : EnemyState
{
    private int cnt;
    private float fallDistance;
    private float fallDuration;
    private float radius;
    private float firstDelay;
    private float lastDelay;
    private float coolDown;
    private AnimationType endAnimation;
    private List<Rune> runeList = new();
    private Skill projectileSo;
    private bool isTargeting;
    private bool isCenterTarget;
    private string castSFX;
    private List<string> SFXKeyList;


    PatternDataSo callBackPatternSo;

    private Action OnStarted;
    private Action OnUpdate;
    private Action OnFinished;
    public PatternState_Meteor(EnemyController enemyController, StateMachine stateMachine, PatternState_Meteor_So data) : base(enemyController, stateMachine)
    {
        cnt = data.cnt;
        fallDistance = data.fallDistance;
        fallDuration = data.firstDelay;
        radius = data.radius;
        firstDelay = data.firstDelay;
        lastDelay = data.lastDelay;
        isTargeting = data.isTargeting;
        isCenterTarget = data.isCenterTarget;
        coolDown = data.coolDown;
        castSFX = data.castSFX;
        SFXKeyList = data.SFX;
        projectileSo = data.projectileSo;

        callBackPatternSo = data.callBackPattern;
        OnStarted = UnityEventUtil.GetEventAction(data.OnStarted, enemyController);
        OnFinished = UnityEventUtil.GetEventAction(data.OnFinished, enemyController);

        foreach (var rune in data.runeList)
        {
            Rune temp = UnityEngine.Object.Instantiate(rune);
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
    private Vector3 finPos;
    private int attackIndex;

    public override void Enter()
    {
        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        OnStarted?.Invoke();
        attackIndex = 0;
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
                /*
            case Phase.Fire:
                FirePhase();
                break;*/
            case Phase.End:
                EndPhase();
                break;
        }

    }

    public override void Exit()
    {
        OnFinished?.Invoke();
    }

    private void WarningPhase()
    {

        SoundManager.PlaySFX(castSFX);

        attackIndex++;
        spawnPos = GetSpawnPos();

        if (isTargeting)
            finPos = enemyController.target.transform.position;
        else if (isCenterTarget)
            finPos = GetCenterPos();
        else
            finPos = GetRandomPos();

        enemyController.animationController.LookDir(dir);


        var warningreturn = WarningEffect.Instance.CreateCircle(
            finPos, radius, firstDelay, enemyController.targetLayer,
        () => { SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]); }
        );
        warningreturn.go.GetComponent<CircleWarningEffectController>().OnDamage += HandleDamage;
        //timer = firstDelay;
        FirePhase();
        timer = lastDelay;
        //currentPhase = Phase.Fire;
        

    }

    
    private void FirePhase()
    {
        GameObject projectile;

        foreach (var rune in runeList)
        {
            rune.enemy = true;
            rune.Apply(projectileSo);
        }

        //So에서 받을필요 없을듯?
        float shotSpeed = 0;
        projectile = projectileSo.EnemyUse(enemyController.enemyStat.attackPower, enemyController.gameObject, shotSpeed, spawnPos);
        projectile.GetComponent<Collider2D>().enabled = false;
        projectile.transform.position = GetSpawnPos();
        projectile.GetComponent<MagicBall>().speed = 0f;
        //projectile.transform.localScale


        MeteorFall(finPos, firstDelay, projectile);

        //timer = lastDelay;
        currentPhase = Phase.End;
        
    }

    private void EndPhase()
    {
        if (attackIndex >= cnt)
        {
            afterCoolTime = Time.time + coolDown;

            enemyController.animationController.isAttack = false;

            
            if (callBackPatternSo != null)
            {
                stateMachine.ChangeState(enemyController.FSM[callBackPatternSo]);
            }
            else
            {
                stateMachine.ChangeState(enemyController.idleState);
            }
        }
        else
        {
            currentPhase = Phase.Warning;
        }

    }

    public Vector3 GetSpawnPos(float yOffset = 5f, float extraHeight = 10f, float xMargin = 5f)
    {
        var bounds = enemyController.roomCollider.bounds;

        float xMin = bounds.min.x - xMargin;
        float xMax = bounds.max.x + xMargin;
        float x = Random.Range(xMin, xMax);

        float yMin = bounds.max.y + yOffset;
        float yMax = yMin + extraHeight;
        float y = Random.Range(yMin, yMax);

        return new Vector3(x, y, 0f);
    }

    public Vector3 GetCenterPos()
    {
        var bounds = enemyController.roomCollider.bounds;
        return new Vector3(bounds.center.x, bounds.center.y, 0f);
    }

    public Vector3 GetRandomPos()
    {
        var bounds = enemyController.roomCollider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector3(x, y, 0f);
    }

    

    public void MeteorFall(Vector3 targetPos, float fallDuration, GameObject projectile)
    {
        Vector3 tempPos = finPos;
        projectile.transform.DOMove(targetPos, fallDuration)
            .SetEase(Ease.InQuart) // 낙하 느낌
            .OnUpdate(() => {
            dir = (tempPos - projectile.transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                rot = Quaternion.Euler(0, 0, angle);

                projectile.transform.rotation = rot;
            })
            .OnComplete(() => {
                ObjectPoolManager.Instance.Return(projectile, projectile.GetComponent<MagicBall>().originalName);
                projectile.GetComponent<Collider2D>().enabled = true;
                EventBus.Publish(CameraEventType.Shake, null);
                EventBus.Publish(GimmickEvent.MeteorFall, null);
            });
    }



    //타격판정 워닝사인용
    //이벤트 제거는 워닝사인 내부에서 Get할때 알아서해주는중
    //하 이거 핸들러에 놓고 꺼내쓰는게 좋아보이긴하는데... 복잡해지네....
    private void HandleDamage(List<IDamageable> damageables)
    {
        foreach (var damageable in damageables)
        {
            damageable.OnDamage(enemyController.enemyStat.attackPower);
        }
    }
}
