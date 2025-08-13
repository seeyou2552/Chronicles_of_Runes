using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState_Retreat : EnemyState
{
    MoveState_Retreat_So data;
    public MoveState_Retreat(EnemyController enemyController, StateMachine stateMachine, MoveState_Retreat_So data)
        : base(enemyController, stateMachine) 
    
    { 
           this.data = data;
    }



    float endStateTime = 2f;
    float lastTime;
    public override void Enter()
    {
        if (enemyController.isDead) return;
        enemyController.aiPath.canMove = true;
        enemyController.setter.target = enemyController.targetPos;

        Vector3 dirFromTarget = (enemyController.transform.position - enemyController.target.position).normalized;
        Vector3 destination = enemyController.transform.position + dirFromTarget * 5f;

        enemyController.targetPos.position = destination;

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
        if (enemyController.aiPath.reachedDestination)
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
