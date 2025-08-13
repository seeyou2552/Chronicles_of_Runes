using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "EnemyName_AimedAttack_Data", menuName = "Patterns/Boss/Attack/AimedAttack")]
public class Boss01PatternState_AimedAttack_So : PatternDataSo
{
    [Header("룬 리스트")]
    public List<Rune> runeList;

    [Header("횟수")]
    public int cnt = 10; //횟수

    [Header("스킬")]
    public GameObject prefab;

    [Header("선후딜")]
    public float firstDelay = 0.75f; //선딜
    public float lastDelay = 0.25f; //후딜

    [Header("탄속")]
    public float shotSpeed = 15f;

    [Header("다음 시전시 선후딜 대기할건지")]
    public bool isDelayStay = true;

    [Header("전조이펙트")]
    public float warningLength = 30f;
    public float warningWidth = 0.2f;

    [Header("시전 사운드")]
    public string castSFX;


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
        return typeof(Boss01PatternState_AimedAttack);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01PatternState_AimedAttack(controller, stateMachine, this);
    }
}
