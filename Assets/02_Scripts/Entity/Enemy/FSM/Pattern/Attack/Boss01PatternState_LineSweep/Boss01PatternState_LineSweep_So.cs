using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "EnemyName_Boss01PatternState_LineSweep_Data", menuName = "Patterns/Boss/Attack/Boss01PatternState_LineSweep")]

public class Boss01PatternState_LineSweep_So : PatternDataSo
{
    [Header("룬 리스트")]
    public List<Rune> runeList;

    [Header("반복횟수")]
    public int cnt = 3;

    [Header("발사갯수")]
    public int shotCnt = 7;

    [Header("발사 딜레이")]
    public float Delay = 1f;

    [Header("탄속")]
    public float shotSpeed = 15f;

    [Header("시전 사운드")]
    public string castSFX;

    [Header("전조현상")]
    public float warningDuration = 0.5f;
    public float warningLength = 30f;
    public float warningWidth = 0.1f;

    [Header("사용할 애니메이션")]
    public AnimationType endAnimation;

    [Header("연계할 패턴")]
    public PatternDataSo callBackPattern;

    [Header("시전중 랜덤 특수패턴")]
    public PatternDataSo specialPattern;

    [Header("콜백메서드")]
    public UnityEvent OnStarted;
    //public UnityEvent OnUpdate;
    public UnityEvent OnFinished;

    [Header("쿨타임")]
    public float coolDown = 5f;

    public override System.Type GetStateType()
    {
        return typeof(Boss01PatternState_LineSweep);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new Boss01PatternState_LineSweep(controller, stateMachine, this);
    }
}
