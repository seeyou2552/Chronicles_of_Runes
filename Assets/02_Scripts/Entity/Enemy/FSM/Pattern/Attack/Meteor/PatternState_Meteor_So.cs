using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "EnemyName_Meteor", menuName = "Patterns/Boss/Attack/Meteor")]
public class PatternState_Meteor_So : PatternDataSo
{

    [Header("룬 리스트")]
    public List<Rune> runeList;


    [Header("횟수")]
    public int cnt = 1; //횟수

    [Header("스펙")]
    public float fallDistance = 5f; //낙하 거리
    public float fallDuration = 0.3f; //낙하 시간
    public float radius = 3f; //반지름

    [Header("선후딜")]
    public float firstDelay = 1.5f; //선딜
    public float lastDelay = 0.15f; //후딜

    [Header("타게팅 여부")]
    public bool isTargeting = false;
    public bool isCenterTarget = false;

    [Header("쿨타임")]
    public float coolDown = 5f;

    [Header("애니메이션")]
    public AnimationType endAnimation;

    [Header("소리")]
    public string castSFX;

    [Header("연계할 패턴")]
    public PatternDataSo callBackPattern;

    [Header("콜백메서드")]
    public UnityEvent OnStarted;
    //public UnityEvent OnUpdate;
    public UnityEvent OnFinished;

    public override System.Type GetStateType()
    {
        return typeof(PatternState_Meteor);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new PatternState_Meteor(controller, stateMachine, this);
    }
}
