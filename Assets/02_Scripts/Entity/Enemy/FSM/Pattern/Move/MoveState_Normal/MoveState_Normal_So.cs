using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyName_MoveState_Normal", menuName = "Patterns/Boss/Move/MoveState_Normal")]
public class MoveState_Normal_So : PatternDataSo
{
    public float endStateTime = 0.5f;
    public bool isChasingAttack = false;

    public PatternDataSo pattern;

    public override System.Type GetStateType()
    {
        return typeof(MoveState_Normal);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new MoveState_Normal(controller, stateMachine, this);
    }
}
