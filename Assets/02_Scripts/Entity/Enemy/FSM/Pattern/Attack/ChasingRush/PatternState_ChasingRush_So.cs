using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "EnemyName_ChasingRush", menuName = "Patterns/Boss/Attack/ChasingRush")]
public class PatternState_ChasingRush_So : PatternDataSo
{

    [Header("바라보는 시간")]
    public float lookDuration;

    [Header("돌진 스펙")]
    public float dashDuration = 5f; //돌진 시간
    public float dashDistance = 10f;
    public float dashWidth = 3f;
    public float dashSpeed = 5f;

    [Header("선후딜")]
    public float firstDelay = 0.5f; //선딜
    public float lastDelay = 0.15f; //후딜

    [Header("데미지 판정")]
    public bool isBodySlam;
    public bool isEffectAttack;

    [Header("쿨타임")]
    public float coolDown = 5f;

    [Header("애니메이션")]
    public AnimationType endAnimation;

    [Header("콜백패턴")]
    public PatternDataSo callBackPatternSo;

    [Header("콜백메서드")]
    public UnityEvent OnStarted;
    //public UnityEvent OnUpdate;
    public UnityEvent OnFinished;

    [Header("소리")]
    public string crashSFX;


    public override System.Type GetStateType()
    {
        return typeof(PatternState_ChasingRush);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new PatternState_ChasingRush(controller, stateMachine, this);
    }
}
