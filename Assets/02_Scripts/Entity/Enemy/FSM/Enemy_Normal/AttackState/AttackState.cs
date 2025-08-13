using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : EnemyState
{
    AttackState_So data;
    public AttackState(EnemyController enemyController, StateMachine stateMachine, AttackState_So data)
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
    }

    public override void Update() 
    {
        int index = enemyController.WeightedRandomIndex(enemyController.attackPatternWeights);
        if (index == -1)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }
        PatternDataSo selectedType = enemyController.attackCachedPatternTypes[index];

        if (enemyController.FSM.TryGetValue(selectedType, out EnemyState nextState))
        {
            stateMachine.ChangeState(nextState);
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
