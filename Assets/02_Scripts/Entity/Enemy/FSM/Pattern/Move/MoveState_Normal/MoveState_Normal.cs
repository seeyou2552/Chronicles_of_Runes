using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveState_Normal : EnemyState
{
    public MoveState_Normal_So data;

    float endStateTime = 0.5f;
    
    bool isChasingAttack;

    PatternDataSo pattern;

    public MoveState_Normal(EnemyController enemyController, StateMachine stateMachine , MoveState_Normal_So data)
        : base(enemyController, stateMachine) 
            {
                this.data = data;
                endStateTime = data.endStateTime;
                isChasingAttack = data.isChasingAttack;
                pattern = data.pattern;
            }


    float lastTime;

    public override void Enter()
    {
        if (enemyController.isDead) return;
        enemyController.aiPath.canMove = true;
        lastTime = Time.time + endStateTime;
    }

    public override void Update()
    {
        if (enemyController.isDead) return;

        if (lastTime < Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
        }


        //추적 완료 후 공격 시킬때
        if (isChasingAttack == true)
        {
            if (enemyController.enemyStat.attackRange * enemyController.enemyStat.attackRange > enemyController.distance)
            {
                stateMachine.ChangeState(enemyController.attackState);
            }
        }

        //그건 아닐때
        else
        {   //공격 리치 벗어나면 수행할거
            if (enemyController.enemyStat.attackRange * enemyController.enemyStat.attackRange < enemyController.distance)
            {
                if (pattern != null && enemyController.FSM[pattern].CheckCoolDown())
                {
                    stateMachine.ChangeState(enemyController.FSM[pattern]);
                }
            }
        }

    }

    public override void Exit()
    {
        enemyController.aiPath.canMove = false;
    }
}