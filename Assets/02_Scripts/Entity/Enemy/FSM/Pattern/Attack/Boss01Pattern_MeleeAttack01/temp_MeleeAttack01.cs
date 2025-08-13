using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class tttemp : EnemyState
{
    private float dashDuration;
    private float dashWidth;
    private float rushSpeed;

    private float firstDelay;
    private float lastDelay;

    private AnimationType endAnimation;

    private float coolDown;
    public PatternDataSo callBackPatternSo;

    public tttemp(EnemyController enemyController, StateMachine stateMachine, PatternState_ChasingRush_So data) : base(enemyController, stateMachine)
    {
        dashDuration = data.dashDuration;
        dashWidth = data.dashWidth;
        rushSpeed = data.dashSpeed;

        firstDelay = data.firstDelay;
        lastDelay = data.lastDelay;

        coolDown = data.coolDown;

        endAnimation = data.endAnimation;

        callBackPatternSo = data.callBackPatternSo;
    }


    private enum Phase
    {
        Warning,
        Rush,
        End
    }

    private Phase currentPhase;
    private float timer;
    private Quaternion rot;
    private Vector3 dir;
    private Vector3 spawnPos;
    private Vector3 finPos;

    private LayerMask targetLayer;

    public override void Enter()
    {

        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        afterCoolTime = Time.time + coolDown;
        targetLayer = LayerMask.GetMask("Enemy"); //임시
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
            case Phase.Rush:
                RushPhase();
                break;
                /*
            case Phase.End:
                EndPhase();
                break;*/
        }

    }

    public override void Exit()
    {
    }

    private void WarningPhase()
    {

        enemyController.animationController.LookDir(dir);

        /*
        WarningEffect.Instance.CreateLines(
            finPos, 1, firstDelay, enemyController.targetLayer
        //() => { SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]); }
        );

        //timer = firstDelay;
        timer = lastDelay;
        */
        currentPhase = Phase.Rush;

    }

    private void RushPhase()
    {
        Vector3 dir = (enemyController.target.position - enemyController.transform.position).normalized;
        float distanceThisFrame = rushSpeed * Time.deltaTime;
        Vector3 nextPos = enemyController.transform.position + dir * distanceThisFrame;

        float detectionRadius = 0.5f;

        // Enemy, Player 레이어만 탐지
        LayerMask detectionMask = LayerMask.GetMask("Enemy", "Player");

        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)nextPos, detectionRadius, detectionMask);

        bool hasValidHit = false;

        foreach (var collider in hits)
        {
            if (collider.gameObject == enemyController.gameObject)
            {
                continue; // 자기 자신은 무시
            }

            hasValidHit = true;

            EnemyController targetEnemy = collider.GetComponentInParent<EnemyController>();
            if (targetEnemy != null)
            {
                EndChangState();
            }

            Logger.Log($"[Rush] 충돌: {collider.gameObject.name}");
            break; // 하나만 처리하고 종료
        }

        if (hasValidHit)
        {
            // 충돌한 경우
            EndChangState();
        }
        else
        {
            // 충돌 없으면 이동
            enemyController.transform.position = nextPos;
        }
    }



    public void EndChangState()
    {
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
