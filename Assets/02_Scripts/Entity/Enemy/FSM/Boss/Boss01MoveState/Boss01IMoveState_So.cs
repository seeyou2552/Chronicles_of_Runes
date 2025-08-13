using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyName_MoveState", menuName = "Patterns/Boss/Carrier/MoveState")]
public class Boss01IMoveState_So : PatternDataSo
{
    public override System.Type GetStateType()
    {
        return typeof(Boss01MoveState);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01MoveState(controller, stateMachine, this);
    }
}
