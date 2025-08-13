using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyName_MoveState", menuName = "Patterns/StateCarrier/MoveState")]
public class MoveState_So : PatternDataSo
{
    public override System.Type GetStateType()
    {
        return typeof(MoveState);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new MoveState(controller, stateMachine, this);
    }
}
