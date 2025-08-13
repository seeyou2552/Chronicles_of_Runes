using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : EnemyState
{
    MoveState_So data;
    public MoveState(EnemyController enemyController, StateMachine stateMachine, MoveState_So data)
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


        //아래는 업데이트로 옮기는게 좋긴함
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
