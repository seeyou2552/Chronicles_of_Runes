using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyName_CreateShelter", menuName = "Patterns/Boss/Attack/CreateShelter")]
public class PatternState_CreateShelter_So : PatternDataSo
{
    [Header("안전지대;")]
    public GameObject shelter;

    [Header("쿨타임")]
    public float coolDown = 15f;

    [Header("애니메이션")]
    public AnimationType endAnimation;

    [Header("소리")]
    private List<string> SFXKeyList;

    [Header("연계패턴")]
    public PatternDataSo pattern;

    public override System.Type GetStateType()
    {
        return typeof(PatternState_CreateShelter);
    }


    public override EnemyState CreateState(EnemyController controller, StateMachine stateMachine)
    {
        return new PatternState_CreateShelter(controller, stateMachine, this);
    }
}
