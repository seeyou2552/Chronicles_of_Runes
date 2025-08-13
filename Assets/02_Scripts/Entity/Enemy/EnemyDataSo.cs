using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "DataSo/EnemyData")]
public class EnemyDataSo : ScriptableObject
{
    public string enemyName;
    public string displayName;
    public bool isBoss;

    public float hp;
    public float speed;

    public float safeDistance = 5f;

    public GameObject enemyPrefab;

    public float chaseRange;
    public float attackPower;
    public float attackRange;

    public List<DropEntry> dropTable;

    [Header("드랍안될 가중치 추가(투표권)")]
    public int NoDropWeight;
}

[System.Serializable]
public class DropEntry
{
    [Header("드랍아이템")]
    public ItemData itemData;
    [Header("드랍 확률%")]
    [Range(0f, 100f)]
    public float dropChance;
}