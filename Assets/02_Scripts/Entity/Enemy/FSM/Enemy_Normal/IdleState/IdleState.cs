using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : EnemyState
{
    IdleState_So data;
    public IdleState(EnemyController enemyController, StateMachine stateMachine, IdleState_So data)
        : base(enemyController, stateMachine)
    {
        this.data = data;
    }
    public bool isDelay = false;
    public override void Enter()
    {
        enemyController.animationController.isAttack = false;
        enemyController.isAniOnUpdate = true;
        enemyController.aiPath.canMove = false;

        //가중치에 따라서 패턴 골라오는 부분
        /*
        if (enemyController.isChasing)
        {
            if (isDelay)
            {
                await UniTask.Delay(1000);
                isDelay = false;
            }

            int index = enemyController.WeightedRandomIndex(enemyController.carrierPatternWeights);
            PatternDataSo selectedType = enemyController.carrierCachedPatternTypes[index];

            if (enemyController.FSM.TryGetValue(selectedType, out EnemyState nextState))
            {
                stateMachine.ChangeState(nextState);
            }
        }
        else
        {
            await UniTask.Delay(500);
            stateMachine.ChangeState(enemyController.idleState);
        }
        */
    }

    public override void Update() 
    {
        if (enemyController.isDead) return;


        if (enemyController.canControl)
        {
            //가중치에 따라서 패턴 골라오는 부분
            if (enemyController.isChasing)
            {

                int index = enemyController.WeightedRandomIndex(enemyController.carrierPatternWeights);
                PatternDataSo selectedType = enemyController.carrierCachedPatternTypes[index];

                if (enemyController.FSM.TryGetValue(selectedType, out EnemyState nextState))
                {
                    stateMachine.ChangeState(nextState);
                }
            }
        }
    }
    public override void Exit() { }


}