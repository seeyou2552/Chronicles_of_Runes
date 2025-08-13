using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss01PatternState : EnemyState
{
    private List<Func<EnemyState>> StateFactories;
    private List<int> weights;
    private System.Random rng = new System.Random();

    public Boss01PatternState(EnemyController enemyController, StateMachine stateMachine)
        : base(enemyController, stateMachine)
    {

        StateFactories = new List<Func<EnemyState>>()
        {
            //() => new Boss01PatternState_CornerCast(enemyController, stateMachine),
            //() => new AttackState(enemyController, stateMachine),

        };

        weights = new List<int>() { 3, 1 };
    }

    public override void Enter()
    {
        enemyController.aiPath.canMove = false;

        int totalWeight = 0;
        foreach (var w in weights)
        {
            totalWeight += w;
        }

        int randomValue = rng.Next(totalWeight);
        int cumulative = 0;
        int selectedIndex = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += weights[i];
            if (randomValue < cumulative)
            {
                selectedIndex = i;
                break;
            }
        }

        EnemyState nextState = StateFactories[selectedIndex]();
        stateMachine.ChangeState(nextState);
    }

    public override void Update()
    {

    }

    public override void Exit()
    {

    }
}
