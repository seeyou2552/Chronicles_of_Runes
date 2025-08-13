using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "EnemyName_ChargeExplosion", menuName = "Patterns/Boss/Attack/ChargeExplosion")]
public class PatternState_ChargeExplosion_So : PatternDataSo
{
    [Header("크기")]
    public float radius = 5f;

    [Header("선후딜")]
    public float firstDelay = 3f; //선딜
    public float lastDelay = 0f; //후딜

    [Header("자폭여부")]
    public bool isSelfDestruct = false;

    [Header("쿨타임")]
    public float coolDown = 5f;

    [Header("애니메이션")]
    public AnimationType endAnimation;


    [Header("연계할 패턴")]
    public PatternDataSo callBackPattern;

    [Header("콜백메서드")]
    public UnityEvent OnStarted;
    //public UnityEvent OnUpdate;
    public UnityEvent OnFinished;


    public override System.Type GetStateType()
    {
        return typeof(PatternState_ChargeExplosion);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new PatternState_ChargeExplosion(controller, stateMachine, this);
    }
}
