using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine
{
    [SerializeField]
    public EnemyState currentState;

    EnemyController enemyController;
    public string CurrentStateName => currentState?.GetType().Name ?? "None";

    public void Initialize(EnemyState startState, EnemyController controller)
    {
        enemyController = controller;
        currentState = startState;
        currentState.Enter();
    }

    public void ChangeState(EnemyState newState)
    {
        if (enemyController.tweens.Count > 0)
        {
            foreach (var t in enemyController.tweens)
            {
                t.Kill();
            }
            enemyController.tweens.Clear();

        }
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();

    }


    public void Update()
    {
        currentState?.Update();
    }
}
