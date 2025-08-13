using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyName_Stay", menuName = "Patterns/StateEffect/Stay")]
public class State_Stay_So : PatternDataSo
{

    [Header("지속시간")]
    public float duration = 5f;


    public override System.Type GetStateType()
    {
        return typeof(State_Stay);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new State_Stay(controller, stateMachine, this);
    }
}
