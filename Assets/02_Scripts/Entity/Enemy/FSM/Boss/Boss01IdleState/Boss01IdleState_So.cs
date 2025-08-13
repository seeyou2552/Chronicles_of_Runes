using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyName_IdleState", menuName = "Patterns/Boss/Carrier/IdleState")]
public class Boss01IdleState_So : PatternDataSo
{
    public override System.Type GetStateType()
    {
        return typeof(Boss01IdleState);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01IdleState(controller, stateMachine, this);
    }
}
