using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateFactory
{
    private EnemyController controller;
    private StateMachine stateMachine;

    public StateFactory(EnemyController controller, StateMachine stateMachine)
    {
        this.controller = controller;
        this.stateMachine = stateMachine;
    }

    public EnemyState Create(PatternDataSo data)
    {
        return data.CreateState(controller, stateMachine);
    }
}
