using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState
{
    protected EnemyController enemyController;
    protected StateMachine stateMachine;

    public float afterCoolTime;
    public EnemyState(EnemyController enemyController, StateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    public bool CheckCoolDown()
    {
        if(afterCoolTime < Time.time)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}