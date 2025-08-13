using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyName_AttackState", menuName = "Patterns/Boss/Carrier/AttackState")]
public class Boss01AttackState_So : PatternDataSo
{
    public override System.Type GetStateType()
    {
        return typeof(Boss01AttackState);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01AttackState(controller, stateMachine, this);
    }
}
