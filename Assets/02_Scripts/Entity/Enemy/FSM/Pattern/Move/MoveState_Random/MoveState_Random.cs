using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState_Random : EnemyState
{
    MoveState_Random_So data;
    float endStateTime;
    float radius;

    public MoveState_Random(EnemyController enemyController, StateMachine stateMachine, MoveState_Random_So data)
        : base(enemyController, stateMachine) 
    { 
        this.data = data;
        endStateTime = data.endStateTime;
        radius = data.radius;
    }


    Vector2 randomPos;

    float lastTime;
    public override void Enter()
    {
        if (enemyController.isDead) return;
        enemyController.aiPath.canMove = true;
        enemyController.setter.target = enemyController.targetPos;
        randomPos = (Vector2)enemyController.transform.position + Random.insideUnitCircle * radius;
        enemyController.targetPos.position = randomPos;

        lastTime = Time.time + endStateTime;
    }

    public override void Update()
    {
        if (enemyController.isDead) return;
        if (lastTime < Time.time) 
        {
            lastTime = Time.time + endStateTime;
            stateMachine.ChangeState(enemyController.idleState);
        }
        if (((Vector3)randomPos - enemyController.transform.position).sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(enemyController.idleState);
        }
        //
    }

    public override void Exit()
    {
        enemyController.aiPath.canMove = false;
        enemyController.setter.target = enemyController.target;
    }
}
