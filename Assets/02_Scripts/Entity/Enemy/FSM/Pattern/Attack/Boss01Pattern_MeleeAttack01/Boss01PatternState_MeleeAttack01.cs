using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss01PatternState_MeleeAttack01 : EnemyState
{
    private Boss01Pattern_MeleeAttack01_So data;

    int dashCnt;           // 돌진 횟수
    float dashDistance;    // 돌진 거리
    float dashDuration;    // 돌진 시간
    float firstDelay;      // 선딜
    float lastDelay;       // 후딜
    float dashWidth;
    float lastRatio;
    float Ratio;

    float rich;            // VFX 위치 보정용
    GameObject slashVFX;

    List<string> SFXKeyList;

    bool isBodySlam;
    bool isEffectAttack;

    float coolDown;

    AnimationType endAnimation;

    PatternDataSo callBackPatternSo;

    private Action OnStarted;
    private Action OnUpdate;
    private Action OnFinished;

    public Boss01PatternState_MeleeAttack01(EnemyController enemyController, StateMachine stateMachine, Boss01Pattern_MeleeAttack01_So data)
        : base(enemyController, stateMachine)
    {
        this.data = data;
        dashCnt = data.dashCnt;
        dashDistance = data.dashDistance;
        dashDuration = data.dashDuration;
        firstDelay = data.firstDelay;
        lastDelay = data.lastDelay;
        dashWidth = data.dashWidth;
        lastRatio = data.lastRatio;
        Ratio = data.Ratio;
        rich = data.rich;
        slashVFX = data.VFX;
        SFXKeyList = data.SFX;
        isBodySlam = data.isBodySlam;
        isEffectAttack = data.isEffectAttack;
        coolDown = data.coolDown;

        OnStarted = UnityEventUtil.GetEventAction(data.OnStarted, enemyController);
        OnFinished = UnityEventUtil.GetEventAction(data.OnFinished, enemyController);


        callBackPatternSo = data.callBackPattern;

        endAnimation = data.endAnimation;
    }

    Vector3 dir;
    public WarningEffectController collisionTracker;

    enum Phase
    {
        Warning,
        Dash,
        End,
    }

    int dashIndex;
    float timer;
    Vector3 dashStartPos;
    Vector3 dashEndPos;
    GameObject vfxObj;

    Phase currentPhase;

    public override void Enter()
    {
        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        OnStarted?.Invoke();
        dashIndex = 0;
        Ratio = 1f;
        currentPhase = Phase.Warning;
        timer = firstDelay * Ratio;

        if (isBodySlam)
        {
            enemyController.ontriggerEntered += HandleDamage;
        }

        SetupWarning();
    }

    public override void Update()
    {
        if (enemyController.isDead)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        // Dashing 중에는 이동을 계속 수행해야 함
        if (currentPhase == Phase.Dash)
            PerformDashStep();

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        switch (currentPhase)
        {
            case Phase.Warning:
                SetupDash();
                currentPhase = Phase.Dash;
                timer = dashDuration;
                break;

            case Phase.Dash:
                warningLine.transform.SetParent(null, true);
                currentPhase = Phase.End;
                timer = lastDelay;
                break;

            case Phase.End:
                EndDash();
                dashIndex++;
                if (dashIndex < dashCnt)
                {
                    if (dashIndex == dashCnt - 1)
                        Ratio = lastRatio;

                    currentPhase = Phase.Warning;
                    timer = firstDelay * Ratio;
                    SetupWarning();
                }
                else
                {
                    EndPattern();
                }
                break;
        }
    }
    public override void Exit()
    {
        OnFinished?.Invoke();
        if (isBodySlam)
        {
            enemyController.ontriggerEntered -= HandleDamage;
        }

        if (collisionTracker != null)
        {
            collisionTracker.OnDamage -= HandleDamage;
        }
    }

    GameObject warningLine;
    private void SetupWarning()
    {
        dir = (enemyController.target.position - enemyController.transform.position).normalized;
        enemyController.animationController.LookDir(dir);

        var warningReturn = WarningEffect.Instance.CreateLines(
            new Vector3[] { enemyController.transform.position },
            dir, 1, dashDistance, dashWidth,
            firstDelay * Ratio, enemyController.targetLayer,
            () => { 
                SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]);
            }
        );
        warningLine = warningReturn.go;
        warningLine.transform.SetParent(enemyController.transform);
        enemyController.tweens.Add(warningReturn.tween);
        //맞는지 체크 한번 해봐야할듯
        if (!isBodySlam && isEffectAttack == false)
        {
            warningReturn.go.GetComponent<WarningEffectController>().OnDamage += HandleDamage;
        }
    }

    private void SetupDash()
    {
        dashStartPos = enemyController.transform.position;
        dashEndPos = dashStartPos + dir * dashDistance;

        enemyController.animationController.animator.SetTrigger(endAnimation.ToString());

        vfxObj = ObjectPoolManager.Instance.Get(slashVFX.name);
        vfxObj.transform.position = dashStartPos + dir * rich;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfxObj.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

        //이펙트 시작 후 잠깐 따라오게 만드는 용도
        vfxObj.transform.SetParent(enemyController.transform);

        //이펙트 타격판정
        if (isEffectAttack)
        {
            var vfx = vfxObj.GetComponent<VFXController>();
            vfx.targetMask = enemyController.targetLayer;
            vfx.isAttack = true;
            vfx.damage = enemyController.enemyStat.attackPower;
        }

        
        if (!isBodySlam && collisionTracker != null)
        {
            collisionTracker.OnDamage += HandleDamage;
        }

        enemyController.animationController.isAttack = true;
    }

    private void PerformDashStep()
    {
        if (isEffectAttack == false)
        {
            float t = 1f - Mathf.Pow(1f - (1 - timer / dashDuration), 2f); // Ease-out
            Vector2 nextPos = Vector3.Lerp(dashStartPos, dashEndPos, t);
            float offset = 1.5f;
            Vector2 rayOrigin = enemyController.transform.position;
            Vector2 rayDir = (nextPos - rayOrigin).normalized;
            //정확하게 계산해야해서 sqrMagnitude쓰면 안될듯
            float rayDist = Vector2.Distance(rayOrigin, nextPos) + offset;

            float radius = 0.7f;
            RaycastHit2D hit = Physics2D.CircleCast(rayOrigin, radius, rayDir, rayDist, LayerMask.GetMask("Obstacle"));

            if (hit.collider != null)
            {
                //보정 안해주는게 날듯
                //enemyController.transform.position = hit.point - rayDir * (radius + offset);
                timer = 0f; // 강제 종료
                return;
            }

            enemyController.transform.position = nextPos;
        }
    }

    private void EndDash()
    {
        // 마지막 위치 보정
        // 일단 잠깐 뺄까
        /*
        if (isEffectAttack == false)
        {
            if (!Physics2D.Raycast(enemyController.transform.position, dir, 0.3f, LayerMask.GetMask("Obstacle")))
            {
                enemyController.transform.position = dashEndPos;
            }
        }
        */
        enemyController.animationController.isAttack = false;

        /*
        if (collisionTracker != null)
        {
            collisionTracker.OnDamage -= HandleDamage;
        }
        */

        if (vfxObj != null)
        {
            vfxObj.transform.SetParent(null);
        }
    }

    private void EndPattern()
    {
        afterCoolTime = Time.time + coolDown;

        //enemyController.doubleBoss.CallBackChangePattern();


        if (callBackPatternSo != null)
        {
            stateMachine.ChangeState(enemyController.FSM[callBackPatternSo]);
        }
        else
        {
            stateMachine.ChangeState(enemyController.idleState);
        }

        
    }




    //타격판정 워닝사인용
    private void HandleDamage(List<IDamageable> damageables)
    {
        foreach (var damageable in damageables)
        {
            damageable.OnDamage(enemyController.enemyStat.attackPower);
        }
    }

    //타격판정 몸통박치기용
    private void HandleDamage(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyController.targetLayer.value) != 0)
        {
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.OnDamage(enemyController.enemyStat.attackPower);
            }
        }
    }
}
