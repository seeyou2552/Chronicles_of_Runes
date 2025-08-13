using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "EnemyName_RandomAimedAttack_Data", menuName = "Patterns/Boss/Attack/RandomAimedAttack")]
public class PatternState_RandomAimedAttack_So : PatternDataSo
{
    [Header("룬 리스트")]
    public List<Rune> runeList;

    [Header("스킬")]
    public GameObject prefab;

    [Header("랜덤 스폰 반경(반지름)")]
    public float radius;

    [Header("선후딜")]
    public float firstDelay = 0.75f; //선딜
    public float lastDelay = 0.25f; //후딜

    [Header("탄속")]
    public float shotSpeed = 15f;

    [Header("전조이펙트")]
    public float warningLength = 40f;
    public float warningWidth = 0.2f;

    public float Ratio = 1f;

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
        return typeof(PatternState_RandomAimedAttack);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new PatternState_RandomAimedAttack(controller, stateMachine, this);
    }
}
