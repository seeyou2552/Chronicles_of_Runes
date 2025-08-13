using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class Boss01AttackState : EnemyState
{
    Boss01AttackState_So data;
    public Boss01AttackState(EnemyController enemyController, StateMachine stateMachine, Boss01AttackState_So data)
        : base(enemyController, stateMachine)
    {
        this.data = data;
    }

    public override void Enter()
    {
        if (enemyController.isDead)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }
        //일반 에네미도 해줘야함

    }

    public override void Update() 
    {
        int index = enemyController.WeightedRandomIndex(enemyController.attackPatternWeights);
        if (index == -1)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }
        else
        {
            PatternDataSo selectedType = enemyController.attackCachedPatternTypes[index];

            if (enemyController.FSM.TryGetValue(selectedType, out EnemyState nextState))
            {
                stateMachine.ChangeState(nextState);
            }
        }
    }
    public override void Exit() 
    {
        if (stateMachine.currentState != enemyController.idleState)
        {
            enemyController.isAniOnUpdate = false;
            enemyController.animationController.StopRun();
            enemyController.aiPath.canMove = false;
        }
    }

}
