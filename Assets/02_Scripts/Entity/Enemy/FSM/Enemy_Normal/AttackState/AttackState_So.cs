using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyName_AttackState", menuName = "Patterns/StateCarrier/AttackState")]
public class AttackState_So : PatternDataSo
{
    public override System.Type GetStateType()
    {
        return typeof(AttackState);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new AttackState(controller, stateMachine, this);
    }
}
