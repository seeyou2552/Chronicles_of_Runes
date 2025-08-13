using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "EnemyName_MoveState_Strafing", menuName = "Patterns/Boss/Move/MoveState_Strafing")]
public class MoveState_Strafing_So : PatternDataSo
{


    [Header("이동 시간")]
    public float minEndStateTime = 0.2f;
    public float maxEndStateTime = 1.0f;

    public override System.Type GetStateType()
    {
        return typeof(MoveState_Strafing);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new MoveState_Strafing(controller, stateMachine, this);
    }
}
