using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState_Strafing : EnemyState
{

    float minEndStateTime = 0.2f;
    float maxEndStateTime = 1.0f;

    public MoveState_Strafing(EnemyController enemyController, StateMachine stateMachine, MoveState_Strafing_So data) : base(enemyController, stateMachine)
    {
        minEndStateTime = data.minEndStateTime;
        maxEndStateTime = data.maxEndStateTime;
    }

    float endStateTime;
    float lastTime;
    Vector2 randomPos;

    float minStrafeDistance = 1.0f;
    float maxStrafeDistance = 5.0f;

    public override void Enter()
    {
        if (enemyController.isDead) return;
        enemyController.aiPath.canMove = true;
        enemyController.setter.target = enemyController.targetPos;

        endStateTime = Random.Range(minEndStateTime, maxEndStateTime);
        lastTime = Time.time + endStateTime;

        // 정면 방향(타겟 기준)
        Vector2 forward = (enemyController.target.position - enemyController.transform.position).normalized;

        // 좌우 방향
        Vector2 right = new Vector2(forward.y, -forward.x);

        // 좌우 중 랜덤 (-1 = left, 1 = right)
        float dir = Random.value < 0.5f ? -1f : 1f;

        // 랜덤 거리
        float distance = Random.Range(minStrafeDistance, maxStrafeDistance);

        // 45도 기울어진 방향 (전진 + 측면)
        Vector2 moveDir = (forward + right * dir).normalized;

        // 최종 목표 위치
        randomPos = (Vector2)enemyController.transform.position + moveDir * distance;
        enemyController.targetPos.position = randomPos;
    }

    public override void Update()
    {
        if (enemyController.isDead) return;

        if (lastTime < Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        if (((Vector3)randomPos - enemyController.transform.position).sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(enemyController.idleState);
        }
    }

    public override void Exit()
    {
        enemyController.aiPath.canMove = false;
        enemyController.setter.target = enemyController.target;
    }
}
