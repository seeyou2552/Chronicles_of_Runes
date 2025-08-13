using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyName_IdleState", menuName = "Patterns/StateCarrier/IdleState")]
public class IdleState_So : PatternDataSo
{
    public override System.Type GetStateType()
    {
        return typeof(IdleState);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new IdleState(controller, stateMachine, this);
    }
}
