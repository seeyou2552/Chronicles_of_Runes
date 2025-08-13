using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyName_Boss01PatternState_CircleAttack_Data", menuName = "Patterns/Boss/Attack/Boss01PatternState_CircleAttack")]
public class Boss01PatternState_CircleAttack_So : PatternDataSo
{
    [Header("반복 횟수")]
    public int cnt = 3;
    [Header("투사체 갯수")]
    public int shotCnt = 15;

    [Header("투사체 스폰 크기")]
    public float radius = 3f;

    [Header("발사간격이던가 단위 ms")]
    public int installIntervalDelay = 10;

    [Header("연사속도 단위ms")]
    public int fireRate = 400;

    [Header("연계할 패턴")]
    public PatternDataSo callBackPattern;

    public override System.Type GetStateType()
    {
        return typeof(Boss01PatternState_CircleAttack);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01PatternState_CircleAttack(controller, stateMachine, this);
    }

}
