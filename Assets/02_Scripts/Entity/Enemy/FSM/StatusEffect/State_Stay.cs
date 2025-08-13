using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Stay : EnemyState
{
    //복사용

    private float duration;
    public State_Stay(EnemyController enemyController, StateMachine stateMachine, State_Stay_So data) : base(enemyController, stateMachine)
    {
        duration = data.duration;
    }

    float timer;
    public override void Enter()
    {
        timer = duration;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            enemyController.stateMachine.ChangeState(enemyController.idleState);
        }
    }

    public override void Exit()
    {
    }
}
