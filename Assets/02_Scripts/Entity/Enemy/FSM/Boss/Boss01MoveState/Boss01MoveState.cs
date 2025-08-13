using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Boss01MoveState : EnemyState
{
    Boss01IMoveState_So data;
    public Boss01MoveState(EnemyController enemyController, StateMachine stateMachine, Boss01IMoveState_So data)
        : base(enemyController, stateMachine)
    {
        this.data = data;
    }

    public override void Enter()
    {
        if (enemyController.isDead) return;
        enemyController.aiPath.canMove = false;

        int index = enemyController.WeightedRandomIndex(enemyController.movePatternWeights);
        if (index == -1)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }
        PatternDataSo selectedType = enemyController.moveCachedPatternTypes[index];

        if (enemyController.FSM.TryGetValue(selectedType, out EnemyState nextState))
        {
            stateMachine.ChangeState(nextState);
        }
    }

    public override void Update() { }
    public override void Exit() { }


}
