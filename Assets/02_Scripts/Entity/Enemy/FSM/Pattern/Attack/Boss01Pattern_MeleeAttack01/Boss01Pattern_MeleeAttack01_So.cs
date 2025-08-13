using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "EnemyName_MeleeAttack01_Data", menuName = "Patterns/Boss/Attack/MeleeAttack01")]
public class Boss01Pattern_MeleeAttack01_So : PatternDataSo
{
    [Header("대쉬횟수")]
    public int dashCnt = 3; //횟수

    [Header("돌진 스펙")]
    public float dashDistance = 5f; //돌진 거리
    public float dashDuration = 0.3f; //돌진 시간
    public float dashWidth = 3f;

    [Header("선후딜")]
    public float firstDelay = 0.5f; //선딜
    public float lastDelay = 0.15f; //후딜

    public float Ratio = 1f;
    [Header("마지막 시전시간 배율")]
    public float lastRatio = 2f;

    [Header("데미지 판정")]
    public bool isBodySlam;
    public bool isEffectAttack;

    [Header("쿨타임")]
    public float coolDown = 0f;

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
        return typeof(Boss01PatternState_MeleeAttack01);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01PatternState_MeleeAttack01(controller, stateMachine, this);
    }
}
