using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PatternState_ChasingRush : EnemyState
{
    private PatternState_ChasingRush_So data;

    float lookDuration;
    float dashDistance;    // 돌진 거리
    float dashDuration;    // 돌진 시간
    float firstDelay;      // 선딜
    float lastDelay;       // 후딜
    float dashWidth;

    float rich;            // VFX 위치 보정용
    GameObject slashVFX;

    List<string> SFXKeyList;
    private string crashSFX;

    bool isBodySlam;
    bool isEffectAttack;

    float coolDown;

    AnimationType endAnimation;


    private Action OnStarted;
    private Action OnUpdate;
    private Action OnFinished;

    PatternDataSo callBackPatternSo;
    //이름 너무 헷갈리게 지엇네
    IPatternCallBack patternCallBack;
    public PatternState_ChasingRush(EnemyController enemyController, StateMachine stateMachine, PatternState_ChasingRush_So data)
        : base(enemyController, stateMachine)
    {
        this.data = data;
        lookDuration = data.lookDuration;
        dashDistance = data.dashDistance;
        dashDuration = data.dashDuration;
        firstDelay = data.firstDelay;
        lastDelay = data.lastDelay;
        dashWidth = data.dashWidth;
        rich = data.rich;
        slashVFX = data.VFX;
        SFXKeyList = data.SFX;
        crashSFX = data.crashSFX;

        isBodySlam = data.isBodySlam;
        isEffectAttack = data.isEffectAttack;
        coolDown = data.coolDown;


        callBackPatternSo = data.callBackPatternSo;
        OnStarted = UnityEventUtil.GetEventAction(data.OnStarted, enemyController);
        OnFinished = UnityEventUtil.GetEventAction(data.OnFinished, enemyController);



        /*
        if (data.patternCallBack != null)
        {
            //callBackPatternSo = data.callBackPattern;
            //이게 뭐야..
            patternCallBack = enemyController.doubleBoss?.GetComponent<IPatternCallBack>();
        }
        */

        /*if (data.patternCallBack is IPatternCallBack cb)
        {
            patternCallBack = cb;
        }*/
        //이거도 된다고하네
        //patternCallBack = data.patternCallBack.GetComponent<IPatternCallBack>();
        endAnimation = data.endAnimation;
    }

    Vector3 dir;
    public WarningEffectController collisionTracker;

    enum Phase
    {
        Looking,
        SetupWarning,
        Warning,
        Dash,
        End,
    }

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
        currentPhase = Phase.Looking;
        timer = lookDuration;

        if (isBodySlam)
        {
            enemyController.ontriggerEntered += HandleDamage;
        }

        //SetupWarning();
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

        if(currentPhase == Phase.Looking)
        {
            LookTarget();
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        switch (currentPhase)
        {
            case Phase.Looking:
                //currentPhase = Phase.SetupWarning;
                //timer = lookDuration;
                SetupWarning();
                timer = firstDelay;
                currentPhase = Phase.Warning;
                break;
                /*
            case Phase.SetupWarning:
                SetupWarning();
                timer = firstDelay;
                currentPhase = Phase.Warning;
                break;
                */
            case Phase.Warning:
                SetupDash();
                currentPhase = Phase.Dash;
                timer = dashDuration;
                break;

            case Phase.Dash:
                currentPhase = Phase.End;
                timer = lastDelay;
                break;

            case Phase.End:
                EndDash();
                EndPattern();

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

    private void SetupWarning()
    {
        dir = (enemyController.target.position - enemyController.transform.position).normalized;
        enemyController.animationController.LookDir(dir);

        var warningReturn = WarningEffect.Instance.CreateLines(
            new Vector3[] { enemyController.transform.position },
            dir, 1, dashDistance, dashWidth,
            firstDelay, enemyController.targetLayer,
        () => { SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]); }
        );

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
        float distanceOffset = 2f;
        float t = 1f - Mathf.Pow(1f - (1 - timer / dashDuration), 2f); // Ease-out
        Vector3 nextPos = Vector3.Lerp(dashStartPos, dashEndPos, t);

        Vector2 rayOrigin = enemyController.transform.position;
        Vector2 rayDir = (nextPos - enemyController.transform.position).normalized;
        //정확하게 계산해야 해서 sqrMagnitude쓰면 안될듯
        float rayDist = Vector2.Distance(enemyController.transform.position, nextPos) + distanceOffset;

        float castRadius = 3f;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            rayOrigin,
            castRadius,
            rayDir,
            rayDist,
            LayerMask.GetMask("Obstacle", "Enemy")
        );

        RaycastHit2D? firstValidHit = null;

        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            if (h.collider.gameObject == enemyController.gameObject) continue;

            firstValidHit = h;
            break;
        }

        if (firstValidHit.HasValue)
        {

            var hit = firstValidHit.Value;
            if (vfxObj != null)
            {
                vfxObj.transform.position = hit.transform.position;
                vfxObj.transform.SetParent(null);
                vfxObj = null;
            }

            Vector3 collisionPos = hit.point - rayDir * 0.3f;
            enemyController.transform.position = collisionPos;

            Vector2 knockbackDir = hit.normal.normalized;
            float knockbackDist = 5.5f;
            float knockbackDuration = 0.2f;

            Vector3 knockbackTargetPos = collisionPos + (Vector3)(knockbackDir * knockbackDist);

            SoundManager.PlaySFX(crashSFX);
            hit.collider.GetComponent<EnemyController>()?.animationController.animator.SetTrigger("TakeDamage");
            var debuff = hit.collider.GetComponent<DebuffController>();
            if (debuff != null)
            {
                debuff.TakeSturn(hit.collider, 3f);
                var selfCollider = enemyController.GetComponent<Collider2D>();
                if (selfCollider != null)
                {
                    enemyController.GetComponent<DebuffController>().TakeSturn(selfCollider, 3f);
                    enemyController.animationController.animator.SetTrigger("TakeDamage");
                    //enemyController.StartCoroutine(enemyController.DieAnimation(3f));
                }

            }

            knockbackTargetPos = collisionPos + (Vector3)(knockbackDir * knockbackDist);

            enemyController.transform.DOMove(knockbackTargetPos, knockbackDuration)
                .SetEase(Ease.OutQuad);

            timer = 0f;
            return;
        }

        // 충돌이 없으면 그냥 이동
        enemyController.transform.position = nextPos;
    }




    private void EndDash()
    {
        // 마지막 위치 보정
        if (!Physics2D.Raycast(enemyController.transform.position, dir, 0.3f, LayerMask.GetMask("Obstacle")))
        {
            enemyController.transform.position = dashEndPos;
        }

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


    public void LookTarget()
    {
        /*
        dir = (enemyController.target.transform.position - enemyController.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        enemyController.transform.rotation = rot;
        */
        enemyController.animationController.LookDir(enemyController.target.transform.position);
    }
}
