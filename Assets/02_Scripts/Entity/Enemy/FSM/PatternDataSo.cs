using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PatternDataSo : ScriptableObject
{
    [Header("설명 메모장")]
    [Tooltip("메모 적는용도")]
    [TextArea(5, 10)]
    public string Comment;
    [Header("시전 확률 가중치")]
    public int weight = 1;

    [Header("이펙트 및 거리")]
    public GameObject VFX;
    public float rich;

    [Header("사운드 (키값)")]
    public List<string> SFX;

    [Header("투사체(스킬So)")]
    public Skill projectileSo;
    public abstract System.Type GetStateType();

    public abstract EnemyState CreateState(EnemyController controller, StateMachine stateMachine);
}