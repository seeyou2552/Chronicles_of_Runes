using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class EnemyStat
{
    public string enemyName;
    public string displayName;
    public bool isBoss;

    public float maxHp;
    public float curHp;
    public float speed;

    public float safeDistance;

    public GameObject enemyPrefab;

    public float chaseRange;
    public float attackPower;
    public float attackRange;

    public List<DropEntry> dropTable;
    public int NoDropWeight;
}
