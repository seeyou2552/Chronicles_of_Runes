using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss01IdleState : EnemyState
{
    Boss01IdleState_So data;
    public Boss01IdleState(EnemyController enemyController, StateMachine stateMachine, Boss01IdleState_So data)
        : base(enemyController, stateMachine)
    {
        this.data = data;
    }

    public override  void Enter()
    {
        enemyController.animationController.isAttack = false;
        enemyController.isAniOnUpdate = true;
        enemyController.aiPath.canMove = false;
    }

    public override void Update() 
    {
        if (enemyController.isDead) return;

        //여기서 큰갈래로 보내줄듯
        /*
        stateMachine.ChangeState(enemyController.FSM[typeof(Boss01AttackState)]);
        stateMachine.ChangeState(enemyController.FSM[typeof(Boss01MoveState)]);
        */
        //가중치에 따라서 패턴 골라오는 부분

        if (enemyController.canControl)
        {
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

