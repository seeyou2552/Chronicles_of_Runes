using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyName_MoveState_Random", menuName = "Patterns/Boss/Move/MoveState_Random")]
public class MoveState_Random_So : PatternDataSo
{
    [Header("최대 이동 시간")]
    public float endStateTime = 1.5f;

    [Header("목적지 범위")]
    public float radius = 5f;
    public override System.Type GetStateType()
    {
        return typeof(MoveState_Random);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new MoveState_Random(controller, stateMachine, this);
    }
}
