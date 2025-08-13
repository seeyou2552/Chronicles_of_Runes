using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyName_Boss01PatternState_CornerCast_Data", menuName = "Patterns/Boss/Attack/Boss01PatternState_CornerCast")]

public class Boss01PatternState_CornerCast_So : PatternDataSo
{

    [Header("이동간 걸리는 시간")]
    public float moveDuration = 1.5f;

    [Header("연계할 패턴")]
    public PatternDataSo callBackPattern;

    public override System.Type GetStateType()
    {
        return typeof(Boss01PatternState_CornerCast);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01PatternState_CornerCast(controller, stateMachine, this);
    }
}
