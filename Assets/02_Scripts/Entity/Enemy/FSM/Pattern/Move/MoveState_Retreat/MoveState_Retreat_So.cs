using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyName_MoveState_Retreat", menuName = "Patterns/Boss/Move/MoveState_Retreat")]
public class MoveState_Retreat_So : PatternDataSo
{
    public override System.Type GetStateType()
    {
        return typeof(MoveState_Retreat);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new MoveState_Retreat(controller, stateMachine, this);
    }
}
